using System;
using UnityEngine;


namespace BoardGames
{
	/// <summary>
	/// Represents a movement of a piece towards the given position.
	/// </summary>
	/// <typeparam name="LocationType">
	/// The data structure used to represent a location on the game board.
	/// </typeparam>
	public class Movement<LocationType, PieceType> where PieceType : Piece<LocationType>
	{
		public LocationType Pos { get; set; }
		public PieceType IsMoving { get; set; }


		public Movement() { }
		public Movement(LocationType pos, PieceType isMoving)
		{
			Pos = pos;
			IsMoving = isMoving;
		}
	}
}