using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    private Text text;
    private Button okButton;

    public override void OnInit()
    {
        skinPath = "TipPanel";
        layer = PanelManager.Layer.Tip;
    }

    public override void OnShow(params object[] arg)
    {
        text = skin.transform.Find("Text").GetComponent<Text>();
        okButton = skin.transform.Find("OKButton").GetComponent<Button>();

        okButton.onClick.AddListener(OnOkClick);

        if(arg.Length == 1)
        {
            text.text = (string)arg[0];
        }
    }

    private void OnOkClick()
    {
        Close();
    }

    public override void OnClose()
    {
    }
}
