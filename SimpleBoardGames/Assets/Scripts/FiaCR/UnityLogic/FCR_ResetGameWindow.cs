using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FiaCR.UnityLogic
{
	public class FCR_ResetGameWindow : MonoBehaviour
	{
		public void Callback_OnConfirm()
		{
			var gameMode = (GameMode.FCR_Game_Offline)GameMode.FCR_Game_Offline.Instance;
			gameMode.ClearGame();
		}
		public void Callback_OnCancel()
		{
			gameObject.SetActive(false);
		}
	}
}
