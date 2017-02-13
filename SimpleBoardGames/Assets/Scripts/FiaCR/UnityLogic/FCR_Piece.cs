using System;
using System.Collections;
using System.Collections.Generic;
using BoardGames;
using UnityEngine;


namespace FiaCR.UnityLogic
{
	public class FCR_Piece : BoardGames.UnityLogic.PieceRenderer<Vector2i>
	{
		public GameObject Child_Friendly, Child_Cursed;

		public Stat<bool, FCR_Piece> MovedAlready;


		protected override void Awake()
		{
			base.Awake();

			MovedAlready = new Stat<bool, FCR_Piece>(this, false);
			MovedAlready.OnChanged += (_this, oldVal, newVal) =>
			{
				if (newVal)
					Spr.color = Color.grey;
				else
					Spr.color = Color.white;
			};

			//When this piece is clicked, show the available moves.
			OnStopClick += (me, endPos) =>
			{
				//Only do this if it's a friendy, unmoved piece and it's Billy's turn.
				var gameMode = (GameMode.FCR_Game_Offline)GameMode.FCR_Game_Offline.Instance;
				if (ToTrack.Owner.Value == Board.Player_Humans &&
					gameMode.CurrentTurn.Value == Board.Player_Humans &&
					!gameMode.IsJuliaTurn && !MovedAlready.Value)
				{
					FCR_MovesUI_Billy.Instance.CurrentPiece = (Piece)ToTrack;
				}
			};

			Child_Friendly.SetActive(false);
			Child_Cursed.SetActive(false);
		}
		private void Start()
		{
			ResetPieceFocus(null, ToTrack);
		}

		protected override void ResetPieceFocus(Piece<Vector2i> oldPiece,
												Piece<Vector2i> newPiece)
		{
			base.ResetPieceFocus(oldPiece, newPiece);

			if (oldPiece != null)
			{
				oldPiece.Owner.OnChanged -= Callback_PieceOwnerChanged;
			}

			//Set the sprite.
			if (newPiece != null)
			{
				if (newPiece.Owner.Value == Board.Player_Humans)
				{
					Child_Friendly.SetActive(true);
					Child_Cursed.SetActive(false);
					Spr = Child_Friendly.GetComponent<SpriteRenderer>();
				}
				else
				{
					Child_Friendly.SetActive(false);
					Child_Cursed.SetActive(true);
					Spr = Child_Cursed.GetComponent<SpriteRenderer>();
				}

				newPiece.Owner.OnChanged += Callback_PieceOwnerChanged;
			}
		}

		private void Callback_PieceOwnerChanged(BoardGames.Piece<Vector2i> piece,
												BoardGames.Players oldTeam,
												BoardGames.Players newTeam)
		{
			if (piece.Owner.Value == Board.Player_Humans)
			{
				Child_Friendly.SetActive(true);
				Child_Cursed.SetActive(false);
				Spr = Child_Friendly.GetComponent<SpriteRenderer>();
			}
			else
			{
				Child_Friendly.SetActive(false);
				Child_Cursed.SetActive(true);
				Spr = Child_Friendly.GetComponent<SpriteRenderer>();
			}
			if (MovedAlready)
				Spr.color = Color.grey;
			else
				Spr.color = Color.white;

			//Play the "Capture" effects.
			var obj = Instantiate(FCR_PieceDispatcher.Instance.PieceCaptureEffectsPrefab);
			obj.transform.position = MyTr.position;
		}

		protected override IEnumerator MovePieceCoroutine(Vector2i startPos, Vector2i endPos)
		{
			//Interpolate from the start pos to the end pos using a curve.

			Board board = (Board)GameMode.FCR_Game_Offline.Instance.TheBoard;
			Vector3 startPosWorld = board.ToWorld(startPos),
					endPosWorld = board.ToWorld(endPos);

			MyTr.position = startPosWorld;
			yield return null;

			float elapsedTime = 0.0f,
				  t = 0.0f;
			while (t < 1.0f)
			{
				MyTr.position = Vector3.Lerp(startPosWorld, endPosWorld,
											 FCR_PieceDispatcher.Instance.PieceMovementCurve.Evaluate(t));

				elapsedTime += Time.deltaTime;
				t = elapsedTime / FCR_PieceDispatcher.Instance.PieceMovementTime;

				yield return null;
			}

			MyTr.position = endPosWorld;
		}
	}
}