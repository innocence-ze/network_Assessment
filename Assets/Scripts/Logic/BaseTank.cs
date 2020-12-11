using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTank : MonoBehaviour
{
    private GameObject model;

    public float rotateSpeed = 20;
    public float speed = 3;

    public float turretRotateSpeed = 30;
    public Transform turret;
    public Transform gun;

    public Transform firePoint;
    public float fireCD = 0.5f;
    public float lastFireTime = 0;

    public float hp = 100;

	public void Start()
	{

	}

	public void Init(string skinPath)
	{
		GameObject modelRes = ResManager.LoadPrefab(skinPath);
		model = Instantiate(modelRes);
		model.transform.parent = this.transform;
		model.transform.localPosition = Vector3.zero;

		turret = model.transform.Find("Turret");
		gun = turret.transform.Find("Gun");
		firePoint = gun.transform.Find("FirePoint");
	}


	public void Fire()
	{
		if (IsDie())
		{
			return;
		}

		GameObject bulletObj = new GameObject("Bullet");
		Bullet bullet = bulletObj.AddComponent<Bullet>();
		bullet.Init(this);

		lastFireTime = Time.time;
	}

	public bool IsDie()
	{
		return hp <= 0;
	}

	public void Attacked(float att)
	{
		if (IsDie())
		{
			return;
		}
		hp -= att;

		if (IsDie())
		{
			GameObject explode = ResManager.LoadPrefab("Explosion");
			Instantiate(explode, transform.position, transform.rotation);
		}
	}


	// Update is called once per frame
	public void Update()
	{

	}
}
