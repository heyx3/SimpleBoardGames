using System;
using UnityEngine;


namespace TronsTour
{
	public struct Movement : BoardGames.IMovement<Vector2i, Piece>
	{
		public Vector2i Pos { get { return pos; } set { pos = value; } }
		public Piece IsMoving { get { return isMoving; } set { isMoving = value; } }

		private Vector2i pos;
		private Piece isMoving;
	}
}