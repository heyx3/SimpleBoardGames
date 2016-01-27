using System;
using System.Collections.Generic;
using UnityEngine;


namespace TronsTour
{
	public class StateMachine : BoardGames.StateMachine<Board, Piece, Vector2i, Movement>
	{
		public abstract class State : BoardGames.State<Board, Piece, Vector2i, Movement>
		{
			protected static Board Brd { get { return (Board)Board.Instance; } }
			protected static Constants Consts { get { return Constants.Instance; } }
		}

		void Start()
		{
			CurrentState = new State_Init();
		}
	}
}