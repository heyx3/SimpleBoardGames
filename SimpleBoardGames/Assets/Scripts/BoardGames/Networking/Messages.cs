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

		MakeMove,
		MoveSuccessful,
	}

	public static class Extensions
	{
		public static void Write(this BinaryWriter writer, IPEndPoint endPoint)
		{
			byte[] bytes = endPoint.Address.GetAddressBytes();
			writer.Write((Int32)bytes.Length);
			writer.Write(bytes);

			writer.Write((Int32)endPoint.Port);
		}
		public static IPEndPoint ReadIP(this BinaryReader reader)
		{
			int nBytes = reader.ReadInt32();
			byte[] ipBytes = reader.ReadBytes(nBytes);
			int port = reader.ReadInt32();
			return new IPEndPoint(new IPAddress(ipBytes), port);
		}
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
				case Types.GameState: b = new GameState(null, null, TurnStates.YourTurn); break;

				case Types.MakeMove: b = new MakeMove(0, 0, null, null, TurnStates.OtherTurn); break;
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

	public class GameState : Base
	{
		public byte[] BoardState, LastAction;
		public TurnStates TurnState;

		public GameState(byte[] boardState, byte[] lastAction, TurnStates turnState)
			: base(Types.GameState)
		{
			BoardState = boardState;
			LastAction = lastAction;
			TurnState = turnState;
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write((Int32)BoardState.Length);
			writer.Write(BoardState);

			writer.Write((Int32)LastAction.Length);
			writer.Write(LastAction);

			writer.Write((byte)TurnState);
		}
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);
			BoardState = reader.ReadBytes(reader.ReadInt32());
			LastAction = reader.ReadBytes(reader.ReadInt32());
			TurnState = (TurnStates)reader.ReadByte();
		}
	}

	#endregion

	#region Make Move

	public class MakeMove : Base
	{
		public ulong SessionID;
		public byte PlayerID;
		public byte[] TheMove, TheBoardAfterMove;
		public TurnStates NewState;

		public MakeMove(ulong sessionID, byte playerID, byte[] theMove, byte[] theBoardAfterMove,
						TurnStates newState)
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
			NewState = (TurnStates)reader.ReadByte();
		}
	}

	public class MoveSuccessful : Base
	{
		public MoveSuccessful() : base(Types.MoveSuccessful) { }
	}

	#endregion
}