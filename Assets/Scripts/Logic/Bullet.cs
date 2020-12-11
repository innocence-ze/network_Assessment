﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

	public float speed = 100f;
	public BaseTank tank;
	private GameObject model;

	
	public void Init(BaseTank bt)
	{
		
		GameObject modelRes = ResManager.LoadPrefab("bulletPrefab");
		model = Instantiate(modelRes);
		model.transform.parent = transform;
		model.transform.localPosition = Vector3.zero;
		model.transform.localEulerAngles = Vector3.zero;

		tank = bt;
		transform.position = bt.firePoint.position;
		transform.rotation = bt.firePoint.rotation;
	}

	// Update is called once per frame
	void Update()
	{
		transform.position += transform.forward * speed * Time.deltaTime;
	}

	
	void OnCollisionEnter(Collision collisionInfo)
	{
		
		GameObject collObj = collisionInfo.gameObject;
		BaseTank hitTank = collObj.GetComponent<BaseTank>();
		
		if (hitTank != null && hitTank == tank)
		{
			return;
		}
		
		if (hitTank != null)
		{
			hitTank.Attacked(35);
		}
		
		GameObject explode = ResManager.LoadPrefab("fire");
		Instantiate(explode, transform.position, transform.rotation);
		
		Destroy(gameObject);
	}
}