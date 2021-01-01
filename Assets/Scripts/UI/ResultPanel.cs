using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : BasePanel
{
	private Image winImage;
	private Image lostImage;
	private Button okBtn;


	public override void OnInit()
	{
		skinPath = "ResultPanel";
		layer = PanelManager.Layer.Tip;
	}

	public override void OnShow(params object[] args)
	{
		winImage = skin.transform.Find("WinImage").GetComponent<Image>();
		lostImage = skin.transform.Find("LostImage").GetComponent<Image>();
		okBtn = skin.transform.Find("OKButton").GetComponent<Button>();

		okBtn.onClick.AddListener(OnOkClick);

		if (args.Length == 1)
		{
			bool isWIn = (bool)args[0];
			if (isWIn)
			{
				winImage.gameObject.SetActive(true);
				lostImage.gameObject.SetActive(false);
			}
			else
			{
				winImage.gameObject.SetActive(false);
				lostImage.gameObject.SetActive(true);
			}
		}
	}


	public override void OnClose()
	{

	}


	public void OnOkClick()
	{
		PanelManager.Open<RoomPanel>();
		

		Close();
	}
}
