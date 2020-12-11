﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour
{
    public string skinPath;
    public GameObject skin;
    public PanelManager.Layer layer = PanelManager.Layer.Panel;

    public void Init()
    {
        GameObject skinPrefab = ResManager.LoadPrefab(skinPath);
        skin = Instantiate(skinPrefab);
    }
    
    public void Close()
    {
        PanelManager.Close(GetType().ToString());
    }

    public virtual void OnInit()
    {

    }

    public virtual void OnShow(params object[] arg)
    {

    }

    public virtual void OnClose()
    {

    }
}
