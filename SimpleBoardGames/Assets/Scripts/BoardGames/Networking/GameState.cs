using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BoardGames.Networking
{
	public enum TurnStates
	{
		YourTurn, OtherTurn,
		YouWon, OtherWon, Tie,
	}

	public struct GameState
	{
		public ulong SessionID { get; private set; }

		public byte[] BoardState { get; private set; }
		public byte[] LastAction { get; private set; }

		public byte CurrentPlayerID { get; private set; }

		public TurnStates TurnState { get; private set; }


		public GameState(ulong sessionID, byte currentPlayerID,
						 byte[] boardState, byte[] lastAction, TurnStates state)
		{
			SessionID = sessionID;
			CurrentPlayerID = currentPlayerID;

			BoardState = boardState;
			LastAction = lastAction;
			TurnState = state;
		}


		public GameState TakeTurn(byte[] newAction, byte[] newBoardState, TurnStates newTurn)
		{
			byte nextPlayer;
			switch (CurrentPlayerID)
			{
				case 0: nextPlayer = 1; break;
				case 1: nextPlayer = 0; break;
				default: throw new NotImplementedException(CurrentPlayerID.ToString());
			}

			return new GameState(SessionID, nextPlayer, newBoardState, newAction, newTurn);
		}
	}
}
