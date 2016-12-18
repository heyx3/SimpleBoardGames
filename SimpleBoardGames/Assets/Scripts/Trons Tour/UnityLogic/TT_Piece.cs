using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TronsTour.UnityLogic
{
	public class TT_Piece : BoardGames.UnityLogic.PieceRenderer<Vector2i>
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
					TT_MovesUI.Instance.CurrentPiece = (Piece)ToTrack;
				}
			};
		}

		protected override void ResetPieceFocus(BoardGames.Piece<Vector2i> oldPiece,
												BoardGames.Piece<Vector2i> newPiece)
		{
			base.ResetPieceFocus(oldPiece, newPiece);
			
			if (newPiece != null)
			{
				MyTr.position = TT_BoardDisplay.Instance.ToWorld(newPiece.CurrentPos);

				//Set the sprite based on what kind of piece this is.
				switch (newPiece.Owner.Value)
				{
					case BoardGames.Players.One:
						Spr.sprite = TT_BoardDisplay.Instance.Piece1Sprite;
						break;
					case BoardGames.Players.Two:
						Spr.sprite = TT_BoardDisplay.Instance.Piece2Sprite;
						break;
					default: throw new NotImplementedException(newPiece.Owner.Value.ToString());
				}
			}
		}

		protected override IEnumerator MovePieceCoroutine(Vector2i startPos, Vector2i endPos)
		{
			//Interpolate from the start pos to the end pos using a curve.
			
			Vector3 startPosWorld = TT_BoardDisplay.Instance.ToWorld(startPos),
					endPosWorld = TT_BoardDisplay.Instance.ToWorld(endPos);

			MyTr.position = startPosWorld;
			yield return null;

			float elapsedTime = 0.0f,
				  t = 0.0f;
			while (t < 1.0f)
			{
				MyTr.position = Vector3.Lerp(startPosWorld, endPosWorld,
										     TT_BoardDisplay.Instance.PieceMovementCurve.Evaluate(t));

				elapsedTime += Time.deltaTime;
				t = elapsedTime / TT_BoardDisplay.Instance.PieceMovementTime;

				yield return null;
			}

			MyTr.position = endPosWorld;
		}
	}
}
