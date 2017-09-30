using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;


namespace BoardGames.Networking.Messages
{
	public enum Types
	{
		Error,

		Handshake,
		HeartbeatClient,
		HeartbeatServer,
		FoundOpponent,
		NewBoard,

		GetGameState,
		GameState,
		AcknowledgeGameEnd,

		MakeMove,
		MoveSuccessful,
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

				case Types.Handshake: b = new Handshake(null, null, 0); break;
				case Types.HeartbeatClient: b = new HeartbeatFromClient(null); break;
				case Types.HeartbeatServer: b = new HeartbeatFromServer(); break;
				case Types.FoundOpponent: b = new FoundOpponent(null, 0, 0, false); break;
				case Types.NewBoard: b = new NewBoard(0, null); break;

				case Types.GetGameState: b = new GetGameState(0, 0); break;
				case Types.GameState: b = new GameState(0, null, null, MatchStates.Turn1); break;
				case Types.AcknowledgeGameEnd: b = new AcknowledgeGameEnd(0); break;

				case Types.MakeMove: b = new MakeMove(0, 0, null, null, MatchStates.Turn1); break;
				case Types.MoveSuccessful: b = new MoveSuccessful(); break;

				default: throw new NotImplementedException(type.ToString());
			}

			b.Deserialize(reader);

			return b;
		}
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

	#region New Client

	public class Handshake : Base
	{
		public IPEndPoint ClientAddr;
		public string ClientName;
		public ulong GameID;

		public Handshake(IPEndPoint clientAddr, string clientName, ulong gameID)
			: base(Types.Handshake)
		{
			ClientAddr = clientAddr;
			ClientName = clientName;
			GameID = gameID;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write(ClientAddr);
			writer.Write(ClientName);
			writer.Write((UInt64)GameID);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);

			ClientAddr = reader.ReadIP();
			ClientName = reader.ReadString();
			GameID = reader.ReadUInt64();
		}
	}

	public class HeartbeatFromClient : Base
	{
		public IPEndPoint ClientAddr;

		public HeartbeatFromClient(IPEndPoint clientAddr)
			: base(Types.HeartbeatClient)
		{
			ClientAddr = clientAddr;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ClientAddr);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			ClientAddr = reader.ReadIP();
		}
	}
	public class HeartbeatFromServer : Base
	{
		public HeartbeatFromServer() : base(Types.HeartbeatServer) { }
	}

	public class FoundOpponent : Base
	{
		public string OpponentName;
		public ulong SessionID;
		public byte PlayerID;
		public bool AmIGoingFirst;

		public FoundOpponent(string opponentName, ulong sessionID, byte playerID, bool amGoingFirst)
			: base(Types.FoundOpponent)
		{
			OpponentName = opponentName;
			SessionID = sessionID;
			PlayerID = playerID;
			AmIGoingFirst = amGoingFirst;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(OpponentName);
			writer.Write((UInt64)SessionID);
			writer.Write((byte)PlayerID);
			writer.Write(AmIGoingFirst);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			OpponentName = reader.ReadString();
			SessionID = reader.ReadUInt64();
			PlayerID = reader.ReadByte();
			AmIGoingFirst = reader.ReadBoolean();
		}
	}

	public class NewBoard : Base
	{
		public ulong SessionID;
		public byte[] BoardState;

		public NewBoard(ulong sessionID, byte[] boardState)
			: base(Types.NewBoard)
		{
			SessionID = sessionID;
			BoardState = boardState;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write((UInt64)SessionID);
			writer.Write((Int32)BoardState.Length);
			writer.Write(BoardState);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			SessionID = reader.ReadUInt64();
			BoardState = reader.ReadBytes(reader.ReadInt32());
		}
	}

	#endregion

	#region Get Game State

	public class GetGameState : Base
	{
		public ulong SessionID;
		public byte PlayerID;

		public GetGameState(ulong sessionID, byte playerID)
			: base(Types.GetGameState)
		{
			SessionID = sessionID;
			PlayerID = playerID;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write((UInt64)SessionID);
			writer.Write((byte)PlayerID);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			SessionID = reader.ReadUInt64();
			PlayerID = reader.ReadByte();
		}
	}


	/// <summary>
	/// The different states of the match.
	/// Either it's somebody's turn, or the game is over.
	/// </summary>
	public enum MatchStates : byte
	{
		Turn1, Turn2,
		Winner1, Winner2, Tie,
	}

	[Serializable]
	public class GameState : Base
	{
		public ulong SessionID { get; private set; }
		public byte[] BoardState { get; private set; }
		public byte[] LastAction { get; private set; }
		public MatchStates MatchState { get; private set; }

		public GameState(ulong sessionID, byte[] boardState, byte[] lastAction, MatchStates matchState)
			: base(Types.GameState)
		{
			SessionID = sessionID;

			BoardState = boardState;
			LastAction = lastAction;

			MatchState = matchState;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write(SessionID);

			writer.Write(BoardState.Length);
			writer.Write(BoardState);
			writer.Write(LastAction.Length);
			writer.Write(LastAction);

			writer.Write((byte)MatchState);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);

			SessionID = reader.ReadUInt64();

			BoardState = reader.ReadBytes(reader.ReadInt32());
			LastAction = reader.ReadBytes(reader.ReadInt32());

			MatchState = (MatchStates)reader.ReadByte();
		}
	}

	public class AcknowledgeGameEnd : Base
	{
		public ulong SessionID { get; private set; }

		public AcknowledgeGameEnd(ulong sessionID) : base(Types.AcknowledgeGameEnd) { SessionID = sessionID; }

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SessionID);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			SessionID = reader.ReadUInt64();
		}
	}

	#endregion

	#region Make Move

	public class MakeMove : Base
	{
		public ulong SessionID;
		public byte PlayerID;
		public byte[] TheMove, TheBoardAfterMove;
		public MatchStates NewState;

		public MakeMove(ulong sessionID, byte playerID, byte[] theMove, byte[] theBoardAfterMove,
						MatchStates newState)
			: base(Types.MakeMove)
		{
			SessionID = sessionID;
			PlayerID = playerID;
			TheMove = theMove;
			TheBoardAfterMove = theBoardAfterMove;
			NewState = newState;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write((UInt64)SessionID);
			writer.Write((byte)PlayerID);

			writer.Write((Int32)TheMove.Length);
			writer.Write(TheMove);

			writer.Write((Int32)TheBoardAfterMove.Length);
			writer.Write(TheBoardAfterMove);

			writer.Write((byte)NewState);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			SessionID = reader.ReadUInt64();
			PlayerID = reader.ReadByte();
			TheMove = reader.ReadBytes(reader.ReadInt32());
			TheBoardAfterMove = reader.ReadBytes(reader.ReadInt32());
			NewState = (MatchStates)reader.ReadByte();
		}
	}

	public class MoveSuccessful : Base
	{
		public MoveSuccessful() : base(Types.MoveSuccessful) { }
	}

	#endregion
}