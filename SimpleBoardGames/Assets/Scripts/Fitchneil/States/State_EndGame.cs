using System;
using System.Collections.Generic;
using UnityEngine;


namespace Fitchneil
{
	public class State_EndGame : StateMachine.State
	{
		public BoardGames.Players Winner;


		public State_EndGame(BoardGames.Players winner)
		{
			Winner = winner;
		}


		public override System.Collections.IEnumerator RunLogicCoroutine()
		{
			//Disable the pieces from being moved.
			foreach (Piece p in Piece.AttackerPieces)
				p.MyCollider.enabled = false;
			foreach (Piece p in Piece.DefenderPieces)
				p.MyCollider.enabled = false;


			//Let the UI show for a few seconds, then end the game.
			if (Winner == Piece.Attackers)
			{
				Consts.AttackersWinUI.SetActive(true);
			}
			else
			{
				Consts.DefendersWinUI.SetActive(true);
			}
			yield return new WaitForSeconds(2.5f);
			UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
		}
		public override void OnExitingState() { }
	}
}