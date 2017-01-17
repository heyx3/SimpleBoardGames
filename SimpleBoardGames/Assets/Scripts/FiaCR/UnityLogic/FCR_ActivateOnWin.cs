using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FiaCR.UnityLogic
{
	public class FCR_ActivateOnWin : BoardGames.UnityLogic.ActivateOnWin<Vector2i>
	{
		public bool ActivateOnFriendsWin = false,
					ActivateOnCurseWin = false;

		protected override bool ShouldActivate(BoardGames.Players? player)
		{
			return (ActivateOnFriendsWin && player.Value == Board.Player_Humans) ||
				   (ActivateOnCurseWin && player.Value == Board.Player_TC);
		}
	}
}