using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BoardGames.UnityLogic
{
	public abstract class ActivateOnWin<LocationType> : MonoBehaviour
		where LocationType : IEquatable<LocationType>
	{
		protected virtual void Start()
		{
			var gameMode = GameMode.GameMode<LocationType>.Instance;

			if (gameMode is GameMode.GameMode_Offline<LocationType>)
			{
				((GameMode.GameMode_Offline<LocationType>)gameMode).OnPlayerWin +=
					(board, player) =>
					{
						gameObject.SetActive(ShouldActivate(player));
					};
			}

			gameObject.SetActive(false);
		}

		protected abstract bool ShouldActivate(Players? player);
	}
}
