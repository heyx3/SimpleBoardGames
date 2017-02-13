using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AnimateTransform : MonoBehaviour
{
	public AnimationCurve PosX = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 0.0f),
						  PosY = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 0.0f),
						  PosZ = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 0.0f),
						  Rot = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 0.0f),
						  ScaleX = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.25f),
						  ScaleY = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.25f),
						  ScaleZ = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);
	public float AnimationSpeed = 4.0f;
	public int AnimationDir = 1;
	public bool ReverseOnFinish = true;

	[NonSerialized]
	public float T = 0.0f;

	private Vector3 startPos, startScale;
	private float startRot;

	private Transform tr;


	private void Awake()
	{
		tr = transform;
		startPos = tr.localPosition;
		startScale = tr.localScale;
		startRot = tr.localEulerAngles.z;
	}
	private void Update()
	{
		if (AnimationDir == 1)
		{
			T += Time.deltaTime * AnimationSpeed;
			if (T >= 1.0f)
			{
				T = 1.0f;
				if (ReverseOnFinish)
					AnimationDir = -1;
				else
					AnimationDir = 0;
			}
		}
		else if (AnimationDir == -1)
		{
			T -= Time.deltaTime * AnimationSpeed;
			if (T <= 0.0f)
			{
				T = 0.0f;
				AnimationDir = 0;
			}
		}

		tr.localPosition = new Vector3(PosX.Evaluate(T), PosY.Evaluate(T), PosZ.Evaluate(T)) +
						   startPos;
		tr.localScale = new Vector3(startScale.x * ScaleX.Evaluate(T),
									startScale.y * ScaleY.Evaluate(T),
									startScale.z * ScaleZ.Evaluate(T));
		tr.localEulerAngles = new Vector3(tr.localEulerAngles.x, tr.localEulerAngles.y,
										  startRot + Rot.Evaluate(T));
	}
}