using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FiaCR.UnityLogic
{
	[RequireComponent(typeof(TextMesh))]
	public class FCR_DisplayMovesLeft : MonoBehaviour
	{
		private TextMesh textMesh;

		private void Awake()
		{
			textMesh = GetComponent<TextMesh>();
		}
		private void Start()
		{
			var gameMode = (GameMode.FCR_Game_Offline)GameMode.FCR_Game_Offline.Instance;
			gameMode.MovesLeft.OnChanged += Callback_MovesLeftChanged;
			Callback_MovesLeftChanged(gameMode, 0, gameMode.MovesLeft);
		}

		private void Callback_MovesLeftChanged(GameMode.FCR_Game_Offline owner, uint oldVal, uint newVal)
		{
			textMesh.text = newVal.ToString();
		}
	}
}
