using System;
using UnityEngine;


public class MoveToPosition : MonoBehaviour
{
	public Vector3 EndPos;
	public float TotalTime = 1.0f;
	public AnimationCurve MovementCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f),
															 new Keyframe(1.0f, 1.0f));

	public float T { get; private set; }

	public Vector3 StartPos { get; private set; }
	public Transform MyTr { get; private set; }


	void Awake()
	{
		MyTr = transform;
		StartPos = MyTr.position;
		T = 0.0f;
	}

	void Update()
	{
		T += Time.deltaTime / TotalTime;
		MyTr.position = Vector3.Lerp(StartPos, EndPos, MovementCurve.Evaluate(T));
	}
}