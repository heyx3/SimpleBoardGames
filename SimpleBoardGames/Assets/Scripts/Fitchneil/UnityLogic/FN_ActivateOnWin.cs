using System;
using System.Collections.Generic;
using System.Linq;
using BoardGames;
using UnityEngine;


namespace Fitchneil.UnityLogic
{
	public class FN_ActivateOnWin : BoardGames.UnityLogic.ActivateOnWin<Vector2i>
	{
		public bool ActivateOnDefenderWin = false,
					ActivateOnAttackerWin = false;

		protected override bool ShouldActivate(Players? player)
		{
			return (ActivateOnDefenderWin && player.Value == Board.Player_Defender) ||
				   (ActivateOnAttackerWin && player.Value == Board.Player_Attacker);
		}
	}
}
