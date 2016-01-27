using System;
using System.Collections.Generic;
using UnityEngine;


namespace TronsTour
{
	public class State_PlayTurns: StateMachine.State
	{
		public BoardGames.Players CurrentPlayer;
		public BoardGames.Players NextPlayer
		{
			get
			{
				return (BoardGames.Players)(((int)CurrentPlayer + 1) % 2);
			}
		}

		private Movement toExecute = null;


		public State_PlayTurns(BoardGames.Players currentPlayer)
		{
			CurrentPlayer = currentPlayer;
		}


		public void ExecuteTurn(Movement move)
		{
			UnityEngine.Assertions.Assert.IsNull(toExecute, "Two moves in one turn!");
			toExecute = move;
		}

		public override System.Collections.IEnumerator RunLogicCoroutine()
		{
			//Keep executing turns until the game ends.
			while (true)
			{
				//Update the UI based on whose turn it is.
				for (int i = 0; i < Consts.TextsForPlayers.Length; ++i)
				{
					foreach (SpriteRenderer sr in Consts.TextsForPlayers[i].Texts)
					{
						if ((int)CurrentPlayer == i)
							sr.color = Consts.TextsForPlayers[i].NormalColor;
						else sr.color = Consts.TextsForPlayers[i].NotMyTurnColor;
					}
				}

				//If there is no move for this player, he lost.
				List<Movement> moves =
					new List<Movement>(Board.Instance.GetMoves(Brd.GetPiece(CurrentPlayer)));
				if (moves.Count == 0)
				{
					StateMachine.Instance.CurrentState = new State_EndGame(NextPlayer);
					yield break;
				}

				//Wait until a move is queued up.
				Brd.GetPiece(NextPlayer).MyCollider.enabled = false;
				Brd.GetPiece(CurrentPlayer).MyCollider.enabled = true;
				while (toExecute == null)
					yield return null;


				//Execute the movement and switch turns.
				Board.Instance.ApplyMove(toExecute);
				CurrentPlayer = NextPlayer;
				toExecute = null;
				yield return new WaitForSeconds(Consts.MovePieceTime);
			}
		}
		public override void OnExitingState() { }
	}
}