using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;


namespace BoardGames.Networking.Messages
{
	public enum MatchStates : byte
	{
		Player1Turn,
		Player2Turn,

		Player1Won,
		Player2Won,
		Tie,
	}
	public static class Extensions
	{
		public static bool IsGameOver(this MatchStates state)
		{
			switch (state)
			{
				case MatchStates.Player1Turn:
				case MatchStates.Player2Turn:
					return false;

				case MatchStates.Player1Won:
				case MatchStates.Player2Won:
				case MatchStates.Tie:
					return true;

				default: throw new NotImplementedException(state.ToString());
			}
		}
	}


	public enum Types : byte
	{
		Error,
		Acknowledge,

		FindGame,
		SuccessfullyInQueue,

		CheckOpponentFound,
		FoundOpponent,
		NewBoard,

		GetGameState,
		GameState,

		MakeMove,

		ForfeitGame,
	}

	public abstract class Base
	{
		public Types Type { get; private set; }

		public Base(Types t) { Type = t; }

		public virtual void Serialize(BinaryWriter writer) { }
		public virtual void Deserialize(BinaryReader reader) { }

		public static void Write(Base msg, BinaryWriter writer)
		{
			writer.Write((byte)msg.Type);
			msg.Serialize(writer);
		}
		public static Base Read(BinaryReader reader)
		{
			Base b = null;

			var type = (Types)reader.ReadByte();
			switch (type)
			{
				case Types.Error: b = new Error(null); break;
				case Types.Acknowledge: b = new Acknowledge(); break;

				case Types.FindGame: b = new FindGame(null, 0); break;
				case Types.SuccessfullyInQueue: b = new SuccessfullyInQueue(0); break;

				case Types.CheckOpponentFound: b = new CheckOpponentFound(0); break;
				case Types.FoundOpponent: b = new FoundOpponent(null, 0, false); break;
				case Types.NewBoard: b = new NewBoard(null, MatchStates.Tie); break;

				case Types.GetGameState: b = new GetGameState(0, 0); break;
				case Types.GameState: b = new GameState(null, null, MatchStates.Tie, 0, 0); break;

				case Types.MakeMove: b = new MakeMove(0, null, null, MatchStates.Tie); break;

				case Types.ForfeitGame: b = new ForfeitGame(0); break;

				default: throw new NotImplementedException(type.ToString());
			}

			b.Deserialize(reader);

			return b;
		}

		#region Helper Functions
		protected static void WriteBytes(byte[] bytes, BinaryWriter writer)
		{
			writer.Write(bytes.Length);
			foreach (byte b in bytes)
				writer.Write(b);
		}
		protected static byte[] ReadBytes(BinaryReader reader)
		{
			byte[] bytes = new byte[reader.ReadInt32()];
			for (int i = 0; i < bytes.Length; ++i)
				bytes[i] = reader.ReadByte();
			return bytes;
		}
		#endregion
	}

	public class Error : Base
	{
		public string Msg;

		public Error(string msg) : base(Types.Error) { Msg = msg; }

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Msg);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			Msg = reader.ReadString();
		}
	}
	public class Acknowledge : Base
	{
		public Acknowledge() : base(Types.Acknowledge) { }
	}

	#region Find Game

	public class FindGame : Base
	{
		public string ClientName;
		public ulong GameID;

		public FindGame(string clientName, ulong gameID)
			: base(Types.FindGame)
		{
			ClientName = clientName;
			GameID = gameID;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ClientName);
			writer.Write(GameID);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			ClientName = reader.ReadString();
			GameID = reader.ReadUInt64();
		}
	}
	public class SuccessfullyInQueue : Base
	{
		public ulong PlayerID;

		public SuccessfullyInQueue(ulong playerID)
			: base(Types.SuccessfullyInQueue)
		{
			PlayerID = playerID;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerID);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			PlayerID = reader.ReadUInt64();
		}
	}

	#endregion

	#region Check Opponent Found

	public class CheckOpponentFound : Base
	{
		public ulong PlayerID;

		public CheckOpponentFound(ulong playerID)
			: base(Types.SuccessfullyInQueue)
		{
			PlayerID = playerID;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerID);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			PlayerID = reader.ReadUInt64();
		}
	}
	public class FoundOpponent : Base
	{
		public string OpponentName;
		public ulong OpponentID;
		public bool AmIGoingFirst;

		public FoundOpponent(string opponentName, ulong opponentID, bool amIGoingFirst)
			: base(Types.FoundOpponent)
		{
			OpponentName = opponentName;
			OpponentID = opponentID;
			AmIGoingFirst = amIGoingFirst;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(OpponentName);
			writer.Write(OpponentID);
			writer.Write(AmIGoingFirst);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			OpponentName = reader.ReadString();
			OpponentID = reader.ReadUInt64();
			AmIGoingFirst = reader.ReadBoolean();
		}
	}
	public class NewBoard : Base
	{
		public byte[] BoardState;
		public MatchStates MatchState;

		public NewBoard(byte[] boardState, MatchStates matchState)
			: base(Types.NewBoard)
		{
			BoardState = boardState;
			MatchState = matchState;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			WriteBytes(BoardState, writer);
			writer.Write((byte)MatchState);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			BoardState = ReadBytes(reader);
			MatchState = (MatchStates)reader.ReadByte();
		}
	}

	#endregion

	#region Get Game State

	public class GetGameState : Base
	{
		/// <summary>
		/// The ID of the player making the request.
		/// </summary>
		public ulong PlayerID;
		/// <summary>
		/// // A 0 represents "no moves have been processed yet".
		/// </summary>
		public int LastKnownMovement;

		public GetGameState(ulong playerID, int lastKnownMovement)
			: base(Types.GetGameState)
		{
			PlayerID = playerID;
			LastKnownMovement = lastKnownMovement;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerID);
			writer.Write(LastKnownMovement);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			PlayerID = reader.ReadUInt64();
			LastKnownMovement = reader.ReadInt32();
		}
	}
	public class GameState : Base
	{
		public byte[] BoardState;
		public byte[][] RecentMoves;
		public MatchStates MatchState;
		public ulong Player1ID, Player2ID;

		public GameState(byte[] boardState, byte[][] recentMoves, MatchStates matchState,
						 ulong player1ID, ulong player2ID)
			: base(Types.GameState)
		{
			BoardState = boardState;
			RecentMoves = recentMoves;
			MatchState = matchState;
			Player1ID = player1ID;
			Player2ID = player2ID;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			WriteBytes(BoardState, writer);

			writer.Write(RecentMoves.Length);
			foreach (byte[] move in RecentMoves)
				WriteBytes(move, writer);

			writer.Write((byte)MatchState);

			writer.Write(Player1ID);
			writer.Write(Player2ID);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);

			BoardState = ReadBytes(reader);

			RecentMoves = new byte[reader.ReadInt32()][];
			for (int i = 0; i < RecentMoves.Length; ++i)
				RecentMoves[i] = ReadBytes(reader);

			MatchState = (MatchStates)reader.ReadByte();

			Player1ID = reader.ReadUInt64();
			Player2ID = reader.ReadUInt64();
		}
	}

	#endregion

	#region Make Move

	public class MakeMove : Base
	{
		public ulong PlayerID;
		public byte[] Move;
		public byte[] NewBoardState;
		public MatchStates NewMatchState;

		public MakeMove(ulong playerID, byte[] move, byte[] newBoardState, MatchStates newMatchState)
			: base(Types.MakeMove)
		{
			PlayerID = playerID;
			Move = move;
			NewBoardState = newBoardState;
			NewMatchState = newMatchState;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerID);
			WriteBytes(Move, writer);
			WriteBytes(NewBoardState, writer);
			writer.Write((byte)NewMatchState);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			PlayerID = reader.ReadUInt64();
			Move = ReadBytes(reader);
			NewBoardState = ReadBytes(reader);
			NewMatchState = (MatchStates)reader.ReadByte();
		}
	}

	#endregion

	#region Forfeit Game

	public class ForfeitGame : Base
	{
		public ulong PlayerID;

		public ForfeitGame(ulong playerID) : base(Types.ForfeitGame) { PlayerID = playerID; }

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerID);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			PlayerID = reader.ReadUInt64();
		}
	}

	#endregion
}