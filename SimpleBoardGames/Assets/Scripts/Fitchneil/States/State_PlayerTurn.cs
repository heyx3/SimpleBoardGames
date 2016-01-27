using System;
using System.Collections.Generic;
using UnityEngine;


namespace Fitchneil
{
	/// <summary>
	/// This state idles until it is told to execute a specific move,
	/// at which point it executes that move and either ends the game or switches to the next turn.
	/// </summary>
	public class State_PlayerTurn : StateMachine.State
	{
		public BoardGames.Players CurrentPlayer;

		private Movement moveToExecute = null;


		public State_PlayerTurn(BoardGames.Players currentPlayer)
		{
			CurrentPlayer = currentPlayer;
		}


		public void ExecuteMove(Movement m)
		{
			moveToExecute = m;
		}

		public override System.Collections.IEnumerator RunLogicCoroutine()
		{
			//Keep executing turns until the game ends.
			while (true)
			{
				//Update the UI based on whose turn it is.
				foreach (SpriteRenderer sr in Consts.AttackerTexts)
					sr.color = Consts.InitialAttackerTextColor;
				foreach (SpriteRenderer sr in Consts.DefenderTexts)
					sr.color = Consts.InitialDefenderTextColor;
				if (CurrentPlayer == Piece.Attackers)
				{
					foreach (SpriteRenderer sr in Consts.DefenderTexts)
						sr.color *= Consts.TurnColorMultiplier;
				}
				else
				{
					foreach (SpriteRenderer sr in Consts.AttackerTexts)
						sr.color *= Consts.TurnColorMultiplier;
				}


				//Wait until a move is queued up.
				while (moveToExecute == null)
					yield return null;


				//See if this move will end the game.

				bool endsGame = false;

				if (CurrentPlayer == Piece.Attackers)
				{
					//The attackers win if the king is captured.
					for (int i = 0; i < moveToExecute.Captures.Count; ++i)
					{
						if (moveToExecute.Captures[i].IsKing)
						{
							endsGame = true;
							break;
						}
					}
				}
				else
				{
					//The defenders win if the king escapes, or if not enough attackers will be left.
					if (moveToExecute.IsMoving.IsKing &&
						(moveToExecute.Pos.x == 0 || moveToExecute.Pos.x == Board.BoardSize - 1 ||
						 moveToExecute.Pos.y == 0 || moveToExecute.Pos.y == Board.BoardSize - 1))
					{
						endsGame = true;
					}
					else if (Brd.NAttackerPieces  - moveToExecute.Captures.Count < 3)
					{
						endsGame = true;
					}
				}


				//Execute the movement and switch turns.
				Board.Instance.ApplyMove(moveToExecute);
				CurrentPlayer = (CurrentPlayer == Piece.Attackers ?
									Piece.Defenders :
									Piece.Attackers);
				moveToExecute = null;
				yield return new WaitForSeconds(Consts.MovePieceTime);

				//If that move won the game, then end the game.
				if (endsGame)
				{
					StateMachine.Instance.CurrentState = new State_EndGame(CurrentPlayer == Piece.Attackers ?
																			   Piece.Defenders :
																			   Piece.Attackers);
					yield break;
				}
			}
		}
		public override void OnExitingState() { }
	}
}