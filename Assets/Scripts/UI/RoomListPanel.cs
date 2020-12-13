using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanel : BasePanel
{
    Text idText;
    Button createButton;
    Button reflashButton;

    Transform content;
    GameObject roomObj;

    public override void OnInit()
    {
        skinPath = "RoomListPanel";
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] arg)
    {
        idText = skin.transform.Find("InfoPanel/IdText").GetComponent<Text>();
        createButton = skin.transform.Find("CtrlPanel/CreateButton").GetComponent<Button>();
        reflashButton = skin.transform.Find("CtrlPanel/ReflashButton").GetComponent<Button>();
        content = skin.transform.Find("ListPanel/Scroll View/Viewport/Content");
        roomObj = skin.transform.Find("Room").gameObject;

        roomObj.SetActive(false);
        idText.text = GameMain.id;

        createButton.onClick.AddListener(OnCreateClick);
        reflashButton.onClick.AddListener(OnReflashClick);

        NetManager.AddMsgListener("MsgGetRoomList", OnMsgGetRoomList);
        NetManager.AddMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.AddMsgListener("MsgEnterRoom", OnMsgEnterRoom);

        MsgGetRoomList msg = new MsgGetRoomList();
        NetManager.Send(msg);

    }

    public override void OnClose()
    {
        NetManager.RemoveMSGlistener("MsgGetRoomList", OnMsgGetRoomList);
        NetManager.RemoveMSGlistener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.RemoveMSGlistener("MsgEnterRoom", OnMsgEnterRoom);
    }

    private void OnMsgEnterRoom(MsgBase msgBase)
    {
        MsgEnterRoom msg = msgBase as MsgEnterRoom;
        if(msg.result == 0)
        {
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("Fail to enter the room");
        }
    }

    private void OnMsgCreateRoom(MsgBase msgBase)
    {
        MsgCreateRoom msg = msgBase as MsgCreateRoom;
        if(msg.result == 0)
        {
            PanelManager.Open<TipPanel>("Create room successfully");
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("failed to Create room ");
        }
    }

    private void OnMsgGetRoomList(MsgBase msgBase)
    {
        MsgGetRoomList msg = msgBase as MsgGetRoomList;

        for(int i = content.childCount - 1; i >=0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        if (msg.rooms == null)
        {
            return;
        }
        for(int i = 0; i < msg.rooms.Length; i++)
        {
            GenerateRoom(msg.rooms[i]);
        }
    }

    void GenerateRoom(RoomInfo roomInfo)
    {
        GameObject o = Instantiate(roomObj);
        o.transform.SetParent(content);
        o.SetActive(true);
        o.transform.localPosition = Vector3.one;

        Transform trans = o.transform;
        Text idText = trans.Find("IdText").GetComponent<Text>();
        Text countText = trans.Find("CountText").GetComponent<Text>();
        Text statusText = trans.Find("StatusText").GetComponent<Text>();
        Button btn = trans.Find("JoinButton").GetComponent<Button>();

        idText.text = roomInfo.id.ToString();
        countText.text = roomInfo.count.ToString();
        if(roomInfo.status == 0)
        {
            statusText.text = "Ready";
        }
        else
        {
            statusText.text = "Fight";
        }

        btn.name = idText.text;
        btn.onClick.AddListener(delegate () { OnJoinClick(btn.name); });
        
    }

    private void OnJoinClick(string name)
    {
        MsgEnterRoom msg = new MsgEnterRoom
        {
            id = int.Parse(name),
        };
        NetManager.Send(msg);

    }

    private void OnReflashClick()
    {
        MsgGetRoomList msg = new MsgGetRoomList();
        NetManager.Send(msg);
    }

    private void OnCreateClick()
    {
        MsgCreateRoom msg = new MsgCreateRoom();
        NetManager.Send(msg);
    }


}
