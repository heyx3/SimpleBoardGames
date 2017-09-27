using System;
using System.Collections.Generic;
using System.IO;
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

		protected override void Action_Do()
		{
			base.Action_Do();

			ThePiece.CurrentPos.Value = EndPos;

			//Capture pieces.
			Board board = (Board)TheBoard;
			foreach (Piece piece in captures)
				board.CapturePiece(piece, ThePiece);
		}
		protected override void Action_Undo()
		{
			base.Action_Undo();

			ThePiece.CurrentPos.Value = StartPos;

			//Put back every piece that was captured.
			Board board = (Board)TheBoard;
			foreach (Piece piece in captures)
				board.PlacePiece(piece);
		}

		public override void Serialize(BinaryWriter stream)
		{
			//Write the piece that is moving, using its position.
			stream.Write((Int32)ThePiece.CurrentPos.Value.x);
			stream.Write((Int32)ThePiece.CurrentPos.Value.y);

			stream.Write((Int32)StartPos.x);
			stream.Write((Int32)StartPos.y);
			stream.Write((Int32)EndPos.x);
			stream.Write((Int32)EndPos.y);

			//Write the pieces that will be captured, using their position.
			stream.Write((Int32)NCaptures);
			foreach (var piece in captures)
			{
				stream.Write((Int32)piece.CurrentPos.Value.x);
				stream.Write((Int32)piece.CurrentPos.Value.y);
			}
		}
		public override void Deserialize(BinaryReader stream)
		{
			Board myBoard = (Board)TheBoard;

			ThePiece = myBoard.GetPiece(new Vector2i(stream.ReadInt32(), stream.ReadInt32()));

			StartPos = new Vector2i(stream.ReadInt32(), stream.ReadInt32());
			EndPos = new Vector2i(stream.ReadInt32(), stream.ReadInt32());

			captures.Clear();
			int nCaps = stream.ReadInt32();
			for (int i = 0; i < nCaps; ++i)
			{
				captures.Add(myBoard.GetPiece(new Vector2i(stream.ReadInt32(),
														   stream.ReadInt32())));
			}
		}
	}
}