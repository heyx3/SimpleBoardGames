using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Fitchneil.UnityLogic
{
	public class FN_Piece : BoardGames.UnityLogic.PieceRenderer<Vector2i>
	{
		protected override void Awake()
		{
			base.Awake();

			//When this piece is clicked, show the available moves.
			OnStopClick += (me, endPos) =>
			{
				//Only do this if it's this player's turn.
				var gameMode = BoardGames.UnityLogic.GameMode.GameMode<Vector2i>.Instance;
				if (ToTrack.Owner.Value == gameMode.CurrentTurn.Value)
				{
					FN_MovesUI.Instance.CurrentPiece = (Piece)ToTrack;
				}
			};
		}

		protected override void ResetPieceFocus(BoardGames.Piece<Vector2i> oldPiece,
												BoardGames.Piece<Vector2i> newPiece)
		{
			base.ResetPieceFocus(oldPiece, newPiece);

			//Set the sprite based on what kind of piece this is.
			if (newPiece != null)
			{
				Piece p = (Piece)newPiece;
				if (p.Owner.Value == Board.Player_Defender)
				{
					if (p.IsKing)
						Spr.sprite = FN_PieceDispatcher.Instance.Sprite_King;
					else
						Spr.sprite = FN_PieceDispatcher.Instance.Sprite_Defender;
				}
				else
				{
					Spr.sprite = FN_PieceDispatcher.Instance.Sprite_Attacker;
				}
			}
		}

		protected override IEnumerator MovePieceCoroutine(Vector2i startPos, Vector2i endPos)
		{
			//Interpolate from the start pos to the end pos using a curve.

			Vector3 startPosWorld = Board.ToWorld(startPos),
					endPosWorld = Board.ToWorld(endPos);

			MyTr.position = startPosWorld;
			yield return null;

			float elapsedTime = 0.0f,
				  t = 0.0f;
			while (t < 1.0f)
			{
				MyTr.position = Vector3.Lerp(startPosWorld, endPosWorld,
										     FN_PieceDispatcher.Instance.PieceMovementCurve.Evaluate(t));

				elapsedTime += Time.deltaTime;
				t = elapsedTime / FN_PieceDispatcher.Instance.PieceMovementTime;

				yield return null;
			}

			MyTr.position = endPosWorld;
		}
	}
}