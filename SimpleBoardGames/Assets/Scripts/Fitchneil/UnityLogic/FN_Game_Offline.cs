using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Fitchneil.UnityLogic.GameMode
{
	public class FN_Game_Offline : BoardGames.UnityLogic.GameMode.GameMode_Offline<Vector2i>
	{
		protected override string GameName { get { return "Fitchneil"; } }


		protected override BoardGames.Board<Vector2i> CreateNewBoard()
		{
			return new Board();
		}
		protected override void OnAction(BoardGames.Action<Vector2i> action)
		{
			//Change turns.
			CurrentTurn.Value = CurrentTurn.Value.Switched();
		}

		protected override void Awake()
		{
			base.Awake();
			
			//If the king moves to the edge of the board, the defenders win.
			var kingPiece = TheBoard.GetPieces().First(p => ((Piece)p).IsKing);
			kingPiece.CurrentPos.OnChanged += (_kingPiece, oldPos, newPos) =>
			{
				if (newPos.x == 0 || newPos.x == Board.BoardSize - 1 ||
					newPos.y == 0 || newPos.y == Board.BoardSize - 1)
				{
					EndGame(Board.Player_Defender);
				}
			};

			//If the king is captured, the attackers win.
			//If all attackers are captured, the defenders win.
			((Board)TheBoard).OnPieceCaptured += (theBoard, captured, capturer) =>
			{
				if (captured.IsKing)
					EndGame(Board.Player_Attacker);
				else if (theBoard.GetPieces(Board.Player_Attacker).Count() == 0)
					EndGame(Board.Player_Defender);
			};
		}
	}
}
