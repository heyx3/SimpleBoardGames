using System;
using System.Collections.Generic;
using UnityEngine;


namespace TronsTour
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
			foreach (Piece p in Brd.Pieces)
				p.MyCollider.enabled = false;

			//Let the UI show for a few seconds, then end the game.
			Consts.WinnerUIs[(int)Winner].SetActive(true);
			yield return new WaitForSeconds(2.5f);
			UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
		}
		public override void OnExitingState()
		{
			
		}
	}
}