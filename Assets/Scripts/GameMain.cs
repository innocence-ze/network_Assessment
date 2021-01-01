using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    public static string id = "";

    public string udpChickSendString = "Tank";
    Coroutine serverFindingCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        Application.wantsToQuit += OnQuitting;
        StartCoroutine(MainThreadCoroutine());
        NetManager.AddEventListener(NetManager.EventType.close, OnConnectClose);
        NetManager.UdpOpen();
        NetManager.onReceiveUDP += OnFindServer;
        serverFindingCoroutine = StartCoroutine(FindServerIp());
    }


    IEnumerator FindServerIp()
    {
        while (true)
        {
            NetManager.UdpSend(udpChickSendString);
            yield return new WaitForSeconds(1.0f);
        }
    }
    void OnFindServer(string udpString)
    {
        if (udpString != udpChickSendString) return;
        CallMainThread(() =>
        {
            NetManager.onReceiveUDP -= OnFindServer;
            NetManager.UdpClose();
            StopCoroutine(serverFindingCoroutine);
            PanelManager.Init();
            BattleManager.Init();
            PanelManager.Open<LoginPanel>();
        });
    }

    bool OnQuitting()
    {
        NetManager.Close();
        return true;
    }

    private void OnConnectClose(string str)
    {
        Debug.Log("Connect close");
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();
    }


    // Threading management
    System.Action mainThreadAction;
    public void CallMainThread(System.Action action)
    {
        mainThreadAction += action;
    }
    IEnumerator MainThreadCoroutine()
    {
        while (true)
        {
            yield return 0;
            mainThreadAction?.Invoke();
            mainThreadAction = null;
        }
    }
}
