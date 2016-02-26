using System;
using System.Collections.Generic;
using UnityEngine;


namespace Fitchneil
{
	public class State_Init : StateMachine.State
	{
		public override System.Collections.IEnumerator RunLogicCoroutine()
		{
			Screen.orientation = ScreenOrientation.Portrait;

			//Wait a little bit and then start the first turn.
			yield return new WaitForSeconds(0.5f);
			StateMachine.Instance.CurrentState = new State_PlayerTurn(Piece.Attackers);
		}
		public override void OnExitingState() { }
	}
}