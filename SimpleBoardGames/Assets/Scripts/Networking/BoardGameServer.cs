using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using UnityEngine;


namespace Networking
{
	public class BoardGameServer : Singleton<BoardGameServer>
	{
		public uint NextID = 0;


		[Serializable]
		public class Player
		{
			public string Name;
		}

		[Serializable]
		public class Connection
		{
			public uint Player1, Player2;

			public bool ContainsPlayer(uint playerID)
			{
				return playerID == Player1 || playerID == Player2;
			}
			public uint GetOtherPlayer(uint playerID)
			{
				return (playerID == Player1) ? Player2 : Player1;
			}
		}


		public int SelectedIP = 0;
		public int Port = 52110;
		
		public UnityEngine.UI.Dropdown UI_IPDropdown;
		public UnityEngine.UI.Text UI_IPText;
		public UnityEngine.UI.Button UI_RefreshButton;
		public UnityEngine.UI.Button UI_BackButton;

		private IPAddress[] myIPs = null;

		private string myDir = Path.Combine(Application.dataPath, "ServerData");



		protected override void Awake()
		{
			base.Awake();
			RefreshIPs();

			UI_IPDropdown.onValueChanged.AddListener((i) => SelectedIP = i);
			UI_RefreshButton.onClick.AddListener(() => RefreshIPs());
			UI_BackButton.onClick.AddListener(() =>
				UnityEngine.SceneManagement.SceneManager.LoadScene("GlobalMenu"));
		}

		private void RefreshIPs()
		{
			//Remember the currently-selected IP.
			string current = (SelectedIP < UI_IPDropdown.options.Count) ?
								     UI_IPDropdown.options[SelectedIP].text :
									 null;

			//Get the new IPs.
			var hosts = Dns.GetHostEntry(Dns.GetHostName());
			myIPs = hosts.AddressList;
			UI_IPDropdown.options = hosts.Aliases
										.Select((al) =>
											new UnityEngine.UI.Dropdown.OptionData(al)).ToList();

			//Try to find the selected IP in the new list.
			SelectedIP = IndexOf(hosts.Aliases, current);
			if (SelectedIP < 0)
				SelectedIP = 0;

			//Update the IP label.
			if (SelectedIP < myIPs.Length)
				UI_IPText.text = myIPs[SelectedIP].ToString();
			else
				UI_IPText.text = "[no IP selected]";
		}


		private static int IndexOf<T>(IList<T> ts, T toFind)
			where T : IEquatable<T>
		{
			for (int i = 0; i < ts.Count; ++i)
				if (toFind.Equals(ts[i]))
					return i;
			return -1;
		}
	}
}