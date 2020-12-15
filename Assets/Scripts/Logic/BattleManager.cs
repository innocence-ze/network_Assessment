using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager
{
    public static Dictionary<string, BaseTank> tankDic = new Dictionary<string, BaseTank>();

    public static void Init()
    {
        NetManager.AddMsgListener("MsgEnterBattle", OnMsgEnterBattle);
        NetManager.AddMsgListener("MsgBattleResult", OnMsgBattleResult);
        NetManager.AddMsgListener("MsgLeaveBattle", OnMsgLeaveBattle);
        NetManager.AddMsgListener("MsgFire",OnMsgFire);
        NetManager.AddMsgListener("MsgSyncTank", OnMsgSyncTank);
        NetManager.AddMsgListener("MsgHit", OnMsgHit);
    }

    public static void AddTank(string id, BaseTank tank)
    {
        tankDic[id] = tank;
    }

    public static void RemoveTank(string id)
    {
        tankDic.Remove(id);
    }

    public static BaseTank GetTank(string id)
    {
        if (tankDic.ContainsKey(id))
        {
            return tankDic[id];
        }
        return null;
    }

    public static BaseTank GetCtrlTank()
    {
        return GetTank(GameMain.id);
    }

    public static void Reset()
    {
        foreach(var tank in tankDic.Values)
        {
            Object.Destroy(tank.gameObject);
        }
        tankDic.Clear();
    }

    public static void EnterBattle(MsgEnterBattle msg)
    {
        Reset();
        PanelManager.Close("RoomPanel");
        PanelManager.Close("ResultPanel");

        for(int i = 0; i < msg.tanks.Length; i++)
        {
            GenrateTank(msg.tanks[i]);
        }
    }

    private static void GenrateTank(TankInfo tankInfo)
    {
        string objName = "Tank_" + tankInfo.id;
        GameObject tankObj = new GameObject(objName);

        BaseTank tank;
        if (tankInfo.id == GameMain.id)
        {
            tank = tankObj.AddComponent<CtrlTank>();
            tankObj.AddComponent<CameraFollow>();
        }
        else
        {
            tank = tankObj.AddComponent<SyncTank>();
        }

        tank.camp = tankInfo.camp;
        tank.id = tankInfo.id;
        tank.hp = tankInfo.hp;

        if (tank.camp == 1)
        {
            tank.Init("TankPrefab");
        }
        else if (tank.camp == 2)
        {
            tank.Init("TankPrefab2");
        }

        Vector3 pos = new Vector3(tankInfo.x, tankInfo.y, tankInfo.z);
        Vector3 rot = new Vector3(tankInfo.ex, tankInfo.ey, tankInfo.ez);
        tank.transform.position = pos;
        tank.transform.eulerAngles = rot;

        AddTank(tankInfo.id, tank);
    }

    public static void OnMsgEnterBattle(MsgBase msgbase)
    {
        MsgEnterBattle msg = msgbase as MsgEnterBattle;
        EnterBattle(msg);
    }

    public static void OnMsgBattleResult(MsgBase msgBase)
    {
        MsgBattleResult msg = msgBase as MsgBattleResult;

        bool isWin = false;
        BaseTank tank = GetCtrlTank();
        if(tank != null && tank.camp == msg.winCamp)
        {
            isWin = true;
        }

        PanelManager.Open<ResultPanel>(isWin);
    }

    public static void OnMsgLeaveBattle(MsgBase msgBase)
    {
        MsgLeaveBattle msg = msgBase as MsgLeaveBattle;
        BaseTank tank = GetTank(msg.id);
        if (tank != null)
        {
            RemoveTank(msg.id);
            Object.Destroy(tank.gameObject);
        }
    }

    public static void OnMsgSyncTank(MsgBase msgBase)
    {
        MsgSyncTank msg = msgBase as MsgSyncTank;

        if(msg.id == GameMain.id)
        {
            return;
        }

        SyncTank tank = GetTank(msg.id) as SyncTank;
        if(tank == null)
        {
            return;
        }

        tank.SyncPos(msg);
    }

    public static void OnMsgFire(MsgBase msgBase)
    {
        MsgFire msg = msgBase as MsgFire;
        if(msg.id == GameMain.id)
        {
            return;
        }

        SyncTank tank = GetTank(msg.id) as SyncTank;
        if(tank == null)
        {
            return;
        }

        tank.SyncFire(msg);
    }

    public static void OnMsgHit(MsgBase msgBase)
    {
        MsgHit msg = msgBase as MsgHit;
        if(msg.id == GameMain.id)
        {
            return;
        }

        BaseTank tank = GetTank(msg.id);
        if(tank == null)
        {
            return;
        }

        tank.Attacked(msg.damage);

    }

}
