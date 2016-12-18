using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Fitchneil
{
	public class Piece : BoardGames.Piece<Vector2i>
	{
		public bool IsKing { get; private set; }

		public Piece(bool isKing, Vector2i pos, BoardGames.Players owner, Board theBoard)
			: base(pos, owner, theBoard)
		{
			IsKing = isKing;
		}

		public override void Serialize(BinaryWriter stream)
		{
			base.Serialize(stream);
			stream.Write(IsKing);
		}
		public override void Deserialize(BinaryReader stream)
		{
			base.Deserialize(stream);
			IsKing = stream.ReadBoolean();
		}
		protected override void SerializeLocation(Vector2i l, BinaryWriter stream)
		{
			stream.Write(l.x);
			stream.Write(l.y);
		}
		protected override Vector2i DeserializeLocation(BinaryReader stream)
		{
			return new Vector2i(stream.ReadInt32(), stream.ReadInt32());
		}
	}
}