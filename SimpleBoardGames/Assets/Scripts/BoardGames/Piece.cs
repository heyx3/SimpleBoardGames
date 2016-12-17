using System;
using System.Collections.Generic;


namespace BoardGames
{
	/// <summary>
	/// A piece on a game board.
	/// </summary>
	public abstract class Piece<LocationType>
	{
		public Stat<LocationType, Piece<LocationType>> CurrentPos { get; private set; }

		public Stat<Players, Piece<LocationType>> Owner { get; private set; }
		public Board<LocationType> TheBoard { get; private set; }


		public Piece(LocationType currentPos, Players owner, Board<LocationType> theBoard)
		{
			CurrentPos = new Stat<LocationType, Piece<LocationType>>(this, currentPos);
			Owner = new Stat<Players, Piece<LocationType>>(this, owner);
			TheBoard = theBoard;
		}

		
		public virtual void Serialize(System.IO.BinaryWriter stream)
		{
			SerializeLocation(CurrentPos.Value, stream);
		}
		public virtual void Deserialize(System.IO.BinaryReader stream)
		{
			CurrentPos.Value = DeserializeLocation(stream);
		}

		protected abstract void SerializeLocation(LocationType l, System.IO.BinaryWriter stream);
		protected abstract LocationType DeserializeLocation(System.IO.BinaryReader stream);
	}
}