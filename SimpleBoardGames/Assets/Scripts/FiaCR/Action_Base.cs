using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace FiaCR
{
	public abstract class Action_Base : BoardGames.Action<Vector2i>
	{
		public int NCaptures { get { return captures.Count; } }
		public IEnumerable<Piece> Captures { get { return captures; } }
		private HashSet<Piece> captures;

		/// <summary>
		/// Whether this action completes a 3x3 block of pieces.
		/// </summary>
		public Vector2i? HostBlockMinCorner { get; private set; }


		public bool IsSpecial
		{
			get
			{
				return NCaptures > 0 ||
					   HostBlockMinCorner.HasValue;
			}
		}

		protected abstract BoardGames.Players Team { get; }


		public Action_Base(HashSet<Piece> _captures, Vector2i? hostBlockMinCorner, Board board)
			: base(board)
		{
			captures = _captures;
			HostBlockMinCorner = hostBlockMinCorner;
		}


		protected override void Action_Do()
		{
			base.Action_Do();

			//For every captured piece, switch its team to this one.
			BoardGames.Players newTeam = Team;
			foreach (Piece p in captures)
				p.Owner.Value = newTeam;

			//If this completes a block, remove all pieces in the block and make a host.
			if (HostBlockMinCorner.HasValue)
			{
				Board board = (Board)TheBoard;
				Vector2i min = HostBlockMinCorner.Value;

				for (int y = min.y; y < min.y + 3; ++y)
					for (int x = min.x; x < min.x + 3; ++x)
						board.RemovePiece(new Vector2i(x, y));

				board.AddHost(new Vector2i(min.x + 1, min.y + 1), newTeam);
			}
		}
		protected override void Action_Undo()
		{
			base.Action_Undo();

			//For every captured piece, switch its team back.
			BoardGames.Players oldTeam = Team.Switched();
			foreach (Piece p in captures)
				p.Owner.Value = oldTeam;

			//If this completed a block, re-add all other pieces in the block and remove the host.
			if (HostBlockMinCorner.HasValue)
			{
				Board board = (Board)TheBoard;
				Vector2i min = HostBlockMinCorner.Value;

				board.RemoveHost(min);

				for (int y = min.y; y < min.y + 3; ++y)
					for (int x = min.x; x < min.x + 3; ++x)
						board.AddPiece(new Piece(new Vector2i(x, y), Team, board));
			}
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

			stream.Write(HostBlockMinCorner.HasValue);
			if (HostBlockMinCorner.HasValue)
			{
				UnityEngine.Assertions.Assert.IsTrue(HostBlockMinCorner.Value.x <= byte.MaxValue);
				UnityEngine.Assertions.Assert.IsTrue(HostBlockMinCorner.Value.y <= byte.MaxValue);
				stream.Write((byte)HostBlockMinCorner.Value.x);
				stream.Write((byte)HostBlockMinCorner.Value.y);
			}
		}
		public override void Deserialize(BinaryReader stream)
		{
			captures.Clear();

			Board board = (Board)TheBoard;
			int nCaps = (int)stream.ReadByte();
			for (int i = 0; i < nCaps; ++i)
			{
				captures.Add(board.GetPiece(new Vector2i(stream.ReadByte(),
														 stream.ReadByte())));
			}

			if (stream.ReadBoolean())
			{
				HostBlockMinCorner = new Vector2i(stream.ReadByte(), stream.ReadByte());
			}
			else
			{
				HostBlockMinCorner = null;
			}
		}
	}
}
