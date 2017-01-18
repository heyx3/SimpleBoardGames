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

			gameMode.CurrentTurn.OnChanged += Callback_NewTurn;
			Callback_NewTurn(gameMode, gameMode.CurrentTurn, gameMode.CurrentTurn);
		}
		private void Callback_NewTurn(BoardGames.UnityLogic.GameMode.GameMode<Vector2i> _gameMode,
									  BoardGames.Players oldTurn, BoardGames.Players newTurn)
		{
			var gameMode = (GameMode.FCR_Game_Offline)_gameMode;

			if (newTurn == Board.Player_Humans && gameMode.IsJuliaTurn)
				gameObject.SetActive(ActivateOnJulia);
			else if (newTurn == Board.Player_Humans && !gameMode.IsJuliaTurn)
				gameObject.SetActive(ActivateOnBilly);
			else if (newTurn == Board.Player_TC)
				gameObject.SetActive(ActivateOnCursed);
		}
	}
}