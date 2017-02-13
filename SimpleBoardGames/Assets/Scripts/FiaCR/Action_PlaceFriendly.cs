using System;
using System.Collections.Generic;
using System.IO;


namespace FiaCR
{
	public class Action_PlaceFriendly : Action_Base
	{
		public Vector2i Pos { get; private set; }


		protected override BoardGames.Players Team { get { return Board.Player_Humans; } }


		public Action_PlaceFriendly(Vector2i pos, Board theBoard,
									HashSet<Piece> captures, Vector2i? hostBlockMinCorner)
			: base(captures, hostBlockMinCorner, theBoard)
		{
			Pos = pos;
		}


		protected override void Action_Do()
		{
			base.Action_Do();

			//If this piece completes a block of 3x3 to make a host, don't actually place it.
			if (!HostBlockMinCorner.HasValue)
			{
				Board theBoard = (Board)TheBoard;
				theBoard.AddPiece(new Piece(Pos, Board.Player_Humans, theBoard));
			}
		}
		protected override void Action_Undo()
		{
			base.Action_Undo();

			var board = (Board)TheBoard;
			if (board.GetPiece(Pos) != null)
				board.RemovePiece(Pos);
		}

		public override void Serialize(BinaryWriter stream)
		{
			base.Serialize(stream);
			stream.Write((Int32)Pos.x);
			stream.Write((Int32)Pos.y);
		}
		public override void Deserialize(BinaryReader stream)
		{
			base.Deserialize(stream);
			Pos = new Vector2i(stream.ReadInt32(), stream.ReadInt32());
		}
	}
}