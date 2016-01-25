using System;
using UnityEngine;


public class Blink : MonoBehaviour
{
	public Renderer ToBlink;
	public float BlinkInterval = 0.1f;

	private float tillBlink;


	void Start()
	{
		tillBlink = BlinkInterval;
	}
	void Update()
	{
		tillBlink -= Time.deltaTime;
		if (tillBlink <= 0.0f)
		{
			ToBlink.enabled = !ToBlink.enabled;
			tillBlink = BlinkInterval;
		}
	}
}