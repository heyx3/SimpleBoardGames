using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace BoardGames.Networking
{
	public class GameState
	{
		public byte[] BoardState;
		public List<byte[]> AllMoves;
		public Messages.MatchStates MatchState;
		public ulong Player1ID, Player2ID;


		public void Serialize(BinaryWriter writer)
		{
			writer.Write(BoardState.Length);
			foreach (byte b in BoardState)
				writer.Write(b);

			writer.Write(AllMoves.Count);
			foreach (var move in AllMoves)
			{
				writer.Write(move.Length);
				foreach (byte b in move)
					writer.Write(b);
			}

			writer.Write((byte)MatchState);

			writer.Write(Player1ID);
			writer.Write(Player2ID);
		}
		public void Deserialize(BinaryReader reader)
		{
			BoardState = new byte[reader.ReadInt32()];
			for (int i = 0; i < BoardState.Length; ++i)
				BoardState[i] = reader.ReadByte();

			AllMoves.Clear();
			int nMoves = reader.ReadInt32();
			for (int i = 0; i < nMoves; ++i)
			{
				AllMoves.Add(new byte[reader.ReadInt32()]);
				for (int j = 0; j < AllMoves[i].Length; ++j)
					AllMoves[i][j] = reader.ReadByte();
			}

			MatchState = (Messages.MatchStates)reader.ReadByte();

			Player1ID = reader.ReadUInt64();
			Player2ID = reader.ReadUInt64();
		}


		public bool DidYouWin(ulong playerID)
		{
			return (playerID == Player1ID && MatchState == Messages.MatchStates.Player1Won) ||
				   (playerID == Player2ID && MatchState == Messages.MatchStates.Player2Won);
		}
		public bool DidYouLose(ulong playerID)
		{
			return (playerID == Player1ID && MatchState == Messages.MatchStates.Player2Won) ||
				   (playerID == Player2ID && MatchState == Messages.MatchStates.Player1Won);
		}
		public bool IsYourTurn(ulong playerID)
		{
			return (playerID == Player1ID && MatchState == Messages.MatchStates.Player1Turn) ||
				   (playerID == Player2ID && MatchState == Messages.MatchStates.Player2Turn);
		}

		public ulong GetOtherPlayer(ulong playerID)
		{
			UnityEngine.Assertions.Assert.IsTrue(playerID == Player1ID |
													 playerID == Player2ID,
												 "Unexpected player ID " + playerID);
			return playerID == Player1ID ?
					   Player2ID :
					   Player1ID;
		}
	}
}
