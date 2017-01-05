using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace FiaCR
{
	public abstract class Action_Base : BoardGames.Action<Vector2i>
	{
		//TODO: Also store the 3x3 block this piece may complete to create a host.

		public int NCaptures { get { return captures.Count; } }
		public IEnumerable<Piece> Captures { get { return captures; } }

		private HashSet<Piece> captures;


		protected abstract BoardGames.Players Team { get; }


		public Action_Base(HashSet<Piece> _captures, Board board)
			: base(board)
		{
			captures = _captures;
		}


		public override void DoAction()
		{
			//For every captured piece, switch its team to this one.
			BoardGames.Players newTeam = Team;
			foreach (Piece p in captures)
				p.Owner.Value = newTeam;

			base.DoAction();
		}
		public override void UndoAction()
		{
			//For every captured piece, switch its team back.
			BoardGames.Players oldTeam = Team.Switched();
			foreach (Piece p in captures)
				p.Owner.Value = oldTeam;

			base.UndoAction();
		}

		public override void Serialize(BinaryWriter stream)
		{
			//Write the pieces that will be captured, by their position.
			UnityEngine.Assertions.Assert.IsTrue(captures.Count <= byte.MaxValue);
			stream.Write((byte)captures.Count);
			foreach (Piece p in captures)
			{
				UnityEngine.Assertions.Assert.IsTrue(p.CurrentPos.Value.x <= byte.MaxValue);
				UnityEngine.Assertions.Assert.IsTrue(p.CurrentPos.Value.y <= byte.MaxValue);

				stream.Write((byte)p.CurrentPos.Value.x);
				stream.Write((byte)p.CurrentPos.Value.y);
			}
		}
		public override void Deserialize(BinaryReader stream)
		{
			captures.Clear();

			Board board = (Board)TheBoard;
			int nCaps = (int)stream.ReadByte();
			for (int i = 0; i < nCaps; ++i)
			{
				captures.Add(board.GetPiece(new Vector2i((int)stream.ReadByte(),
														 (int)stream.ReadByte())));
			}
		}
	}
}
