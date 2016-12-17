using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoardGames
{
	/// <summary>
	/// Represents a placement of a new piece on the board.
	/// </summary>
	/// <typeparam name="LocationType">
	/// The data structure used to represent a location on the game board.
	/// </typeparam>
	public abstract class Placement<LocationType>
		where LocationType : IEquatable<LocationType>
	{
		/// <summary>
		/// The spot where the piece will be placed at.
		/// </summary>
		public Stat<LocationType, Placement<LocationType>> Pos { get; private set; }

		public Board<LocationType> TheBoard { get; private set; }


		public Placement(LocationType pos, Board<LocationType> theBoard)
		{
			Pos = new Stat<LocationType, Placement<LocationType>>(this, pos);
			TheBoard = theBoard;
		}
		

		/// <summary>
		/// Default behavior: raises the board's "OnPlace" event.
		/// </summary>
		public virtual void Place()
		{
			TheBoard.RaiseEvent_Placement(this);
		}
	}
}
