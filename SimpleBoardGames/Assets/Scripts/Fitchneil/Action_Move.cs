using System;
using System.Collections.Generic;
using UnityEngine;


namespace Fitchneil
{
	public class Action_Move : BoardGames.Action<Vector2i>
	{
		/// <summary>
		/// The pieces that would be captured if this move was taken.
		/// </summary>
		public IEnumerable<Piece> Captures { get { return captures; } }
		public int NCaptures { get { return captures.Count; } }
		private HashSet<Piece> captures;

		public Vector2i StartPos { get; private set; }
		public Vector2i EndPos { get; private set; }

		public Piece ThePiece { get; private set; }

		
		public Action_Move(Vector2i pos, Piece isMoving, IEnumerable<Piece> _captures)
			: base(isMoving.TheBoard)
		{
			ThePiece = isMoving;
			StartPos = ThePiece.CurrentPos;
			EndPos = pos;

			captures = new HashSet<Piece>(_captures);
		}


		/// <summary>
		/// Gets whether this move is special in some way.
		/// This is used by the UI when showing the player what moves are possible.
		/// </summary>
		public bool GetIsSpecial()
		{
			//The move is special if it captures at least one piece or if it lets the king escape.
			return captures.Count > 0 ||
				   (ThePiece.IsKing &&
				    (EndPos.x == 0 || EndPos.x == Board.BoardSize - 1 ||
					 EndPos.y == 0 || EndPos.y == Board.BoardSize - 1));
		}

		public override void DoAction()
		{
			ThePiece.CurrentPos.Value = EndPos;

			//Capture pieces.
			Board board = (Board)TheBoard;
			foreach (Piece piece in captures)
				board.CapturePiece(piece, ThePiece);

			base.DoAction();
		}
		public override void UndoAction()
		{
			//Put back every piece that was captured.
			Board board = (Board)TheBoard;
			foreach (Piece piece in captures)
				board.PlacePiece(piece);

			base.UndoAction();
		}
	}
}