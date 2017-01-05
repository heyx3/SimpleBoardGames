using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace FiaCR
{
	public class Piece : BoardGames.Piece<Vector2i>
	{
		public Piece(Vector2i pos, BoardGames.Players owner, Board theBoard)
			: base(pos, owner, theBoard)
		{

		}


		public override void Serialize(BinaryWriter stream)
		{
			base.Serialize(stream);
		}
		public override void Deserialize(BinaryReader stream)
		{
			base.Deserialize(stream);
		}

		protected override void SerializeLocation(Vector2i l, BinaryWriter stream)
		{
			stream.Write((Int32)l.x);
			stream.Write((Int32)l.y);
		}
		protected override Vector2i DeserializeLocation(BinaryReader stream)
		{
			return new Vector2i(stream.ReadInt32(), stream.ReadInt32());
		}
	}
}