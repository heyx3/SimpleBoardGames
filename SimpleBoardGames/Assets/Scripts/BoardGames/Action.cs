using System;
using UnityEngine;


namespace BoardGames
{
	/// <summary>
	/// Represents a game action -- moving a piece, placing a piece, or whatever.
	/// </summary>
	/// <typeparam name="LocationType">
	/// The data structure used to represent a location on the game board.
	/// </typeparam>
	public abstract class Action<LocationType>
		where LocationType : IEquatable<LocationType>
	{
		public Board<LocationType> TheBoard { get; private set; }

		public Action(Board<LocationType> theBoard)
		{
			TheBoard = theBoard;
		}


		/// <summary>
		/// Default behavior: Raises the board's "OnAction" event.
		/// Note that child classes probably want to call this base implementation
		///     at the END of their implementation.
		/// </summary>
		public virtual void DoAction()
		{
			TheBoard.DidAction(this);
		}
		/// <summary>
		/// Default behavior: Raises the board's "OnUndoAction" event.
		/// Note that child classes probably want to call this base implementation
		///     at the END of their implementation.
		/// </summary>
		public virtual void UndoAction()
		{
			TheBoard.UndidAction(this);
		}

		public abstract void Serialize(System.IO.BinaryWriter stream);
		public abstract void Deserialize(System.IO.BinaryReader stream);
	}
}