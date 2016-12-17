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
	public class Movement<LocationType>
	{
		/// <summary>
		/// The new position after the move.
		/// </summary>
		public Stat<LocationType, Movement<LocationType>> Pos { get; private set; }
		/// <summary>
		/// The piece that is being moved.
		/// </summary>
		public Stat<Piece<LocationType>, Movement<LocationType>> IsMoving { get; private set; }

		
		public Movement(LocationType pos, Piece<LocationType> isMoving)
		{
			Pos = new Stat<LocationType, Movement<LocationType>>(this, pos);
			IsMoving = new Stat<Piece<LocationType>, Movement<LocationType>>(this, isMoving);
		}


		/// <summary>
		/// Default behavior: Sets the piece's position
		///     and raises the board's "OnMove" event.
		/// </summary>
		public virtual void ApplyMove()
		{
			IsMoving.Value.CurrentPos.Value = Pos;
			IsMoving.Value.TheBoard.RaiseEvent_Movement(this);
		}

		//TODO: Also have to be able to "undo move". Do the same thing with Placement. Then have the Board keep track of an undo stack.
	}
}