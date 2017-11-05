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

		public int Port = 50111;
		public int PlayerTimeout = 20;

		public int MaxMessages = 100;
		public bool LogMessagesInUnity = true;


		private List<BoardGames.Networking.GameState> activeGames =
			new List<BoardGames.Networking.GameState>();
		private object lock_activeGames = new object();
		private BoardGames.Networking.GameState FindGame(ulong playerID)
		{
			lock (lock_activeGames)
			{
				foreach (var game in activeGames)
					if (game.Player1ID == playerID || game.Player2ID == playerID)
						return game;
			}
			return null;
		}

		/// <summary>
		/// Contains every finished game,
		///     indexed by the PlayerID of the player that hasn't been notified of it yet.
		/// </summary>
		private Dictionary<ulong, GameState> finishedGamesByUnacknowledgedPlayer =
			new Dictionary<ulong, GameState>();
		private object lock_finishedGames = new object();

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
			//Get the player with the given ID.
			var playerData = playerMatcher.TryGetKey(msg.PlayerID);
			if (!playerData.HasValue)
			{
				var failMsg = new Messages.Error("Player ID " + msg.PlayerID +
												     " not found in matchmaking queue");
				Try(() => Messages.Base.Write(failMsg, streamWriter),
					"Sending error msg");

				return;
			}

			//See if he has a match yet.
			bool wasFirst;
			var tryMatch = playerMatcher.TryPop(playerData.Value, out wasFirst);
			if (tryMatch.HasValue)
			{
				var match = tryMatch.Value;

				//If error occurs, we should leave the player in the queue.
				System.Action<Exception> onFailure = e => playerMatcher.Push(playerData.Value, match);

				//Tell the player he has an opponent.
				var foundPlayerMsg = new Messages.FoundOpponent(match.Name, match.PlayerID, wasFirst);
				if (!Try(() => Messages.Base.Write(foundPlayerMsg, streamWriter),
						 "Sending opponent msg", onFailure))
				{
					return;
				}

				//If the player who asked is going first, get the board state from him.
				if (wasFirst)
				{
					//Get the initial board state from the player.
					Messages.Base _newBoardMsg = null;
					if (!Try(() => Messages.Base.Read(streamReader), "Getting board state", onFailure))
						return;

					var newBoardMsg = _newBoardMsg as Messages.NewBoard;
					if (newBoardMsg == null)
					{
						onFailure(null);

						string errMsg = "Unexpected message type " + _newBoardMsg.Type + "; expected NewBoard";
						Try(() => Messages.Base.Write(new Messages.Error(errMsg), streamWriter),
							"Sending error msg from 'init board state'");

						return;
					}

					//Record the initial game state.
					GameState state = new GameState();
					state.AllMoves = new List<byte[]>();
					state.BoardState = newBoardMsg.BoardState;
					state.MatchState = newBoardMsg.MatchState;
					state.Player1ID = playerData.Value.PlayerID;
					state.Player2ID = match.PlayerID;
					lock (lock_activeGames)
					{
						activeGames.Add(state);
					}
				}
				//Otherwise, send the game state to him.
				else
				{
					var gameState = FindGame(playerData.Value.PlayerID);
					if (gameState == null)
					{
						Try(() => {throw new NullReferenceException("Couldn't find game with id " + playerData.Value.PlayerID);},
							"sending game state to player",
							onFailure);
						return;
					}

					//Send the game state.
					var gameStateMsg = new Messages.GameState(gameState.BoardState,
															  gameState.AllMoves.ToArray(),
															  gameState.MatchState,
															  gameState.Player1ID, gameState.Player2ID);
					if (!Try(() => Messages.Base.Write(gameStateMsg, streamWriter),
							"Giving game state to player", onFailure))
					{
						return;
					}

					//Wait for an acknowledgement.
					Messages.Base _acknowledgeMsg = null;
					if (!Try(() => _acknowledgeMsg = Messages.Base.Read(streamReader),
							 "Getting acknowledgement from client in 'CheckOpponentFound'",
							 onFailure))
					{
						return;
					}
					if (!(_acknowledgeMsg is Messages.Acknowledge))
						onFailure(null);
				}
			}
			else
			{
				var nullMsg = new Messages.FoundOpponent(null, ulong.MaxValue, false);
				Try(() => Messages.Base.Write(nullMsg, streamWriter),
					"Sending \"null opponent\" msg");
			}
		}
		private void SocketHandler_GetGameState(Messages.GetGameState msg, NetworkStream stream,
												BinaryReader streamReader, BinaryWriter streamWriter)
		{
			var game = FindGame(msg.PlayerID);
			if (game == null)
			{
				Try(() => {throw new NullReferenceException("Couldn't find game with id " + msg.PlayerID);},
					"sending game state to player");
				return;
			}

			byte[][] recentMoves = new byte[game.AllMoves.Count - msg.LastKnownMovement][];
			for (int i = 0; i < recentMoves.Length; ++i)
				recentMoves[i] = game.AllMoves[i + msg.LastKnownMovement];

			var gameMsg = new Messages.GameState(game.BoardState, recentMoves,
												 game.MatchState, game.Player1ID, game.Player2ID);
			if (!Try(() => Messages.Base.Write(gameMsg, streamWriter),
					 "telling game state"))
			{
				return;
			}

			//If the game is over, make sure the player acknowledges that fact.
			if (gameMsg.MatchState.IsGameOver())
			{
				Messages.Base _acknowledgeMsg = null;
				if (!Try(() => _acknowledgeMsg = Messages.Base.Read(streamReader),
					     "Getting acknowledgement from client in 'CheckOpponentFound'"))
				{
					return;
				}
				if (!(_acknowledgeMsg is Messages.Acknowledge))
				{
					Try(() => {throw new Exception("Expected Acknowledgement, got " + _acknowledgeMsg.Type);},
						"casting message to acknowledgement");
				}

				//Remove the game from various data structures.
				lock (lock_finishedGames)
					finishedGamesByUnacknowledgedPlayer.Remove(msg.PlayerID);
				lock (lock_activeGames)
					activeGames.Remove(game);
			}
		}
		private void SocketHandler_MakeMove(Messages.MakeMove msg, NetworkStream stream,
											BinaryReader streamReader, BinaryWriter streamWriter)
		{
			//Get the game the message is referencing.
			var game = FindGame(msg.PlayerID);
			if (game == null)
			{
				Try(() => {throw new NullReferenceException("Couldn't find game with id " + msg.PlayerID);},
					"sending game state to player");
				return;
			}

			//Update the game.
			//Hold onto the old game state in case something goes wrong.
			var oldBoardState = game.BoardState;
			var oldMatchState = game.MatchState;
			game.BoardState = msg.NewBoardState;
			game.AllMoves.Add(msg.Move);
			game.MatchState = msg.NewMatchState;

			//We need to let the other player know next time he checks into this server.
			if (msg.NewMatchState.IsGameOver())
			{
				lock (lock_finishedGames)
				{
					finishedGamesByUnacknowledgedPlayer.Add(game.GetOtherPlayer(msg.PlayerID), game);
				}
			}

			//Send an acknowledgement. If it fails, undo the changes.
			Try(() => Messages.Base.Write(new Messages.Acknowledge(), streamWriter),
				"sending move acknowledgement to client",
				e =>
				{
					lock (lock_finishedGames)
						finishedGamesByUnacknowledgedPlayer.Remove(game.GetOtherPlayer(msg.PlayerID));

					game.BoardState = oldBoardState;
					game.AllMoves.RemoveAt(game.AllMoves.Count - 1);
					game.MatchState = oldMatchState;
				});
		}
		private void SocketHandler_ForfeitGame(Messages.ForfeitGame msg, NetworkStream stream,
											   BinaryReader streamReader, BinaryWriter streamWriter)
		{
			//TODO: Implement. Make sure to add to finishedGamesByUnacknowledgedPlayer.

			//Get the game the message is referencing.
			var game = FindGame(msg.PlayerID);
			if (game == null)
			{
				Try(() => {throw new NullReferenceException("Couldn't find game with id " + msg.PlayerID);},
					"sending game state to player");
				return;
			}

			//End the game.
			var oldMatchState = game.MatchState;
			game.MatchState = (game.Player1ID == msg.PlayerID) ?
								  MatchStates.Player2Won :
								  MatchStates.Player1Won;
			lock (lock_finishedGames)
				finishedGamesByUnacknowledgedPlayer.Add(game.GetOtherPlayer(msg.PlayerID), game);

			//Send an acknowledgement. If it fails, undo the changes.
			Try(() => Messages.Base.Write(new Messages.Acknowledge(), streamWriter),
				"sending forfeit acknowledgement to client",
				e =>
				{
					lock (lock_finishedGames)
						finishedGamesByUnacknowledgedPlayer.Remove(game.GetOtherPlayer(msg.PlayerID));
					game.MatchState = oldMatchState;
				});
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

			lock (lock_activeGames)
			{
				writer.Write(activeGames.Count);
				foreach (var game in activeGames)
					game.Serialize(writer);
			}

			lock (lock_finishedGames)
			{
				writer.Write(finishedGamesByUnacknowledgedPlayer.Count);
				foreach (var idAndGame in finishedGamesByUnacknowledgedPlayer)
				{
					writer.Write(idAndGame.Key);
					idAndGame.Value.Serialize(writer);
				}
			}
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

			lock (lock_activeGames)
			{
				activeGames.Clear();
				int nGames = reader.ReadInt32();
				for (int i = 0; i < nGames; ++i)
				{
					var game = new Networking.GameState();
					game.Deserialize(reader);
					activeGames.Add(game);
				}
			}

			lock (lock_finishedGames)
			{
				finishedGamesByUnacknowledgedPlayer.Clear();
				int nGames = reader.ReadInt32();
				for (int i = 0; i < nGames; ++i)
				{
					new Networking.GameState();
				}
			}

			//The port may have just changed, so restart the listener just in case.
			RestartListener();
		}
	}
}
