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


		public void DoAction()
		{
			Action_Do();
			TheBoard.DidAction(this);
		}
		public void UndoAction()
		{
			Action_Undo();
			TheBoard.UndidAction(this);
		}
		protected virtual void Action_Do() { }
		protected virtual void Action_Undo() { }

		public abstract void Serialize(System.IO.BinaryWriter stream);
		public abstract void Deserialize(System.IO.BinaryReader stream);
	}
}