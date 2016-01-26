using System;
using System.Collections;
using UnityEngine;


namespace BoardGames
{
	/// <summary>
	/// A state in the game's state machine.
	/// </summary>
	public abstract class State<BoardType, PieceType, LocationType, MoveType>
		where PieceType : Piece<LocationType>
		where LocationType : struct
		where MoveType : struct, IMovement<LocationType, PieceType>
		where BoardType : Board<PieceType, LocationType, MoveType>
	{
		public abstract IEnumerator RunLogicCoroutine();
		public abstract void OnExitingState();
	}


	/// <summary>
	/// The state machine managing various game states.
	/// </summary>
	public abstract class StateMachine<BoardType, PieceType, LocationType, MoveType>
		: Singleton<StateMachine<BoardType, PieceType, LocationType, MoveType>>
		where PieceType : Piece<LocationType>
		where LocationType : struct
		where MoveType : struct, IMovement<LocationType, PieceType>
		where BoardType : Board<PieceType, LocationType, MoveType>
	{
		/// <summary>
		/// The current state of the game, or "null" if no state is currently running.
		/// </summary>
		public State<BoardType, PieceType, LocationType, MoveType> CurrentState
		{
			get { return currentState; }
			set
			{
				if (currentState != null)
				{
					StopCoroutine(currentStateCor);
					currentState.OnExitingState();
				}

				currentState = value;

				if (currentState != null)
				{
					currentStateCor = StartCoroutine(currentState.RunLogicCoroutine());
				}
			}
		}

		private State<BoardType, PieceType, LocationType, MoveType> currentState;
		private Coroutine currentStateCor = null;


		protected override void Awake()
		{
			base.Awake();

			currentState = null;
		}
	}
}