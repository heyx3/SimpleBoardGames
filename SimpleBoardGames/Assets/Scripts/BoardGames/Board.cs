using System;
using System.Collections.Generic;
using System.Linq;


namespace BoardGames
{
	public abstract class Board<LocationType>
		where LocationType : IEquatable<LocationType>
	{
		//TODO: Keep a stack of actions to undo/redo, and add UI for it.

		public event System.Action<Board<LocationType>,
								   BoardGames.Action<LocationType>> OnAction,
																	OnUndoAction;

		/// <summary>
		/// Raised when the game is over.
		/// The second parameter is the winner, or null if there was a tie.
		/// </summary>
		public event System.Action<Board<LocationType>, Players?> OnGameFinished;


		public void DidAction(Action<LocationType> action)
		{
			if (OnAction != null)
				OnAction(this, action);
		}
		public void UndidAction(Action<LocationType> action)
		{
			if (OnUndoAction != null)
				OnUndoAction(this, action);
		}
		public void FinishedGame(Players? winner)
		{
			if (OnGameFinished != null)
				OnGameFinished(this, winner);
		}


		/// <summary>
		/// Gets all pieces on this board.
		/// </summary>
		public abstract IEnumerable<Piece<LocationType>> GetPieces();
		/// <summary>
		/// Gets all pieces on the board that satisfy the given condition.
		/// </summary>
		public IEnumerable<Piece<LocationType>> GetPieces(Predicate<Piece<LocationType>> predicate)
		{
			return GetPieces().Where(p => predicate(p));
		}

		/// <summary>
		/// Gets all possible actions the given piece can currently perform.
		/// Default behavior: returns an empty container.
		/// </summary>
		public virtual IEnumerable<Action<LocationType>> GetActions(Piece<LocationType> piece) { yield break; }
		/// <summary>
		/// Gets all possible actions the given player can currently perform.
		/// Note that this should be used for any actions that aren't tied to a specific piece.
		/// </summary>
		public virtual IEnumerable<Action<LocationType>> GetActions(Players player) { yield break; }


		//The board must be serializable to and from a byte stream.
		public abstract void Serialize(System.IO.BinaryWriter stream);
		public abstract void Deserialize(System.IO.BinaryReader stream);
	}
}