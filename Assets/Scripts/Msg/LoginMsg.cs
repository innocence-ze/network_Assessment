﻿public class MsgLogin : MsgBase
{
    public MsgLogin() { msgName = "MsgLogin"; }
    public string id = "";

    //0-succeed; 1-fail
    public int result = 0;
}