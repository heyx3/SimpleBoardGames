using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

using BoardGames.Networking.Messages;

namespace BoardGames.Networking
{
	public class Server : Singleton<Server>
	{
		public UnityEngine.UI.InputField UI_Port, UI_MaxMessages;
		public Transform UI_MessagesContentParent;
		public GameObject UI_MessagePrefab;

		public List<BoardGames.Networking.GameState> ActiveGames =
			new List<BoardGames.Networking.GameState>();

		public int Port = 50111;
		public int PlayerTimeout = 20;

		public int MaxMessages = 100;
		public bool LogMessagesInUnity = true;

		private Logger logger;
		private TcpListener networkListener = null;
		private PlayerMatcher playerMatcher;
		private ulong nextPlayerID = 1;


		/// <summary>
		/// Attempts an action. If an exception gets thrown,
		///     an error will be added to "messages".
		/// Returns whether the action succeeded without exceptions.
		/// </summary>
		private bool Try(Action action, string description,
					     System.Action<Exception> onException = null)
		{
			try
			{
				action();
				return true;
			}
			catch (Exception e)
			{
				if (onException != null)
					onException(e);

				logger.Add(Message.Error("Error " + description + ": (" +
										     e.GetType() + ") " + e.Message));
				return false;
			}
		}
		/// <summary>
		/// Restarts the network listener.
		/// Is usually called when the Port changes.
		/// NOTE: if the listener is "null", a new one will NOT be started!
		/// </summary>
		private void RestartListener()
		{
			if (networkListener != null)
			{
				networkListener.Stop();
				networkListener = new TcpListener(IPAddress.Any, Port);
				networkListener.Start();

				logger.Add(Message.Warning("Restarting network listener with new port " + Port));
			}
		}

		private void Start()
		{
			Screen.orientation = ScreenOrientation.Landscape;

			//Set up Port editor UI.
			UI_Port.text = Port.ToString();
			UI_Port.onEndEdit.AddListener(newVal =>
			{
				int newPort;
				if (int.TryParse(newVal, out newPort) && newPort != Port &&
					newPort >= 0 && newPort <= ushort.MaxValue)
				{
					Port = newPort;
					RestartListener();
				}
			});

			//Set up MaxMessages editor UI.
			UI_MaxMessages.text = MaxMessages.ToString();
			UI_MaxMessages.onEndEdit.AddListener(newVal =>
			{
				int newMax;
				if (int.TryParse(newVal, out newMax) && newMax != MaxMessages && newMax > 0)
				{
					MaxMessages = newMax;
					logger.MaxMessages = newMax;
				}
			});

			//Set up message UI.
			logger = new Logger(MaxMessages, LogMessagesInUnity);
			logger.OnNewMessage += msg =>
			{
				var obj = Instantiate(UI_MessagePrefab);
				var uiText = obj.GetComponent<UnityEngine.UI.Text>();
				uiText.text = msg.Text;
				uiText.color = msg.Color;
				obj.transform.SetParent(UI_MessagesContentParent, false);
			};
			logger.OnRemoveMessages += nMsgs =>
			{
				for (int i = 0; i < nMsgs; ++i)
					Destroy(UI_MessagesContentParent.GetChild(0).gameObject);
			};

			playerMatcher = new PlayerMatcher();

			StartCoroutine(Coroutine_ServerLoop());
		}
		private System.Collections.IEnumerator Coroutine_ServerLoop()
		{
			logger.Add(new Message("Starting server..."));
			yield return null;

			//Start the listener.
			//Note that other methods may change/reallocate it while this coroutine is resting.
			if (networkListener != null)
				networkListener.Stop();
			networkListener = new TcpListener(IPAddress.Any, Port);
			networkListener.Start();

			//Wait for new connections to come in.
			logger.Add(new Message("Waiting for connections"));
			yield return null;
			while (true)
			{
				while (!networkListener.Pending())
					yield return null;

				Socket socket = null;
				if (!Try(() => socket = networkListener.AcceptSocket(), "accepting socket",
						 e => { networkListener.Stop(); networkListener = null; }))
					yield break;

				logger.Add(new Message("Found a connection: " + socket.RemoteEndPoint));
				yield return null;

				//Spawn a new thread for the socket.
				var thread = new Thread(SocketHandler);
				thread.Start(socket);
			}
		}

		/// <summary>
		/// Runs the logic for a new connection.
		/// This method can be run on its own separate thread.
		/// </summary>
		/// <param name="_socket">
		/// The Socket object. Passed as a generic object to conform to the Thread interface.
		/// This method will dispose of it when it's done.
		/// </param>
		private void SocketHandler(object _socket)
		{
			using (var socket = (Socket)_socket)
			using (var stream = new NetworkStream(socket))
			using (var streamReader = new BinaryReader(stream))
			using (var streamWriter = new BinaryWriter(stream))
			{
				//Wait for a message from the client.
				var msg = Messages.Base.Read(streamReader);

				//Only three types of messages are expected initially.
				switch (msg.Type)
				{
					case Messages.Types.FindGame:
						SocketHandler_FindGame((Messages.FindGame)msg, stream,
											   streamReader, streamWriter);
						break;
					case Messages.Types.CheckOpponentFound:
						SocketHandler_CheckOpponentFound((Messages.CheckOpponentFound)msg, stream,
														 streamReader, streamWriter);
						break;
					case Messages.Types.GetGameState:
						SocketHandler_GetGameState((Messages.GetGameState)msg, stream,
												   streamReader, streamWriter);
						break;
					case Messages.Types.MakeMove:
						SocketHandler_MakeMove((Messages.MakeMove)msg, stream,
											   streamReader, streamWriter);
						break;
					case Messages.Types.ForfeitGame:
						SocketHandler_ForfeitGame((Messages.ForfeitGame)msg, stream,
												  streamReader, streamWriter);
						break;

					default:
						Messages.Base.Write(new Messages.Error("Expected message types: FindGame, CheckOpponentFound, GetGameState, MakeMove, or ForfeitGame; got " +
															     msg.Type),
											streamWriter);
						break;
				}
			}
		}
		private void SocketHandler_FindGame(Messages.FindGame msg, NetworkStream stream,
											BinaryReader streamReader, BinaryWriter streamWriter)
		{
			ulong playerID = nextPlayerID;

			nextPlayerID = unchecked(nextPlayerID + 1);

			var successMsg = new Messages.SuccessfullyInQueue(playerID);
			if (Try(() => Messages.Base.Write(successMsg, streamWriter), "Sending 'success' msg"))
				playerMatcher.Push(new PlayerMatcher.Player(msg.ClientName, playerID, msg.GameID));
		}
		private void SocketHandler_CheckOpponentFound(Messages.CheckOpponentFound msg,
													  NetworkStream stream,
													  BinaryReader streamReader,
													  BinaryWriter streamWriter)
		{
			//TODO: Implement.
		}
		private void SocketHandler_GetGameState(Messages.GetGameState msg, NetworkStream stream,
												BinaryReader streamReader, BinaryWriter streamWriter)
		{
			//TODO: Implement.
		}
		private void SocketHandler_MakeMove(Messages.MakeMove msg, NetworkStream stream,
											BinaryReader streamReader, BinaryWriter streamWriter)
		{
			//TODO: Implement.
		}
		private void SocketHandler_ForfeitGame(Messages.ForfeitGame msg, NetworkStream stream,
											   BinaryReader streamReader, BinaryWriter streamWriter)
		{
			//TODO: Implement.
		}

		/// <summary>
		/// Saves the state of this server to the given stream.
		/// </summary>
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(Port);
			writer.Write(PlayerTimeout);

			writer.Write(MaxMessages);

			writer.Write(nextPlayerID);

			playerMatcher.Foreach(count => writer.Write(count),
								  (player, opponent) =>
								  {
									  writer.Write(player.Name);
									  writer.Write(player.PlayerID);
									  writer.Write(player.GameID);
									  writer.Write(opponent.HasValue);
									  if (opponent.HasValue)
									  {
										  writer.Write(opponent.Value.Name);
										  writer.Write(opponent.Value.PlayerID);
										  writer.Write(opponent.Value.GameID);
									  }
								  });

			writer.Write(ActiveGames.Count);
			foreach (var game in ActiveGames)
				game.Serialize(writer);
		}
		public void Deserialize(BinaryReader reader)
		{
			Port = reader.ReadInt32();
			PlayerTimeout = reader.ReadInt32();

			MaxMessages = reader.ReadInt32();

			nextPlayerID = reader.ReadUInt64();

			playerMatcher.Clear();
			int queueSize = reader.ReadInt32();
			for (int i = 0; i < queueSize; ++i)
			{
				var player = new PlayerMatcher.Player(reader.ReadString(),
													  reader.ReadUInt64(),
													  reader.ReadUInt64());
				if (reader.ReadBoolean())
				{
					playerMatcher.Push(player, new PlayerMatcher.Player(reader.ReadString(),
																		reader.ReadUInt64(),
																		reader.ReadUInt64()));
				}
				else
				{
					playerMatcher.Push(player);
				}
			}

			ActiveGames.Clear();
			int nGames = reader.ReadInt32();
			for (int i = 0; i < nGames; ++i)
			{
				var game = new Networking.GameState();
				game.Deserialize(reader);
				ActiveGames.Add(game);
			}

			//The port may have just changed, so restart the listener just in case.
			RestartListener();
		}
	}
}
