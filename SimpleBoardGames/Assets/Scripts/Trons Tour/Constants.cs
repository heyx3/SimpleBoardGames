using System;
using System.Collections.Generic;
using UnityEngine;


namespace TronsTour
{
	public class Constants : Singleton<Constants>
	{
		public Sprite OpenSpaceSprite, ClosedSpaceSprite;
		public Sprite MoveOptionSprite;

		public AnimationCurve MovePieceCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
		public float MovePieceTime = 0.25f;

		public GameObject PlacePieceEffectPrefab;

		public Color GoodMoveCol = Color.green,
					 BadMoveCol = Color.red;

		public GameObject[] WinnerUIs;

		
		[Serializable]
		public class TurnTexts
		{
			public SpriteRenderer[] Texts = new SpriteRenderer[1];
			public Color NormalColor = Color.white,
						 NotMyTurnColor = Color.grey;
		}
		public TurnTexts[] TextsForPlayers = new TurnTexts[2];
	}
}