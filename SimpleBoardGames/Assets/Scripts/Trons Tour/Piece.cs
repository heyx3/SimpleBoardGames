using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace TronsTour
{
	public class Piece : BoardGames.Piece<Vector2i>
	{
		public Piece(Vector2i pos, BoardGames.Players owner, Board theBoard)
			: base(pos, owner, theBoard) { }

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