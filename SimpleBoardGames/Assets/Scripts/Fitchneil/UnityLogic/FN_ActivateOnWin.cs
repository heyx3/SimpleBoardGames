using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Fitchneil.UnityLogic
{
	public class FN_ActivateOnWin : MonoBehaviour
	{
		public bool ActivateOnDefenderWin = false,
					ActivateOnAttackerWin = false;

		private void Start()
		{
			var gameMode = BoardGames.UnityLogic.GameMode.GameMode<Vector2i>.Instance;
			
			if (gameMode is GameMode.FN_Game_Offline)
			{
				((GameMode.FN_Game_Offline)gameMode).OnPlayerWin += (board, player) =>
				{
					gameObject.SetActive((ActivateOnDefenderWin &&
											 player.Value == Board.Player_Defender) ||
										 (ActivateOnAttackerWin &&
											 player.Value == Board.Player_Attacker));
				};
			}

			gameObject.SetActive(false);
		}
	}
}
