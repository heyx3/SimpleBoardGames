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
		public int LastKnownMove;


		public ClientMatchmakingData(System.Net.IPEndPoint server, ulong playerID,
									 string opponentName = null, ulong? opponentID = null,
									 int lastKnownMove = 0)
		{
			Server = server;
			PlayerID = playerID;

			OpponentName = opponentName;
			OpponentID = opponentID;
			LastKnownMove = lastKnownMove;
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

				writer.Write(LastKnownMove);
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
				LastKnownMove = reader.ReadInt32();
			}
			else
			{
				OpponentName = null;
				OpponentID = null;
				LastKnownMove = 0;
			}
		}
	}
}
