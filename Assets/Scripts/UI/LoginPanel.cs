using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel
{
    private InputField idInput;
    private Button loginButton;

    public override void OnInit()
    {
        skinPath = "LoginPanel";
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] arg)
    {
        idInput = skin.transform.Find("IdInput").GetComponent<InputField>();
        loginButton = skin.transform.Find("LoginButton").GetComponent<Button>();

        loginButton.onClick.AddListener(OnLoginClick);

        NetManager.AddMsgListener("MsgLogin", OnMsgLogin);
        NetManager.AddEventListener(NetManager.EventType.connectSuccess, OnConnectSuccess);
        NetManager.AddEventListener(NetManager.EventType.connectFail, OnConnectFail);

        NetManager.Connect("127.0.0.1", 8888);

    }

    public override void OnClose()
    {
        NetManager.RemoveMSGlistener("MsgLogin", OnMsgLogin);
        NetManager.RemoveEventListener(NetManager.EventType.connectSuccess, OnConnectSuccess);
        NetManager.RemoveEventListener(NetManager.EventType.connectFail, OnConnectFail);
    }

    private void OnConnectFail(string str)
    {
        PanelManager.Open<TipPanel>(str);
    }

    private void OnConnectSuccess(string str)
    {
        Debug.Log("Connect to server succeed");
    }

    private void OnMsgLogin(MsgBase msg)
    {
        MsgLogin ml = msg as MsgLogin;
        if(ml.result == 0)
        {
            Debug.Log("login succeed");

            GameObject tankObj = new GameObject("MyTank");
            CtrlTank ctrlTank = tankObj.AddComponent<CtrlTank>();
            ctrlTank.Init("TankPrefab");
            tankObj.AddComponent<CameraFollow>();
            GameMain.id = ml.id;
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("The id is already in the game, please choose another one");
        }
    }

    private void OnLoginClick()
    {
        if(idInput.text == "")
        {
            PanelManager.Open<TipPanel>("Please input your id");
            return;
        }

        MsgLogin msgLogin = new MsgLogin
        {
            id = idInput.text,
        };

        NetManager.Send(msgLogin);
    }

}
