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

		public List<GameState> ActiveGames = new List<GameState>();
		public int Port = 50111;

		public int MaxMessages = 100;
		public bool LogMessagesInUnity = true;

		private Logger logger;
		private TcpListener networkListener = null;


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

					//Restart the network listener.
					networkListener.Stop();
					networkListener = new TcpListener(IPAddress.Any, Port);
					networkListener.Start();

					logger.Add(Message.Warning("Restarting network listener with new port " + Port));
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
				//TODO: Implement.
			}
		}
	}
}
