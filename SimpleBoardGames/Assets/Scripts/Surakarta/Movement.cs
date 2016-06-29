using System;
using System.Collections.Generic;
using UnityEngine;


namespace Surakarta
{
	public class Movement : BoardGames.Movement<Vector2i, Piece>
	{
		//TODO: Use Multi-curve for all movements.

		public bool IsCapturing;
		public Vector2i PreviousPos;


		public Movement() : base() { }
		public Movement(Vector2i prevPos, Vector2i newPos, Piece thePiece, bool isACapture)
			: base(newPos, thePiece) { PreviousPos = prevPos; IsCapturing = isACapture; }
	}
}