using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMoveClick()
    {
        MsgMove mm = new MsgMove()
        {
            x = 120,
            y = 111,
            z = 0
        };
        NetManager.Send(mm);
    }

    public void OnConnectClick()
    {
        NetManager.Connect("127.0.0.1", 8888);
    }

}