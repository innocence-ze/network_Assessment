using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncTank : BaseTank
{
	//predictive message
	private Vector3 lastPos;
	private Vector3 lastRot;

	private Vector3 forecastPos;
	private Vector3 forecastRot;

	private float forecastTime;


	public new void Init(string skinPath)
	{
		base.Init(skinPath);

		rb.constraints = RigidbodyConstraints.FreezeAll;
		rb.useGravity = false;

		lastPos = transform.position;
		lastRot = transform.eulerAngles;
		forecastPos = transform.position;
		forecastRot = transform.eulerAngles;
		forecastTime = Time.time;
	}

	new void Update()
	{
		base.Update();

		ForecastUpdate();
	}

	//synchronize position
	public void SyncPos(MsgSyncTank msg)
	{
		//forecast position
		Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
		Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
		forecastPos = pos + 2 * (pos - lastPos);
		forecastRot = rot + 2 * (rot - lastRot);
		
		//update
		lastPos = pos;
		lastRot = rot;
		forecastTime = Time.time;
		
		//turret
		Vector3 le = turret.localEulerAngles;
		le.y = msg.turretY;
		turret.localEulerAngles = le;
	}


	//update position
	public void ForecastUpdate()
	{
		//time
		float t = (Time.time - forecastTime) / CtrlTank.syncInterval;
		t = Mathf.Clamp(t, 0f, 1f);

		//position
		Vector3 pos = transform.position;
		pos = Vector3.Lerp(pos, forecastPos, t);
		transform.position = pos;

		//rotation
		Quaternion quat = transform.rotation;
		Quaternion forcastQuat = Quaternion.Euler(forecastRot);
		quat = Quaternion.Lerp(quat, forcastQuat, t);
		transform.rotation = quat;
	}

	//fire
	public void SyncFire(MsgFire msg)
	{
		Bullet bullet = Fire();
		//update pos
		Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
		Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
		bullet.transform.position = pos;
		bullet.transform.eulerAngles = rot;
	}

}
