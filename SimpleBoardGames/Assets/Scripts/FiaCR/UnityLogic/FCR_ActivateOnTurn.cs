using System;
using UnityEngine;


namespace FiaCR.UnityLogic
{
	public class FCR_ActivateOnTurn : MonoBehaviour
	{
		public bool ActivateOnBilly = false,
					ActivateOnJulia = false,
					ActivateOnCursed = false;

		private void Start()
		{
			var gameMode = (GameMode.FCR_Game_Offline)GameMode.FCR_Game_Offline.Instance;

			gameMode.OnTurnChanged += Callback_NewTurn;
			Callback_NewTurn(gameMode);
		}
		private void Callback_NewTurn(GameMode.FCR_Game_Offline gameMode)
		{
			if (gameMode.CurrentTurn == Board.Player_Humans && gameMode.IsJuliaTurn)
				gameObject.SetActive(ActivateOnJulia);
			else if (gameMode.CurrentTurn == Board.Player_Humans && !gameMode.IsJuliaTurn)
				gameObject.SetActive(ActivateOnBilly);
			else if (gameMode.CurrentTurn == Board.Player_TC)
				gameObject.SetActive(ActivateOnCursed);
			else
				Debug.LogError("Unknown turn value " + gameMode.CurrentTurn.Value.ToString());
		}
	}
}