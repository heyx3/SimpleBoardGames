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


		//The below methods contain the actual client-server message-passing code.
		//The program flow is written to line up with the messaging code in Server.cs.

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
			GameMessageBase response = null;
			bool success = Try(() => response = GameMessageBase.Read(serverReader));
			if (!success ||
				(response.Type != Networking.Messages.Types.SuccessfullyInQueue &&
				 response.Type != Networking.Messages.Types.Error))
			{
				clientUI.SetErrorMsg("Couldn't get/understand 'FindGame' response");
				return;
			}

			//If it's an error, give up.
			if (response.Type == Networking.Messages.Types.Error)
			{
				var errMsg = (Networking.Messages.Error)response;
				clientUI.SetErrorMsg("Server error on 'FindGame': " + errMsg.Msg);
				return;
			}

			//Store the data from the response.
			var matchmakingQueueData = (Networking.Messages.SuccessfullyInQueue)response;
			var outData = new ClientMatchmakingData((IPEndPoint)client.Client.RemoteEndPoint,
													matchmakingQueueData.PlayerID);
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
			GameMessageBase response = null;
			if (!Try(() => response = GameMessageBase.Read(serverReader)) ||
				(response.Type != Networking.Messages.Types.FoundOpponent &&
				 response.Type != Networking.Messages.Types.Error))
			{
				clientUI.SetErrorMsg("Couldn't get/understand 'CheckMatch' response");
				return;
			}

			//If the response is an error, give up.
			if (response.Type == Networking.Messages.Types.Error)
			{
				var errMsg = (Networking.Messages.Error)response;
				clientUI.SetErrorMsg("Server error on 'CheckMatch': " + errMsg.Msg);
				return;
			}

			var opponentInfo = (Networking.Messages.FoundOpponent)response;

			//If an opponent hasn't been found yet, check in again later.
			if (opponentInfo.OpponentName == null)
				return;

			//If this player is going first, we need to generate the initial game board.
			if (opponentInfo.AmIGoingFirst)
			{
				byte[] boardBytes = null;
				using (var boardStream = new MemoryStream())
				{
					using (var boardWriter = new BinaryWriter(boardStream))
						TheBoard.Serialize(boardWriter);
					boardStream.Flush();
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
				//TODO: Implement.
			}
		}
		private void TalkToServer_GetMatchState(TcpClient client, NetworkStream serverStream,
							  				    BinaryWriter serverWriter, BinaryReader serverReader)
		{
			//TODO: Implement.
		}
	}
}
