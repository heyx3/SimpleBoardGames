using System;
using UnityEngine;


public class KillAfterTime : MonoBehaviour
{
	public float KillTime = 2.5f;
	public UnityEngine.Object ToKill = null;


	void Start()
	{
		if (ToKill == null)
		{
			ToKill = gameObject;
		}
	}
	void Update()
	{
		KillTime -= Time.deltaTime;
		if (KillTime <= 0.0f)
		{
			Destroy(gameObject);
		}
	}
}