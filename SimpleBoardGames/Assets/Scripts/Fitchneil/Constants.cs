using System;
using UnityEngine;


namespace Fitchneil
{
	public class Constants : Singleton<Constants>
	{
		public GameObject KingPrefab, DefenderPrefab, AttackerPrefab;

		public Sprite Movement;

		public float TurnColorMultiplier = 0.25f;

		public Color NormalMovementIndicator = Color.yellow,
					 SpecialMovementIndicator = Color.green;
		public Color ValidMovement = Color.green,
					 InvalidMovement = Color.red;

		public GameObject DestroyedPieceEffectPrefab;
		

		public AnimationCurve MovePieceCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
		public float MovePieceTime = 0.25f;


		public SpriteRenderer[] AttackerTexts, DefenderTexts;
		[NonSerialized]
		public Color InitialAttackerTextColor, InitialDefenderTextColor;

		public GameObject AttackersWinUI, DefendersWinUI;


		public Piece CreateKing(Vector2i pos)
		{
			GameObject go = Instantiate(KingPrefab);
			go.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0.0f);

			return go.GetComponent<Piece>();
		}
		public Piece CreateDefender(Vector2i pos)
		{
			GameObject go = Instantiate(DefenderPrefab);
			go.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0.0f);

			return go.GetComponent<Piece>();
		}
		public Piece CreateAttacker(Vector2i pos)
		{
			GameObject go = Instantiate(AttackerPrefab);
			go.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0.0f);

			return go.GetComponent<Piece>();
		}


		void Start()
		{
			InitialAttackerTextColor = AttackerTexts[0].color;
			InitialDefenderTextColor = DefenderTexts[0].color;
		}
	}
}