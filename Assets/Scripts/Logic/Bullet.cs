using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

	public float speed = 100f;
	public BaseTank tank;
	private GameObject model;
	private float life = 10f;

	
	public void Init(BaseTank bt)
	{
		
		GameObject modelRes = ResManager.LoadPrefab("BulletPrefab");
		model = Instantiate(modelRes);
		model.transform.parent = transform;
		model.transform.localPosition = Vector3.zero;
		model.transform.localEulerAngles = Vector3.zero;

		var rb = gameObject.AddComponent<Rigidbody>();
		rb.useGravity = false;
		rb.mass = 0.1f;

		var sc = gameObject.AddComponent<SphereCollider>();
		sc.radius = 0.52f;
		sc.isTrigger = true;

		tank = bt;
		transform.position = bt.firePoint.position;
		transform.rotation = bt.firePoint.rotation;
	}

	// Update is called once per frame
	void Update()
	{
		transform.position += transform.forward * speed * Time.deltaTime;
		life -= Time.deltaTime;
		if(life  <= 0)
        {
			Destroy(gameObject);
        }
	}

    private void OnTriggerEnter(Collider other)
	{
		
		GameObject collObj = other.gameObject;
		BaseTank hitTank = collObj.GetComponent<BaseTank>();
		
		if (hitTank != null && hitTank == tank)
		{
			return;
		}
		
		if (hitTank != null)
		{
			SendMsgHit(tank, hitTank);
		}
		
		GameObject explode = ResManager.LoadPrefab("Fire");
		explode = Instantiate(explode, transform.position, transform.rotation);
		Destroy(explode, 1);
		Destroy(gameObject, 1);
		gameObject.SetActive(false);
	}

	void SendMsgHit(BaseTank tank, BaseTank hitTank)
    {
		if(hitTank==null || tank == null)
        {
			return;
        }

		if(tank.id != GameMain.id)
        {
			return;
        }

		MsgHit msg = new MsgHit
		{
			targetId = hitTank.id,
			id = tank.id,
			x = transform.position.x,
			y = transform.position.y,
			z = transform.position.z,
		};
		NetManager.Send(msg);
    }
}
