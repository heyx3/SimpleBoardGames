using System;
using System.Collections.Generic;
using UnityEngine;


namespace Fitchneil
{
	public class Piece : BoardGames.Piece<Vector2i>
	{
		public static readonly BoardGames.Players Defenders = BoardGames.Players.One,
												  Attackers = BoardGames.Players.Two;
		

		public static IEnumerable<Piece> AttackerPieces { get { return attackerPs; } }
		public static IEnumerable<Piece> DefenderPieces { get { return defenderPs; } }

		private static List<Piece> attackerPs = new List<Piece>(),
								   defenderPs = new List<Piece>();


		public bool CanDragMe
		{
			get
			{
				State_PlayerTurn pt = StateMachine.Instance.CurrentState as State_PlayerTurn;
				return (pt != null && pt.CurrentPlayer == Owner);
			}
		}


		public bool IsKing = false;

		//UI stuff.
		public Transform[] DraggingTracers = new Transform[2];
		public Transform PrevPosIndicator;
		private static List<Movement> moves = null;
		private static List<SpriteRenderer> movementIndicators;


		public override void OnDrag(Vector2 startMouseWorldPos, Vector2 currentMouseWorldPos)
		{
			if (!CanDragMe || moves == null)
				return;

			MyTr.position = currentMouseWorldPos;
			
			Vector2i newPosI = Board.ToBoard(currentMouseWorldPos);
			Vector3 oldPos = Board.ToWorld(CurrentPos),
					newPos = Board.ToWorld(newPosI);

			//Find whether a valid move is currently indicated.
			bool isValidMove = false;
			for (int i = 0; i < moves.Count; ++i)
			{
				if (moves[i].Pos == newPosI)
				{
					isValidMove = true;
					break;
				}
			}

			//Update UI elements.
			for (int i = 0; i < DraggingTracers.Length; ++i)
			{
				DraggingTracers[i].position = newPos;
				DraggingTracers[i].gameObject.SetActive(true);

				DraggingTracers[i].GetComponent<SpriteRenderer>().color = (isValidMove ?
					Constants.Instance.ValidMovement :
					Constants.Instance.InvalidMovement);
			}
			PrevPosIndicator.position = oldPos;
			PrevPosIndicator.gameObject.SetActive(true);
		}
		public override void OnStartClick(Vector2 mouseWorldPos)
		{
			if (!CanDragMe)
				return;

			ClearMoves();
			SetUpMoves();
		}
		public override void OnStopClick(Vector2 mouseWorldPos)
		{

		}
		public override void OnStopDrag(Vector2 startMouseWorldPos, Vector2 endMouseWorldPos)
		{
			if (!CanDragMe || moves == null)
				return;

			
			//Deactivate the UI stuff.
			for (int i = 0; i < DraggingTracers.Length; ++i)
			{
				DraggingTracers[i].gameObject.SetActive(false);
			}
			PrevPosIndicator.gameObject.SetActive(false);
			
			//Move this piece back to its current position.
			MyTr.position = Board.ToWorld(CurrentPos);

			//If the user indicated a valid move, execute it.
			Vector2i newPosI = Board.ToBoard(endMouseWorldPos);
			if (newPosI != CurrentPos)
			{
				for (int i = 0; i < moves.Count; ++i)
				{
					if (moves[i].Pos == newPosI)
					{
						((State_PlayerTurn)StateMachine.Instance.CurrentState).ExecuteMove(moves[i]);
						ClearMoves();

						return;
					}
				}
			}
		}


		protected override void Awake()
		{
			base.Awake();

			UnityEngine.Assertions.Assert.IsTrue(!IsKing || Owner == Defenders,
												 "King piece must be on team " + Defenders.ToString() + "!");

			if (Owner == Attackers)
				attackerPs.Add(this);
			else
				defenderPs.Add(this);
		}
		void OnDestroy()
		{
			if (Owner == Attackers)
				attackerPs.Remove(this);
			else
				defenderPs.Remove(this);
		}

		private void SetUpMoves()
		{
			moves = new List<Movement>(Board.Instance.GetMoves(this));
			movementIndicators = SpritePool.Instance.AllocateSprites(moves.Count,
																	 Constants.Instance.Movement,
																	 1, null, "Movement Indicator");

			for (int i = 0; i < moves.Count; ++i)
			{
				movementIndicators[i].transform.position = Board.ToWorld(moves[i].Pos);

				if (moves[i].GetIsSpecial())
				{
					movementIndicators[i].color = Constants.Instance.SpecialMovementIndicator;
				}
				else
				{
					movementIndicators[i].color = Constants.Instance.NormalMovementIndicator;
				}
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