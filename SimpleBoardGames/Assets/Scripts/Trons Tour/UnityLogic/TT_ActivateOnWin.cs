using System;
using System.Collections.Generic;
using System.Linq;
using BoardGames;
using UnityEngine;


namespace TronsTour.UnityLogic
{
	public class TT_ActivateOnWin : BoardGames.UnityLogic.ActivateOnWin<Vector2i>
	{
		public BoardGames.Players Player;
		
		protected override bool ShouldActivate(Players? player)
		{
			return player.Value == Player;
		}
	}
}
