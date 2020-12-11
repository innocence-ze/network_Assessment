using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    public static string id = "";

    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddEventListener(NetManager.EventType.close, OnConnectClose);

        PanelManager.Init();
        PanelManager.Open<LoginPanel>();
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
}
