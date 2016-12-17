using System;
using UnityEngine;


namespace Fitchneil.UnityLogic
{
	public class FN_Constants : Singleton<FN_Constants>
	{
		public Sprite Sprite_Defender, Sprite_Attacker, Sprite_King;

		public AnimationCurve PieceMovementCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
		public float PieceMovementTime = 0.5f;
	}
}