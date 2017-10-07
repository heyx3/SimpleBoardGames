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
		}
	}
}
