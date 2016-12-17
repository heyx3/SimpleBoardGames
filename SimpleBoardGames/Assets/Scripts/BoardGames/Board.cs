using System;
using System.Collections.Generic;
using System.Linq;


namespace BoardGames
{
	public abstract class Board<LocationType>
		where LocationType : IEquatable<LocationType>
	{
		public event Action<Board<LocationType>, Movement<LocationType>> OnMove;
		public event Action<Board<LocationType>, Placement<LocationType>> OnPlace;

		public void RaiseEvent_Movement(Movement<LocationType> movement)
		{
			if (OnMove != null)
				OnMove(this, movement);
		}
		public void RaiseEvent_Placement(Placement<LocationType> placement)
		{
			if (OnPlace != null)
				OnPlace(this, placement);
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
		/// Gets all possible moves the given piece can currently do.
		/// Default behavior: returns an empty container.
		/// </summary>
		public virtual IEnumerable<Movement<LocationType>> GetMoves(Piece<LocationType> piece) { yield break; }
		/// <summary>
		/// Gets all placements the given player can currently make.
		/// Default behavior: returns an empty container.
		/// </summary>
		public virtual IEnumerable<Placement<LocationType>> GetNewPlacements(Players player) { yield break; }

		
		//The board must be serializable to and from a byte stream.
		public abstract void Serialize(System.IO.BinaryWriter stream);
		public abstract void Deserialize(System.IO.BinaryReader stream);
	}
}