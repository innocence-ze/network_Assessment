using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlTank : BaseTank
{

	new void Update()
	{
		base.Update();
		MoveUpdate();
		TurretUpdate();
		FireUpdate();
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

		Fire();
	}
}
