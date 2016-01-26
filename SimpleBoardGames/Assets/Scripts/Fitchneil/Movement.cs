using System;
using System.Collections.Generic;
using UnityEngine;
using BoardGames;


namespace Fitchneil
{
	public struct Movement : IMovement<Vector2i, Piece>
	{
		public Piece IsMoving { get { return p; } set { p = value; } }
		public Vector2i Pos { get { return pos; } set { pos = value; } }

		/// <summary>
		/// The pieces that would be captured if this move was taken.
		/// </summary>
		public List<Piece> Captures { get { return caps; } set { caps = value; } }

		private Piece p;
		private Vector2i pos;
		private List<Piece> caps;


		public Movement(Vector2i _pos, Piece isMoving, List<Piece> captures)
		{
			pos = _pos;
			p = isMoving;
			caps = captures;
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