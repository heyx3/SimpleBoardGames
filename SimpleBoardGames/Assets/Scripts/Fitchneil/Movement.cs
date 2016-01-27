using System;
using System.Collections.Generic;
using UnityEngine;


namespace Fitchneil
{
	public class Movement : BoardGames.Movement<Vector2i, Piece>
	{
		/// <summary>
		/// The pieces that would be captured if this move was taken.
		/// </summary>
		public List<Piece> Captures { get; set; }


		public Movement() { }
		public Movement(Vector2i pos, Piece isMoving, List<Piece> captures)
			: base(pos, isMoving)
		{
			Captures = captures;
		}


		/// <summary>
		/// Gets whether this move is special in some way.
		/// </summary>
		public bool GetIsSpecial()
		{
			//The move is special if it captures at least one piece or if it lets the king escape.
			return Captures.Count > 0 ||
				   (IsMoving.IsKing && (Pos.x == 0 || Pos.x == Board.BoardSize - 1 ||
										Pos.y == 0 || Pos.y == Board.BoardSize - 1));
		}
	}
}