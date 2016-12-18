using System;
using System.Collections.Generic;
using System.Linq;
using BoardGames;
using UnityEngine;


namespace TronsTour.UnityLogic.GameMode
{
	public class TT_Game_Offline : BoardGames.UnityLogic.GameMode.GameMode_Offline<Vector2i>
	{
		public int Width = 6, Height = 9;

		protected override string GameName { get { return "Trons Tour"; } }


		protected override Board<Vector2i> CreateNewBoard()
		{
			return new Board(Width, Height);
		}
		protected override void OnAction(BoardGames.Action<Vector2i> move)
		{
			//Change turns.
			CurrentTurn.Value = CurrentTurn.Value.Switched();

			//If the current piece has no moves left, that player loses.
			Board board = (Board)TheBoard;
			if (board.GetActions(board.GetPiece(CurrentTurn)).Count() == 0)
				EndGame(CurrentTurn.Value.Switched());
		}

		protected override void Awake()
		{
			base.Awake();

			Screen.orientation = ScreenOrientation.Portrait;
		}
	}
}
