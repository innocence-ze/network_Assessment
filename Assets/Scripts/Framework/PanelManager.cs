using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PanelManager
{
    public enum Layer
    {
        Panel,
        Tip,
    }

    static Dictionary<Layer, Transform> layerDic = new Dictionary<Layer, Transform>();
    public static Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();

    public static Transform root;
    public static Transform canvas;

    public static void Init()
    {
        root = GameObject.Find("Root").transform;
        canvas = root.Find("Canvas");

        Transform panel = canvas.Find("Panel");
        Transform tip = canvas.Find("Tip");

        layerDic.Add(Layer.Panel, panel);
        layerDic.Add(Layer.Tip, tip);
    }

    public static void Open<T>(params object[] args) where T:BasePanel
    {
        string name = typeof(T).ToString();
        if (panelDic.ContainsKey(name))
        {
            return;
        }

        BasePanel panel = root.gameObject.AddComponent<T>();
        panel.OnInit();
        panel.Init();

        Transform layer = layerDic[panel.layer];
        panel.skin.transform.SetParent(layer, false);

        panelDic.Add(name, panel);
        panel.OnShow(args);
    }

    public static void Close(string name)
    {
        if (!panelDic.ContainsKey(name))
        {
            return;
        }
        BasePanel panel = panelDic[name];
        if(panel == null)
        {
            return;
        }

        panel.OnClose();
        panelDic.Remove(name);

        Object.Destroy(panel.skin);
        Object.Destroy(panel);
    }

}
