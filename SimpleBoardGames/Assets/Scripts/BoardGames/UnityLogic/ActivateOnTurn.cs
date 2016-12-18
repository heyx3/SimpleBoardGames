using System;
using UnityEngine;


namespace BoardGames.UnityLogic
{
	/// <summary>
	/// Activates this script's GameObject when a specific player starts their turn,
	///     and deactivates it when that player ends their turn.
	/// </summary>
	public class ActivateOnTurn<LocationType> : MonoBehaviour
		where LocationType : IEquatable<LocationType>
	{
		public BoardGames.Players Player;

		private void Start()
		{
			var gameMode = GameMode.GameMode<LocationType>.Instance;

			gameMode.CurrentTurn.OnChanged +=
				(_gameMode, oldTurn, newTurn) =>
				{
					gameObject.SetActive(newTurn == Player);
				};
			gameObject.SetActive(gameMode.CurrentTurn.Value == Player);
		}
	}
}