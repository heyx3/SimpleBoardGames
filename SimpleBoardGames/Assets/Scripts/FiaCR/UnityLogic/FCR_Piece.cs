﻿using System;
using System.Collections;
using System.Collections.Generic;
using BoardGames;
using UnityEngine;


namespace FiaCR.UnityLogic
{
	//TODO: Indicate when friendly pieces have already been moved this turn.

	public class FCR_Piece : BoardGames.UnityLogic.PieceRenderer<Vector2i>
	{
		public GameObject Child_Friendly, Child_Cursed;


		protected override void Awake()
		{
			base.Awake();

			//When this piece is clicked, show the available moves.
			OnStopClick += (me, endPos) =>
			{
				//Only do this if it's a friendy piece and it's Billy's turn.
				var gameMode = (GameMode.FCR_Game_Offline)GameMode.FCR_Game_Offline.Instance;
				if (ToTrack.Owner.Value == Board.Player_Humans &&
					gameMode.CurrentTurn.Value == Board.Player_Humans &&
					!gameMode.IsJuliaTurn)
				{
					FCR_MovesUI_Billy.Instance.CurrentPiece = (Piece)ToTrack;
				}
			};

			Child_Friendly.SetActive(false);
			Child_Cursed.SetActive(false);
		}
		private void Start()
		{
			//When this piece changes teams, flip its sprite.
			ToTrack.Owner.OnChanged += (piece, oldTeam, newTeam) =>
			{
				ResetPieceFocus(piece, piece);

				//Play the "capture" effects.
				var obj = Instantiate(FCR_PieceDispatcher.Instance.PieceCaptureEffectsPrefab);
				obj.transform.position = MyTr.position;
			};
		}

		protected override void ResetPieceFocus(Piece<Vector2i> oldPiece,
												Piece<Vector2i> newPiece)
		{
			base.ResetPieceFocus(oldPiece, newPiece);

			//Set the sprite.
			if (newPiece != null)
			{
				Child_Friendly.SetActive(newPiece.Owner.Value == Board.Player_Humans);
				Child_Cursed.SetActive(newPiece.Owner.Value == Board.Player_TC);
			}
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