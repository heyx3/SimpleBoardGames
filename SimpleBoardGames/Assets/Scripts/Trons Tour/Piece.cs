using System;
using System.Collections.Generic;
using UnityEngine;


namespace TronsTour
{
	public class Piece : BoardGames.Piece<Vector2i>
	{
		public Transform[] DraggingTracers = new Transform[2];
		public Transform PrevPosIndicator;

		private static List<Movement> moves = null;
		private static List<SpriteRenderer> movementIndicators;


		public override void OnDrag(Vector2 startMouseWorldPos, Vector2 currentMouseWorldPos)
		{
			if (moves == null)
				return;

			MyTr.position = currentMouseWorldPos;

			Board brd = (Board)Board.Instance;
			
			Vector2i newPosI = brd.ToBoard(currentMouseWorldPos);
			Vector3 oldPos = brd.ToWorld(CurrentPos),
					newPos = brd.ToWorld(newPosI);

			//Update UI elements.
			for (int i = 0; i < DraggingTracers.Length; ++i)
			{
				DraggingTracers[i].position = newPos;
				DraggingTracers[i].gameObject.SetActive(true);
			}
			PrevPosIndicator.position = oldPos;
			PrevPosIndicator.gameObject.SetActive(true);
		}
		public override void OnStartClick(Vector2 mouseWorldPos)
		{
			ClearMoves();
			SetUpMoves();
		}
		public override void OnStopClick(Vector2 mouseWorldPos)
		{

		}
		public override void OnStopDrag(Vector2 startMouseWorldPos, Vector2 endMouseWorldPos)
		{
			if (moves == null)
				return;
			

			Board brd = (Board)Board.Instance;
			
			//Deactivate the UI stuff.
			for (int i = 0; i < DraggingTracers.Length; ++i)
			{
				DraggingTracers[i].gameObject.SetActive(false);
			}
			PrevPosIndicator.gameObject.SetActive(false);
			
			//Move this piece back to its current position.
			MyTr.position = brd.ToWorld(CurrentPos);

			//If the user indicated a valid move, execute it.
			Vector2i newPosI = brd.ToBoard(endMouseWorldPos);
			if (newPosI != CurrentPos)
			{
				for (int i = 0; i < moves.Count; ++i)
				{
					if (moves[i].Pos == newPosI)
					{
						((State_PlayTurns)StateMachine.Instance.CurrentState).ExecuteTurn(moves[i]);
						ClearMoves();

						return;
					}
				}
			}
		}

		private void SetUpMoves()
		{
			moves = new List<Movement>(Board.Instance.GetMoves(this));
			movementIndicators = SpritePool.Instance.AllocateSprites(moves.Count,
																	 Constants.Instance.MoveOptionSprite,
																	 1, null, "Movement Indicator");

			Board brd = (Board)Board.Instance;
			for (int i = 0; i < moves.Count; ++i)
			{
				movementIndicators[i].transform.position = brd.ToWorld(moves[i].Pos);
			}
		}
		private void ClearMoves()
		{
			if (moves == null)
				return;

			moves = null;
			SpritePool.Instance.DeallocateSprites(movementIndicators);
			movementIndicators = null;
		}
	}
}