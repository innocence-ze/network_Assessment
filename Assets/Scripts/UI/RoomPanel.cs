using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : BasePanel
{
    Button startButton;
    Button closeButton;

    Transform content;
    GameObject playerObj;

    public override void OnInit()
    {
        skinPath = "RoomPanel";
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] arg)
    {
        startButton = skin.transform.Find("CtrlPanel/StartButton").GetComponent<Button>();
        closeButton = skin.transform.Find("CtrlPanel/CloseButton").GetComponent<Button>();
        content = skin.transform.Find("ListPanel/Scroll View/Viewport/Content");
        playerObj = skin.transform.Find("Player").gameObject;

        playerObj.SetActive(false);

        startButton.onClick.AddListener(OnStartClick);
        closeButton.onClick.AddListener(OnCloseClick);

        NetManager.AddMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.AddMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.AddMsgListener("MsgStartBattle", OnMsgStartBattle);

        MsgGetRoomInfo msg = new MsgGetRoomInfo();
        NetManager.Send(msg);
    }

    public override void OnClose()
    {
        NetManager.RemoveMSGlistener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.RemoveMSGlistener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.RemoveMSGlistener("MsgStartBattle", OnMsgStartBattle);
    }

    private void OnMsgStartBattle(MsgBase msgbase)
    {
        MsgStartBattle msg = msgbase as MsgStartBattle;
        if(msg.result == 0)
        {
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("Start fail! Only room owner can start battle and two teams both need at least one player");
        }
    }

    private void OnMsgLeaveRoom(MsgBase msgbase)
    {
        MsgLeaveRoom msg = msgbase as MsgLeaveRoom;

        if(msg.result == 0)
        {
            PanelManager.Open<TipPanel>("Exit Room");
            PanelManager.Open<RoomListPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("Fail to exit room");
        }
    }

    //TODO
    private void OnMsgGetRoomInfo(MsgBase msgbase)
    {
        MsgGetRoomInfo msg = msgbase as MsgGetRoomInfo;
        for(int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        if(msg.players == null || msg.players.Length == 0)
        {
            return;
        }
        for(int i = 0; i < msg.players.Length; i++)
        {
            GeneratePlayerInfo(msg.players[i]);
        }
    }

    //TODO
    private void GeneratePlayerInfo(PlayerInfo playerInfo)
    {
        GameObject o = Instantiate(playerObj);
        o.transform.SetParent(content);
        o.SetActive(true);
        o.transform.localPosition = Vector3.one;

        Transform trans = o.transform;
        Text idText = trans.Find("IdText").GetComponent<Text>();
        Text CampText = trans.Find("CampText").GetComponent<Text>();

        idText.text = playerInfo.id;
        if(playerInfo.camp == 1)
        {
            CampText.text = "Red";
        }
        else
        {
            CampText.text = "Blue";
        }

        if(playerInfo.isOwner == 1)
        {
            CampText.text += "!";
        }
    }

    private void OnCloseClick()
    {
        MsgLeaveRoom msg = new MsgLeaveRoom();
        NetManager.Send(msg);
    }

    private void OnStartClick()
    {
        MsgStartBattle msg = new MsgStartBattle();
        NetManager.Send(msg);
    }


}
