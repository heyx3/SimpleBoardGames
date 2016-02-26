using System;
using System.Collections.Generic;
using UnityEngine;


namespace TronsTour
{
	public class State_Init : StateMachine.State
	{
		public override System.Collections.IEnumerator RunLogicCoroutine()
		{
			Screen.orientation = ScreenOrientation.Portrait;

			//Wait a bit, then start the first turn.
			yield return new WaitForSeconds(0.25f);
			StateMachine.Instance.CurrentState = new State_PlayTurns(BoardGames.Players.One);
		}
		public override void OnExitingState()
		{
			
		}
	}
}