using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Net;


/// <summary>
/// net manager of client, 
/// use asynchronous methods instead of blocking mathods
/// </summary>
public static class NetManager
{
    #region socket, buffer and queue
    static UdpClient udpClient;
    const int udpPort = 6666;
    const int serverPort = 7777;
    public static string serverIP;
    static System.Threading.Thread udpReceiveThread;
    public static System.Action<string> onReceiveUDP;

    static Socket socket;
    static ByteArray receiveBuffer;
    static Queue<ByteArray> sendQueue;
    #endregion

    #region trigger whether is connecting or is closing the socket
    static bool isConnecting;
    static bool isClosing;
    #endregion

    #region the list of messages
    static List<MsgBase> msgList;
    static int msgCount;
    const int MAX_MESSAGE_FIRE = 10;
    #endregion

    #region whehter still connect (by ping)
    public static bool isUsePing;
    public static int pingInterval = 30;
    static float lastPingTime = 0;
    static float lastPongTime = 0;
    #endregion

    #region delegate and listener of event and message of network
    // event
    public enum EventType
    {
        connectSuccess,
        connectFail,
        close,
    }
    public delegate void EventListener(string str);
    static Dictionary<EventType, EventListener> eventListenerDic = new Dictionary<EventType, EventListener>();
    public static void AddEventListener(EventType et, EventListener el)
    {
        if (eventListenerDic.ContainsKey(et))
        {
            eventListenerDic[et] += el;
        }
        else
        {
            eventListenerDic[et] = el;
        }
    }

    public static void RemoveEventListener(EventType et, EventListener el)
    {
        if (eventListenerDic.ContainsKey(et))
        {
            eventListenerDic[et] -= el;
        }
    }

    public static void FireEvent(EventType et, string str)
    {
        if (eventListenerDic.ContainsKey(et))
        {
            eventListenerDic[et](str);
        }
    }

    //message
    public delegate void MsgListener(MsgBase msg);
    static Dictionary<string, MsgListener> msgListenerDic = new Dictionary<string, MsgListener>();
    public static void AddMsgListener(string str, MsgListener ml)
    {
        if (msgListenerDic.ContainsKey(str))
        {
            msgListenerDic[str] += ml;
        }
        else
        {
            msgListenerDic[str] = ml;
        }
    }

    public static void RemoveMSGlistener(string str, MsgListener ml)
    {
        if (msgListenerDic.ContainsKey(str))
        {
            msgListenerDic[str] -= ml;
        }
    }

    public static void FireMsg(string str, MsgBase mb)
    {
        if (msgListenerDic.ContainsKey(str))
        {
            msgListenerDic[str](mb);
        }
    }
    #endregion

    #region socket method (asynchronous tcp)
    public static void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
        {
            Debug.LogWarning("Connect fail, already connected");
            return;
        }
        if (isConnecting)
        {
            Debug.LogWarning("Connect fail, is connecting");
            return;
        }

        InitialNetManager();

        //do not use nagle
        socket.NoDelay = true;
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }

    static void InitialNetManager()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        receiveBuffer = new ByteArray();
        sendQueue = new Queue<ByteArray>();

        isClosing = false;
        isConnecting = false;

        msgList = new List<MsgBase>();
        msgCount = 0;

        isUsePing = true;
        lastPingTime = Time.time;
        lastPongTime = Time.time;
        if (!msgListenerDic.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
        }
    }

    static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket skt = ar.AsyncState as Socket;
            skt.EndConnect(ar);
            Debug.Log("Socket connect success");
            FireEvent(EventType.connectSuccess, "");
            isConnecting = false;

            socket.BeginReceive(receiveBuffer.bytes, receiveBuffer.writeIndex, receiveBuffer.Remain, 0, ReceiveCallback, skt);

        }
        catch (SocketException ex)
        {
            Debug.Log("Socket connect fail: " + ex.ToString());
            FireEvent(EventType.connectFail, ex.ToString());
            isConnecting = false;
        }
    }

    static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket skt = ar.AsyncState as Socket;

            int count = skt.EndReceive(ar);
            receiveBuffer.writeIndex += count;

            //process data
            OnReceivingData();

            if (receiveBuffer.Remain < 8)
            {
                receiveBuffer.Move();
                receiveBuffer.Resize(receiveBuffer.Length * 2);
            }

            skt.BeginReceive(receiveBuffer.bytes, receiveBuffer.writeIndex, receiveBuffer.Remain, 0, ReceiveCallback, skt);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket receive fail" + ex.ToString());
        }
    }

    static void OnReceivingData()
    {
        if (receiveBuffer.Length <= 2)
        {
            return;
        }

        int readIndex = receiveBuffer.readIndex;
        byte[] bytes = receiveBuffer.bytes;
        Int16 bodyLength = (Int16)((bytes[readIndex + 1] << 8) | bytes[readIndex]);
        if (receiveBuffer.Length < bodyLength + 2)
            return;
        receiveBuffer.readIndex += 2;
        //decode name
        string msgName = MsgBase.DecodeName(receiveBuffer.bytes, receiveBuffer.readIndex, out int nameCount);
        if (msgName == "")
        {
            Debug.LogError("On receiving data, Decode name fail");
            return;
        }
        receiveBuffer.readIndex += nameCount;

        //decode body
        int bodyCount = bodyLength - nameCount;
        MsgBase msg = MsgBase.Decode(msgName, receiveBuffer.bytes, receiveBuffer.readIndex, bodyCount);
        receiveBuffer.readIndex += bodyCount;
        receiveBuffer.CheckAndMove();
        lock (msgList)
        {
            msgList.Add(msg);
            msgCount++;
        }
        if (receiveBuffer.Length > 2)
        {
            OnReceivingData();
        }
    }

    public static void Close()
    {
        UdpClose();

        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }

        if (sendQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
            FireEvent(EventType.close, "");
            isClosing = false;
        }
    }

    public static void Send(MsgBase msg)
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting || isClosing)
        {
            return;
        }

        //encode data
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;

        byte[] sendBytes = new byte[len + 2];
        //record the length of each package,  
        //while 0 is low byte and 1 is high byte
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        //add data into dqueue to send it in callback
        lock (sendQueue)
        {
            sendQueue.Enqueue(ba);
            count = sendQueue.Count;
        }
        if (count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }

    static void SendCallback(IAsyncResult ar)
    {
        Socket skt = ar.AsyncState as Socket;
        if (skt == null || !skt.Connected)
        {
            return;
        }
        int count = skt.EndSend(ar);

        //dequeue data from send queue if it has been send entirely,
        //else continue send data, until queue is empty
        ByteArray ba = null;
        lock (sendQueue)
        {
            ba = sendQueue.Peek();
        }
        ba.readIndex += count;
        if (ba.Length == 0)
        {
            lock (sendQueue)
            {
                sendQueue.Dequeue();
                if (sendQueue.Count > 0)
                {
                    ba = sendQueue.Peek();
                }
                else
                {
                    ba = null;
                }
            }
        }

        if (ba != null)
        {
            skt.BeginSend(ba.bytes, ba.readIndex, ba.Length, 0, SendCallback, skt);
        }
        //after send all data, if system call close function, close socket
        else if (isClosing)
        {
            socket.Close();
            isClosing = false;
        }
    }
    #endregion

    #region socket method (thread udp)
    public static void UdpOpen()
    {
        udpClient = new UdpClient(udpPort);
        udpReceiveThread = new System.Threading.Thread(UDPReceiveThread);
        udpReceiveThread.Start();
    }
    public static void UdpClose()
    {
        udpReceiveThread?.Abort();
        udpClient?.Close();
        udpClient = null;
    }

    public static void UdpSend(string udpSendString)
    {
        var udpStringToBytes = System.Text.Encoding.UTF8.GetBytes(udpSendString);
        udpClient.Send(udpStringToBytes, udpStringToBytes.Length, new IPEndPoint(IPAddress.Broadcast, serverPort));
    }

    static void UDPReceiveThread()
    {
        IPEndPoint remotePoint = new IPEndPoint(IPAddress.Any, serverPort); // any

        while (true)
        {
            try
            {
                byte[] receiveBytes = udpClient.Receive(ref remotePoint);
                // block here ------------
                string udpReceiveString = System.Text.Encoding.ASCII.GetString(receiveBytes);
                serverIP = remotePoint.Address.ToString();
                // do something
                onReceiveUDP?.Invoke(udpReceiveString);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.ConnectionReset)
                {
                    return;
                }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
        }
    }
    #endregion

    #region update method
    public static void Update()
    {
        MsgUpdate();
        //PingUpdate();
    }

    public static void MsgUpdate()
    {
        if (msgCount == 0)
        {
            return;
        }
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
            MsgBase msg = null;
            lock (msgList)
            {
                if (msgList.Count > 0)
                {
                    msg = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }
            if (msg != null)
            {
                FireMsg(msg.msgName, msg);
            }
            else
            {
                break;
            }
        }
    }

    public static void PingUpdate()
    {
        if (!isUsePing)
        {
            return;
        }
        if (Time.time - lastPingTime > pingInterval)
        {
            MsgPing msgPing = new MsgPing();
            Send(msgPing);
            lastPingTime = Time.time;
            Debug.Log("Send ping at: " + lastPingTime);
        }
        if (Time.time - lastPongTime > 4 * pingInterval)
        {
            Debug.Log("Close");
            Close();
        }
    }

    private static void OnMsgPong(MsgBase msg)
    {
        Debug.Log("Receive pong");
        lastPongTime = Time.time;
    }

    #endregion



   
}
