using System;
using System.Collections.Generic;
using UnityEngine;

using SingleCurve = Curves.Curve<UnityEngine.Vector3, Curves.Ops_Lerp>;
using MultiCurve = Curves.MultiCurve<UnityEngine.Vector3, Curves.Ops_Lerp>;


public class FollowCurve : MonoBehaviour
{
	public MultiCurve Curve = new MultiCurve(new MultiCurve.CurveElement(new SingleCurve(Vector3.zero, Vector3.one), 1.0f));

	public float TraversalTime = 1.0f;
	public float T = 0.0f;

	public bool DestroyWhenDone = true;

	public event UnityEngine.Events.UnityAction<FollowCurve> OnDoneMoving;


	public Transform MyTr { get; private set; }


	void Start()
	{
		MyTr = transform;
	}
	void Update()
	{
		if (T >= 1.0f)
		{
			if (OnDoneMoving != null)
				OnDoneMoving(this);
			if (DestroyWhenDone)
				Destroy(this);
		}

		MyTr.position = Curve.GetValue(T / TraversalTime);
		T += Time.deltaTime;
	}
}