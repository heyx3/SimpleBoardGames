using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class TestServer : MonoBehaviour
{
	public int Port = 50111;

	private string myPublicIP = "",
				   myLocalIP = "";

	private List<string> msgs = new List<string>();
	private Vector2 msgsScrollPos = Vector2.zero;
	private Coroutine connectionCoroutine = null;
					private void Start() { BoardGames.Utilities.GetLocalIP(); }
	public System.Collections.IEnumerator Coroutine_TestConnection()
	{
		msgs.Clear();
		yield return null;

		TcpListener listener = new TcpListener(IPAddress.Any, Port);

		listener.Start();
		msgs.Add("Waiting for a connection...");
		yield return null;

		Socket socket = null;
		while (!listener.Pending())
			yield return null;
		if (!Try(() => socket = listener.AcceptSocket(), e => listener.Stop()))
			yield break;

		msgs.Add("Found a connection at " + socket.RemoteEndPoint);
			yield return new WaitForSeconds(0.5f);

		using (var clientStream = new NetworkStream(socket))
		using (var clientStreamReader = new BinaryReader(clientStream))
		using (var clientStreamWriter = new BinaryWriter(clientStream))
		{
			msgs.Add("Waiting for message...");
			yield return null;

			string receive = null;
			if (!Try(() => receive = clientStreamReader.ReadString(),
					 e  => { socket.Close(); listener.Stop(); }))
				yield break;
			msgs.Add("\"" + receive + "\"");
			yield return null;

			char[] chars = receive.ToCharArray();
			for (int i = 0; i < chars.Length; ++i)
			{
				if ((chars[i] >= 'a' && chars[i] < 'z') ||
					(chars[i] >= 'A' && chars[i] < 'Z') ||
					(chars[i] >= '0' && chars[i] < '9'))
				{
					chars[i] += (char)1;
				}
			}
			var toSend = new string(chars);

			msgs.Add("Sending \"" + toSend + "\"...");
			yield return null;
			if (!Try(() => clientStreamWriter.Write(toSend),
					 e  => { socket.Close(); listener.Stop(); }))
				yield break;

			yield return new WaitForSeconds(0.5f);
			msgs.Add("Finished!");
		}

		socket.Close();
		listener.Stop();
	}
	private bool Try(Action action, Action<Exception> onException = null)
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

			msgs.Add("Error: (" + e.GetType() + ") " + e.Message);
			return false;
		}
	}

	public void OnGUI()
	{
		GUILayout.Label("Server");
		GUILayout.Space(15.0f);
		if (GUILayout.Button("Find connection", GUILayout.MaxWidth(200.0f)))
		{
			if (connectionCoroutine != null)
				StopCoroutine(connectionCoroutine);
			connectionCoroutine = StartCoroutine(Coroutine_TestConnection());
		}

		//Public/local IP:
		GUILayout.BeginHorizontal();
		GUILayout.Label("Public IP: " + myPublicIP);
		GUILayout.Space(15.0f);
		GUILayout.Label("Local IP:" + myLocalIP);
		GUILayout.Space(15.0f);
		if (GUILayout.Button("Refresh"))
		{
			myPublicIP = BoardGames.Utilities.GetPublicIP().ToString();
			myLocalIP = BoardGames.Utilities.GetLocalIP().ToString();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		//Port:
		GUILayout.BeginHorizontal();
		GUILayout.Label("Port:");
		int p;
		if (int.TryParse(GUILayout.TextField(Port.ToString()), out p))
			Port = p;
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(25.0f);

		//Log messages:
		msgsScrollPos = GUILayout.BeginScrollView(msgsScrollPos);
		for (int i = 0; i < msgs.Count; ++i)
			GUILayout.Label(msgs[i]);
		GUILayout.EndScrollView();
	}
}