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


		/// <summary>
		/// Gets all pieces on this board.
		/// </summary>
		public abstract IEnumerable<Piece<LocationType>> GetPieces();
		/// <summary>
		/// Gets all pieces located at the given position.
		/// Default behavior: just calls "GetPieces()" and filters it by position.
		/// </summary>
		public virtual IEnumerable<Piece<LocationType>> GetPieces(LocationType space)
		{
			return GetPieces().Where(p => p.CurrentPos.Value.Equals(space));
		}
		/// <summary>
		/// Gets all game pieces belonging to the given player.
		/// Default behavior: just calls "GetPieces()" and filters it by team.
		/// </summary>
		public virtual IEnumerable<Piece<LocationType>> GetPieces(Players team)
		{
			return GetPieces().Where(p => p.Owner.Value == team);
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