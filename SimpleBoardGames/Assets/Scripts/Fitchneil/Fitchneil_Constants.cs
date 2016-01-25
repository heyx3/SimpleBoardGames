using System;
using System.Collections.Generic;
using UnityEngine;


public class Fitchneil_Constants : Singleton<Fitchneil_Constants>
{
	public AnimationCurve MovePieceCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
	public float MovePieceTime = 0.5f;
}