using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Vector3 distance = new Vector3(0, 8, -18);
	public Vector3 offset = new Vector3(0, 5f, 0);
	public float speed = 3f;

	// Use this for initialization
	void Start()
	{
		Vector3 pos = transform.position;
		Vector3 forward = transform.forward;
		Vector3 targetPos = pos + forward * distance.z;
		targetPos.y += distance.y;
		Camera.main.transform.position = targetPos;
		Camera.main.transform.LookAt(pos + offset);
	}

	void LateUpdate()
	{
		Vector3 pos = transform.position;
		Vector3 forward = transform.forward;
        Vector3 targetPos = pos + forward * distance.z;
        targetPos.y += distance.y;
		Vector3 cameraPos = Camera.main.transform.position;
		cameraPos = Vector3.MoveTowards(cameraPos, targetPos, Time.deltaTime * speed);
		Camera.main.transform.position = cameraPos;
		Camera.main.transform.LookAt(pos + offset);
	}
}



