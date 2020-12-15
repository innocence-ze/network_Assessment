using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlTank : BaseTank
{
	float lastSendSyncTime = 0;
	public static float syncInterval = 0.1f;


	new void Update()
	{
		base.Update();
		MoveUpdate();
		TurretUpdate();
		FireUpdate();
		SyncUpdate();
	}


	public void MoveUpdate()
	{
		if (IsDie())
		{
			return;
		}

		float x = Input.GetAxis("Horizontal");
		transform.Rotate(0, x * rotateSpeed * Time.deltaTime, 0);
		
		float y = Input.GetAxis("Vertical");
		Vector3 s = y * transform.forward * speed * Time.deltaTime;
		transform.transform.position += s;
	}

	public void TurretUpdate()
	{
		if (IsDie())
		{
			return;
		}

		float axis = 0;
		if (Input.GetKey(KeyCode.Q))
		{
			axis = -1;
		}
		else if (Input.GetKey(KeyCode.E))
		{
			axis = 1;
		}
		Vector3 le = turret.localEulerAngles;
		le.y += axis * Time.deltaTime * turretRotateSpeed;
		turret.localEulerAngles = le;
	}

	public void FireUpdate()
	{
		if (IsDie())
		{
			return;
		}

		if (!Input.GetKey(KeyCode.Space) || Time.time - lastFireTime < fireCD)
		{
			return;
		}

        Bullet bullet = Fire();

		MsgFire msg = new MsgFire
		{
			x = bullet.transform.position.x,
			y = bullet.transform.position.y,
			z = bullet.transform.position.z,
			ex = bullet.transform.eulerAngles.x,
			ey = bullet.transform.eulerAngles.y,
			ez = bullet.transform.eulerAngles.z,
		};

		NetManager.Send(msg);
	}

	public void SyncUpdate()
    {
		if(Time.time - lastSendSyncTime < syncInterval)
        {
			return;
        }
		lastSendSyncTime = Time.time;

		MsgSyncTank msg = new MsgSyncTank
		{
			x = transform.position.x,
			y = transform.position.y,
			z = transform.position.z,
			ex = transform.eulerAngles.x,
			ey = transform.eulerAngles.y,
			ez = transform.eulerAngles.z,
			turretY = turret.localEulerAngles.y,
		};
		NetManager.Send(msg);
    }
}
