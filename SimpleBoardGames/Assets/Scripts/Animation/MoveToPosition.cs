using System;
using UnityEngine;


public class MoveToPosition : MonoBehaviour
{
	public delegate void MoveFinishedDelegate(Transform objTr);


	public event MoveFinishedDelegate OnFinishedMove;

	public Vector3 EndPos;
	public float TotalTime = 1.0f;
	public AnimationCurve MovementCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

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

		if (T >= 1.0f)
		{
			if (OnFinishedMove != null)
				OnFinishedMove(MyTr);

			Destroy(this);
		}
	}
}