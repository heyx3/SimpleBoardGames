using System;
using System.Collections.Generic;
using UnityEngine;


namespace Surakarta
{
	public class Constants : Singleton<Constants>
	{
		public AnimationCurve NormalMoveCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
		public float NormalMoveTime = 0.5f;

		public GameObject CapturePlayer1EffectsPrefab, CapturePlayer2EffectsPrefab;
	}
}