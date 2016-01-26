using System;
using System.Collections.Generic;
using UnityEngine;


namespace BoardGames
{
	/// <summary>
	/// Represents a movement of a piece towards the given position.
	/// </summary>
	/// <typeparam name="LocationType">
	/// The data structure used to represent a location on the game board.
	/// </typeparam>
	public interface IMovement<LocationType, PieceType>
		where LocationType : struct
		where PieceType : Piece<LocationType>
	{
		LocationType Pos { get; set; }
		PieceType IsMoving { get; set; }
	}


	/// <summary>
	/// The game board. Is a Monobehavior singleton.
	/// </summary>
	/// <typeparam name="PieceType">The type of game piece found on the board.</typeparam>
	/// <typeparam name="LocationType">
	/// The data structure used to represent a location on the game board.
	/// </typeparam>
	/// <typeparam name="MoveType">
	/// The data structure used to represent a movement on this board.
	/// </typeparam>
	public abstract class Board<PieceType, LocationType, MoveType>
		: Singleton<Board<PieceType, LocationType, MoveType>>
		where PieceType : Piece<LocationType>
		where LocationType : struct
		where MoveType : struct, IMovement<LocationType, PieceType>
	{
		/// <summary>
		/// Gets the piece located at the given position, or null if no piece is there.
		/// </summary>
		public abstract PieceType GetPiece(LocationType space);
		/// <summary>
		/// Gets all game pieces belonging to the given player.
		/// </summary>
		public abstract IEnumerable<PieceType> GetPieces(Players team);

		/// <summary>
		/// Gets all possible moves the given piece can currently do.
		/// </summary>
		public abstract IEnumerable<MoveType> GetMoves(PieceType piece);
		/// <summary>
		/// Applies the given movement to the given piece.
		/// </summary>
		public abstract void ApplyMove(MoveType move);
	}
}