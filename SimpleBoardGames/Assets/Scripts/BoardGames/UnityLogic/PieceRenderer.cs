using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BoardGames.UnityLogic
{
	/// <summary>
	/// The Unity engine representation of a piece.
	/// Responds to mouse/touch inputs like clicking and dragging.
	/// </summary>
	/// <typeparam name="LocationType">
	/// The data structure used to represent positions on the game board.
	/// </typeparam>
	public abstract class PieceRenderer<LocationType> : InputResponder
		where LocationType : IEquatable<LocationType>
	{
		public Piece<LocationType> ToTrack
		{
			get { return toTrack; }
			set
			{
				ResetPieceFocus(toTrack, value);
				toTrack = value;
			}
		}
		private Piece<LocationType> toTrack = null;
		
		public SpriteRenderer Spr { get; protected set; }

		private Coroutine movePieceCoroutine = null;


		protected override void Awake()
		{
			base.Awake();
			Spr = GetComponentInChildren<SpriteRenderer>();
		}
		protected virtual void OnDestroy()
		{
			ToTrack = null;
		}

		/// <summary>
		/// Called when this renderer should start representing a new piece
		///     or at least stop representing the current one.
		/// </summary>
		/// <param name="oldPiece">The piece this instance previously tracked. May be null.</param>
		/// <param name="newPiece">The new piece this instance will track. May be null.</param>
		protected virtual void ResetPieceFocus(Piece<LocationType> oldPiece, Piece<LocationType> newPiece)
		{
			if (oldPiece != null)
				oldPiece.CurrentPos.OnChanged -= Callback_PieceMoved;
			if (newPiece != null)
				newPiece.CurrentPos.OnChanged += Callback_PieceMoved;
		}

		/// <summary>
		/// Runs the coroutine that animates the movement of the piece to the given position.
		/// </summary>
		protected abstract System.Collections.IEnumerator MovePieceCoroutine(LocationType startPos,
							 											     LocationType endPos);

		private void Callback_PieceMoved(Piece<LocationType> piece, LocationType oldPos, LocationType newPos)
		{
			if (movePieceCoroutine != null)
				StopCoroutine(movePieceCoroutine);

			movePieceCoroutine = StartCoroutine(MovePieceCoroutine(oldPos, newPos));
		}
	}
}
