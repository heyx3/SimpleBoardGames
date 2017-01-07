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
			((Board)TheBoard).AddPiece(new Piece(Pos, Board.Player_Humans, (Board)TheBoard));
		}
		protected override void Action_Undo()
		{
			base.Action_Undo();
			((Board)TheBoard).RemovePiece(Pos);
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