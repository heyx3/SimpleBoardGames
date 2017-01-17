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

			gameMode.CurrentTurn.OnChanged += (_gameMode, oldTurn, newTurn) =>
			{
				if (newTurn == Board.Player_Humans && gameMode.IsJuliaTurn)
					gameObject.SetActive(ActivateOnJulia);
				else if (newTurn == Board.Player_Humans && !gameMode.IsJuliaTurn)
					gameObject.SetActive(ActivateOnBilly);
				else if (newTurn == Board.Player_TC)
					gameObject.SetActive(ActivateOnCursed);
			};
		}
	}
}