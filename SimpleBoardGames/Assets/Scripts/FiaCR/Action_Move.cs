using System;
using System.Collections.Generic;
using System.IO;
using BoardGames;

namespace FiaCR
{
	public class Action_Move : Action_Base
	{
		public Piece ToMove { get; private set; }
		public Vector2i NewPos { get; private set; }

		private Vector2i oldPos;
		private bool movedOffHost;


		protected override Players Team {  get { return ToMove.Owner; } }


		public Action_Move(Piece toMove, Vector2i newPos,
						   HashSet<Piece> captures, Vector2i? hostBlockMinCorner)
			: base(captures, hostBlockMinCorner, (Board)toMove.TheBoard)
		{
			ToMove = toMove;
			NewPos = newPos;
		}


		protected override void Action_Do()
		{
			base.Action_Do();

			oldPos = ToMove.CurrentPos;
			movedOffHost = ((Board)TheBoard).GetHost(oldPos).HasValue;

			ToMove.CurrentPos.Value = NewPos;
		}
		protected override void Action_Undo()
		{
			base.Action_Undo();

			if (movedOffHost)
				((Board)TheBoard).RemovePiece(oldPos);

			ToMove.CurrentPos.Value = oldPos;
		}

		public override void Serialize(BinaryWriter stream)
		{
			base.Serialize(stream);

			//Write the piece to move, by its position.
			UnityEngine.Assertions.Assert.IsTrue(ToMove.CurrentPos.Value.x <= byte.MaxValue);
			UnityEngine.Assertions.Assert.IsTrue(ToMove.CurrentPos.Value.y <= byte.MaxValue);
			stream.Write((byte)ToMove.CurrentPos.Value.x);
			stream.Write((byte)ToMove.CurrentPos.Value.y);
			
			UnityEngine.Assertions.Assert.IsTrue(NewPos.x <= byte.MaxValue);
			UnityEngine.Assertions.Assert.IsTrue(NewPos.y <= byte.MaxValue);
			stream.Write((byte)NewPos.x);
			stream.Write((byte)NewPos.y);
		}
		public override void Deserialize(BinaryReader stream)
		{
			base.Deserialize(stream);

			ToMove = ((Board)TheBoard).GetPiece(new Vector2i(stream.ReadByte(), stream.ReadByte()));
			NewPos = new Vector2i(stream.ReadByte(), stream.ReadByte());
		}
	}
}
