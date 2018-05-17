using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace BoardGames.UnityLogic.GameMode
{
	public struct ClientMatchmakingData
	{
		public System.Net.IPEndPoint Server;

		public ulong PlayerID;

		//The opponent data is set to "null" if an opponent hasn't been found yet.
		public string OpponentName;
		public ulong? OpponentID;


		public ClientMatchmakingData(System.Net.IPEndPoint server, ulong playerID,
									 string opponentName = null, ulong? opponentID = null)
		{
			Server = server;
			PlayerID = playerID;

			OpponentName = opponentName;
			OpponentID = opponentID;
		}


		public void Serialize(BinaryWriter writer)
		{
			writer.Write(Server);
			writer.Write(PlayerID);

			writer.Write(OpponentName != null);
			if (OpponentName != null)
			{
				writer.Write(OpponentName);
				writer.Write(OpponentID.Value);
			}
		}
		public void Deserialize(BinaryReader reader)
		{
			Server = reader.ReadIP();
			PlayerID = reader.ReadUInt64();

			if (reader.ReadBoolean())
			{
				OpponentName = reader.ReadString();
				OpponentID = reader.ReadUInt64();
			}
			else
			{
				OpponentName = null;
				OpponentID = null;
			}
		}
	}
}
