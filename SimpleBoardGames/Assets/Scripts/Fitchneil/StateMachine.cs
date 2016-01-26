using System;
using UnityEngine;


namespace Fitchneil
{
	public class StateMachine : BoardGames.StateMachine<Board, Piece, Vector2i, Movement>
	{
		/// <summary>
		/// The base game state for Fitchneil.
		/// </summary>
		public abstract class State : BoardGames.State<Board, Piece, Vector2i, Movement>
		{
			protected static StateMachine FSM { get { return (StateMachine)StateMachine.Instance; } }
			protected static Board Brd { get { return (Board)Board.Instance; } }
			protected static Constants Consts { get { return Constants.Instance; } }
		}


		void Start()
		{
			CurrentState = new State_Init();
		}
	}
}