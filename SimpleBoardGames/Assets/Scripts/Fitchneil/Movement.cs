using System;
using System.Collections.Generic;
using UnityEngine;


namespace Fitchneil
{
	public class Movement : BoardGames.Movement<Vector2i>
	{
		/// <summary>
		/// The pieces that would be captured if this move was taken.
		/// </summary>
		public HashSet<Piece> Captures { get; private set; }

		
		public Movement(Vector2i pos, Piece isMoving, IEnumerable<Piece> captures)
			: base(pos, isMoving)
		{
			Captures = new HashSet<Piece>(captures);
		}


		/// <summary>
		/// Gets whether this move is special in some way.
		/// This is used by the UI when showing the player what moves are possible.
		/// </summary>
		public bool GetIsSpecial()
		{
			//The move is special if it captures at least one piece or if it lets the king escape.
			return Captures.Count > 0 ||
				   (((Piece)IsMoving.Value).IsKing &&
				    (Pos.Value.x == 0 || Pos.Value.x == Board.BoardSize - 1 ||
					 Pos.Value.y == 0 || Pos.Value.y == Board.BoardSize - 1));
		}

		public override void ApplyMove()
		{
			base.ApplyMove();

			Piece capturer = (Piece)IsMoving.Value;
			Board board = (Board)capturer.TheBoard;
			foreach (Piece piece in Captures)
				board.CapturePiece(piece, capturer);
		}
	}
}