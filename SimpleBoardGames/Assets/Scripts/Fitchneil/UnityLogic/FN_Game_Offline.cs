using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Fitchneil.UnityLogic.GameMode
{
	public class FN_Game_Offline : BoardGames.UnityLogic.GameMode.GameMode_Offline<Vector2i>
	{
		protected override string GameName { get { return "Fitchneil"; } }


		protected override BoardGames.Board<Vector2i> CreateNewBoard()
		{
			return new Board();
		}
		protected override void OnAction(BoardGames.Action<Vector2i> action)
		{
			//Change turns.
			CurrentTurn.Value = CurrentTurn.Value.Switched();
		}

		protected override void Awake()
		{
			base.Awake();

			Screen.orientation = ScreenOrientation.Portrait;
		}
	}
}
