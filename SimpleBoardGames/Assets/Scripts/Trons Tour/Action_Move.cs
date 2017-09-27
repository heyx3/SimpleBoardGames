using System;
using System.IO;

namespace TronsTour
{
	public class Action_Move : BoardGames.Action<Vector2i>
	{
		public Vector2i StartPos { get; private set; }
		public Vector2i EndPos { get; private set; }

		public Piece ThePiece { get; private set; }


		public Action_Move(Vector2i pos, Piece isMoving)
			: base(isMoving.TheBoard)
		{
			ThePiece = isMoving;
			StartPos = ThePiece.CurrentPos;
			EndPos = pos;
		}


		protected override void Action_Do()
		{
			base.Action_Do();
			ThePiece.CurrentPos.Value = EndPos;
		}
		protected override void Action_Undo()
		{
			base.Action_Undo();
			ThePiece.CurrentPos.Value = StartPos;
		}

		public override void Serialize(BinaryWriter stream)
		{
			stream.Write((byte)ThePiece.Owner.Value);
			stream.Write((Int32)StartPos.x);
			stream.Write((Int32)StartPos.y);
			stream.Write((Int32)EndPos.x);
			stream.Write((Int32)EndPos.y);
		}
		public override void Deserialize(BinaryReader stream)
		{
			ThePiece = ((Board)TheBoard).GetPiece((BoardGames.Players)stream.ReadByte());
			StartPos = new Vector2i(stream.ReadInt32(), stream.ReadInt32());
			EndPos = new Vector2i(stream.ReadInt32(), stream.ReadInt32());
		}
	}
}