using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FiaCR.UnityLogic
{
	[RequireComponent(typeof(AnimateTransform))]
	public class FCR_ResetGameButton : InputResponder
	{
		public GameObject ConfirmationWindow;


		protected override void Awake()
		{
			base.Awake();

			OnStopClick += (_this, mPos) =>
			{
				ConfirmationWindow.SetActive(true);

				GetComponent<AnimateTransform>().AnimationDir = 1;
			};
		}
		private void Start()
		{
			GameMode.FCR_Game_Offline.Instance.CurrentTurn.OnChanged += Callback_NewTurn;

			ConfirmationWindow.SetActive(false);
		}
		private void OnDestroy()
		{
			if (GameMode.FCR_Game_Offline.Instance != null)
				GameMode.FCR_Game_Offline.Instance.CurrentTurn.OnChanged -= Callback_NewTurn;
		}

		private void Callback_NewTurn(BoardGames.UnityLogic.GameMode.GameMode<Vector2i> _gameMode,
									  BoardGames.Players oldTeam, BoardGames.Players newTeam)
		{
			enabled = (newTeam == Board.Player_Humans);
		}
	}
}
