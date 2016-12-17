using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BoardGames;

namespace Fitchneil.UnityLogic.GameMode
{
	public class FN_Game_Offline : BoardGames.UnityLogic.GameMode.GameMode_Offline<Vector2i>
	{
		protected override string GameName { get { return "Fitchneil"; } }

		protected override BoardGames.Board<Vector2i> CreateNewBoard()
		{
			return new Board();
		}
		protected override void OnMove(Movement<Vector2i> move)
		{
			//Change turns.
			CurrentTurn.Value = CurrentTurn.Value.Switched();
		}
		protected override void OnPlace(Placement<Vector2i> place)
		{
			throw new NotImplementedException("No placements in Fitchneil!");
		}
	}
}
