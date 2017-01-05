using System;
using System.Collections.Generic;
using System.IO;


namespace FiaCR
{
	public class Action_PlaceFriendly : Action_Base
	{
		public Vector2i Pos { get; private set; }


		protected override BoardGames.Players Team { get { return Board.Player_Humans; } }


		public Action_PlaceFriendly(Vector2i pos, HashSet<Piece> captures, Board theBoard)
			: base(captures, theBoard)
		{
			Pos = pos;
		}


		public override void DoAction()
		{
			((Board)TheBoard).AddPiece(new Piece(Pos, Board.Player_Humans, (Board)TheBoard));
			base.DoAction();
		}
		public override void UndoAction()
		{
			((Board)TheBoard).RemovePiece(Pos);
			base.UndoAction();
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