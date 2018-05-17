using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace BoardGames.UnityLogic.GameMode
{
	public class GameClientUI : Singleton<GameClientUI>
	{
		[SerializeField]
		private UnityEngine.UI.Text errorText;

		[SerializeField]
		private GameObject scene_NewMatch, scene_FindingMatch;


		public string MyName = "Billy";
		public string ServerIP = "127.0.0.1";
		public int ServerPort = 50111;
		public int Timeout = 5;

		private string nextErrorText = null;
		private object errorTextLocker = new object();


		public void SetErrorMsg(string msg)
		{
			lock (errorTextLocker)
			{
				nextErrorText = msg;
			}
		}
		public void SetIsFindingMatch(bool isFinding)
		{
			scene_NewMatch.SetActive(!isFinding);
			scene_FindingMatch.SetActive(isFinding);
		}


		public void Callback_ServerIPChanged(string newIP)
		{
			System.Net.IPAddress tryVal;
			if (System.Net.IPAddress.TryParse(newIP, out tryVal))
				ServerIP = newIP;
		}
		public void Callback_PortChanged(string newPort)
		{
			int tryVal;
			if (int.TryParse(newPort, out tryVal) && tryVal > 0)
				ServerPort = tryVal;
		}
		public void Callback_TimeoutChanged(string newTimeout)
		{
			int tryVal;
			if (int.TryParse(newTimeout, out tryVal) && tryVal > 0)
				Timeout = tryVal;
		}
		public void Callback_NameChanged(string newName) { MyName = newName; }


		private void Update()
		{
			lock (errorTextLocker)
			{
				if (nextErrorText != null)
				{
					errorText.text = nextErrorText;
					nextErrorText = null;
				}
			}
		}
	}
}
