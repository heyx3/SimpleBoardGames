using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

using GameMessageBase = BoardGames.Networking.Messages.Base;
using MatchStates = BoardGames.Networking.Messages.MatchStates;


namespace BoardGames.UnityLogic.GameMode
{
	//TODO: the matchmaking file data is actually part of the normal game saving/loading.


	public abstract class GameMode_Networked<LocationType> : GameMode<LocationType>
		where LocationType : IEquatable<LocationType>
	{
		/*
			Implementation note:
			The UI for talking to the server is controlled by the GameClientUI class.
			It has two modes: waiting to talk to the server, and waiting for a match.
			If the server gives us a match to start/resume, we disable that UI
			    and allow the normal game UI to play out.
		*/


		private static bool Try(Action a)
		{
			try
			{
				a();
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// A unique ID identifying this board game.
		/// Must be a compile-time constant.
		/// </summary>
		protected abstract ulong GameID { get; }


		/// <summary>
		/// The location of the file containing the status of finding a match on the server.
		/// </summary>
		private string matchmakingDataFilePath
		{
			get
			{
				return Path.Combine((Application.isEditor ?
										 Path.Combine(Application.dataPath, "..\\") :
										 Application.dataPath),
									Path.Combine("ClientStatus", GameID + ".status"));
			}
		}

		private GameClientUI clientUI { get { return GameClientUI.Instance; } }


		public GameObject Scene_ClientUI, Scene_Game;

		private Thread serverCommsThread = null;
		private Networking.Messages.GameState serverResult_stateToLoad = null; //TODO: Check for this to not be null, then call LoadGameState().
		private bool? serverResult_forfeited = null; //TODO: Check for this to not be null, then react.


		protected override void Awake()
		{
			base.Awake();

			Scene_Game.SetActive(false);
			Scene_ClientUI.SetActive(false);
		}


		/// <summary>
		/// Reads from disk the data structure containing the current status
		///     of finding a match on the server.
		/// </summary>
		private ClientMatchmakingData GetMatchmakingData()
		{
			try
			{
				var data = new ClientMatchmakingData();
				using (var stream = File.OpenRead(matchmakingDataFilePath))
				using (var reader = new BinaryReader(stream))
					data.Deserialize(reader);
				return data;
			}
			catch (Exception e)
			{
				clientUI.SetErrorMsg("Failure reading " + matchmakingDataFilePath +
									 ": " + e.Message);
				return new ClientMatchmakingData();
			}
		}
		private void SetMatchmakingData(ClientMatchmakingData data)
		{
			try
			{
				using (var stream = File.OpenWrite(matchmakingDataFilePath))
				using (var writer = new BinaryWriter(stream))
					data.Serialize(writer);
			}
			catch (Exception e)
			{
				clientUI.SetErrorMsg("Failure writing " + matchmakingDataFilePath +
									 ": " + e.Message);
			}
		}

		/// <summary>
		/// Starts talking to the server on another thread.
		/// Returns that thread.
		/// </summary>
		private Thread ContactServer()
		{
			//If nothing is going on, try connecting to a server to ask for a match.
			if (!File.Exists(matchmakingDataFilePath))
			{
				Scene_ClientUI.SetActive(true);
				clientUI.SetIsFindingMatch(false);
				return TalkToServer(new IPEndPoint(IPAddress.Parse(clientUI.ServerIP),
												   clientUI.ServerPort),
									TalkToServer_FirstContact);
			}
			else
			{
				//We must have a presence on the server already.
				var matchmakingData = GetMatchmakingData();

				//If we're still waiting for a match, check up on that.
				if (matchmakingData.OpponentName == null)
				{
					Scene_ClientUI.SetActive(true);
					clientUI.SetIsFindingMatch(true);
					return TalkToServer(matchmakingData.Server, TalkToServer_CheckMatch);
				}
				//Otherwise, pull the game state and load the board.
				else
				{
					Scene_Game.SetActive(true);
					return TalkToServer(matchmakingData.Server, TalkToServer_GetMatchState);
				}
			}
		}
		private void ConfigureServerStream(NetworkStream stream)
		{
			stream.WriteTimeout = clientUI.Timeout;
			stream.ReadTimeout = clientUI.Timeout;
		}
		private Thread TalkToServer(IPEndPoint serverAddress,
								    Action<TcpClient, NetworkStream, BinaryWriter, BinaryReader> talkFunc)
		{
			Thread thr = new Thread(() =>
			{
				using (var client = new TcpClient())
				{
					if (!Try(() => client.Connect(serverAddress)))
					{
						clientUI.SetErrorMsg("Couldn't contact server");
						return;
					}
					using (var serverStream = client.GetStream())
					using (var serverReader = new BinaryReader(serverStream))
					using (var serverWriter = new BinaryWriter(serverStream))
					{
						ConfigureServerStream(serverStream);
						talkFunc(client, serverStream, serverWriter, serverReader);
					}
				}
			});

			thr.Start();
			return thr;
		}

		private void LoadGameState(Networking.Messages.GameState state)
		{
			//Load the board.
			using (var boardStream = new MemoryStream(state.BoardState))
			using (var boardReader = new BinaryReader(boardStream))
				TheBoard.Deserialize(boardReader);

			//TODO: Load up the rest of the match state.
		}


		//The below methods contain the actual client-server message-passing code.
		//They are written to line up with the messaging code in Server.cs.

		private void TalkToServer_FirstContact(TcpClient client, NetworkStream serverStream,
											   BinaryWriter serverWriter, BinaryReader serverReader)
		{
			//Ask the server to find a game for me.
			var msg_findGame = new Networking.Messages.FindGame(clientUI.MyName,
																GameID);
			if (!Try(() => GameMessageBase.Write(msg_findGame, serverWriter)))
			{
				clientUI.SetErrorMsg("Couldn't send 'FindGame' to server");
				return;
			}

			//Check the response.
			var queueResult = TryReadMsg<Networking.Messages.SuccessfullyInQueue>(
							      serverReader,
								  "getting 'FindGame' response");
			if (queueResult == null)
				return;

			//Store the result.
			var outData = new ClientMatchmakingData((IPEndPoint)client.Client.RemoteEndPoint,
													queueResult.PlayerID);
			SetMatchmakingData(outData);
		}
		private void TalkToServer_CheckMatch(TcpClient client, NetworkStream serverStream,
											 BinaryWriter serverWriter, BinaryReader serverReader)
		{
			var matchMakingData = GetMatchmakingData();

			//Ask the server whether a match was found.
			var initialMsg = new Networking.Messages.CheckOpponentFound(matchMakingData.PlayerID);
			if (!Try(() => GameMessageBase.Write(initialMsg, serverWriter)))
			{
				clientUI.SetErrorMsg("Couldn't send 'CheckMatch' message");
				return;
			}

			//Get the response.
			var opponentInfo = TryReadMsg<Networking.Messages.FoundOpponent>(
							       serverReader,
								   "getting 'CheckMatch' response");
			if (opponentInfo == null)
				return;

			//If an opponent hasn't been found yet, check in again later.
			if (opponentInfo.OpponentName == null)
				return;

			//If this player is going first, we need to generate the initial game board.
			//Note that a game board has already been generated; we can just use that.
			if (opponentInfo.AmIGoingFirst)
			{
				byte[] boardBytes = null;
				using (var boardStream = new MemoryStream())
				{
					using (var boardWriter = new BinaryWriter(boardStream))
						TheBoard.Serialize(boardWriter);
					boardBytes = boardStream.GetBuffer();
				}

				var boardStateMsg = new Networking.Messages.NewBoard(boardBytes,
																	 MatchStates.Player1Turn);
				if (!Try(() => GameMessageBase.Write(boardStateMsg, serverWriter)))
				{
					clientUI.SetErrorMsg("Couldn't send 'NewBoard' response");
					return;
				}
			}
			//Otherwise, we need to get the current game state.
			else
			{
				//Get and load the state.
				var gameState = TryReadMsg<Networking.Messages.GameState>(
									serverReader,
								    "getting first 'GameState'");
				if (gameState == null)
					return;
				serverResult_stateToLoad = gameState;

				//Acknowledge.
				var acknowledge = new Networking.Messages.Acknowledge();
				if (!Try(() => GameMessageBase.Write(acknowledge, serverWriter)))
				{
					clientUI.SetErrorMsg("Couldn't send 'Ack' after first game state");
					return;
				}

				//Start the game UI.
				Scene_ClientUI.SetActive(false);
				Scene_Game.SetActive(true);
			}
		}
		private void TalkToServer_GetMatchState(TcpClient client, NetworkStream serverStream,
							  				    BinaryWriter serverWriter, BinaryReader serverReader)
		{
			//Send the request.
			var matchData = GetMatchmakingData();
			var request = new Networking.Messages.GetGameState(matchData.PlayerID,
															   matchData.LastKnownMove);
			if (!Try(() => GameMessageBase.Write(request, serverWriter)))
			{
				clientUI.SetErrorMsg("Error sending 'GetGameState' request");
				return;
			}

			//Get the state from the server and load it.
			var gameState = TryReadMsg<Networking.Messages.GameState>(serverReader,
																	  "getting game state");
			if (gameState == null)
				return;
			serverResult_stateToLoad = gameState;

			//If the game has ended, we need to tell the server that we definitely received it.
			if (Networking.Messages.Extensions.IsGameOver(gameState.MatchState))
			{
				var acknowledge = new Networking.Messages.Acknowledge();
				if (!Try(() => GameMessageBase.Write(acknowledge, serverWriter)))
				{
					clientUI.SetErrorMsg("Error sending 'Ack' after game state");
					return;
				}
			}
		}
		private void TalkToServer_MakeMove(TcpClient client, NetworkStream serverStream,
										   BinaryWriter serverWriter, BinaryReader serverReader)
		{
			//TODO: How does the move get passed in here? Maybe go through with my refactoring idea.
		}
		private void TalkToServer_Forfeit(TcpClient client, NetworkStream serverStream,
										  BinaryWriter serverWriter, BinaryReader serverReader)
		{
			var matchData = GetMatchmakingData();

			//Send the request.
			var request = new Networking.Messages.ForfeitGame(matchData.PlayerID);
			if (!Try(() => GameMessageBase.Write(request, serverWriter)))
			{
				clientUI.SetErrorMsg("Error sending 'ForfeitGame' request");
				return;
			}

			//Receive an acknowledgement.
			var ack = TryReadMsg<Networking.Messages.Acknowledge>(serverReader,
																  "getting 'Forfeit' ack");
			serverResult_forfeited = (ack != null);
		}

		/// <summary>
		/// Tries to read a message of the given type from the given stream.
		/// If there is an error, outputs the error to the UI and returns null.
		/// Otherwise, returns the message.
		/// </summary>
		/// <param name="thisAction">Used when printing an error.</param>
		private T TryReadMsg<T>(BinaryReader reader, string thisAction)
			where T : GameMessageBase
		{
			GameMessageBase message = null;
			if (!Try(() => message = GameMessageBase.Read(reader)))
			{
				clientUI.SetErrorMsg("Unable to get message " + thisAction);
				return null;
			}

			if (message.Type == Networking.Messages.Types.Error &&
				typeof(T) != typeof(Networking.Messages.Error))
			{
				var errMsg = (Networking.Messages.Error)message;
				clientUI.SetErrorMsg("Error " + thisAction + ": " + errMsg.Msg);
				return null;
			}

			if (message.GetType() != typeof(T))
			{
				clientUI.SetErrorMsg("Unexpected message type " + message.GetType().Name +
									 " " + thisAction);
				return null;
			}

			return (T)message;
		}
	}
}
