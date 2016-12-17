using System;
using UnityEngine;


namespace Fitchneil.UnityLogic
{
	public class FN_ActivateOnTurn : MonoBehaviour
	{
		public BoardGames.Players Player;


		private void Start()
		{
			var gameMode = BoardGames.UnityLogic.GameMode.GameMode<Vector2i>.Instance;

			gameMode.CurrentTurn.OnChanged +=
				(_gameMode, oldTurn, newTurn) =>
				{
					gameObject.SetActive(newTurn == Player);
				};
			gameObject.SetActive(gameMode.CurrentTurn.Value == Player);
		}
	}
}