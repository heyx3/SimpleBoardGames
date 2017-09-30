using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class TestClient : MonoBehaviour
{
	public string IP = "127.0.0.1";
	public int Port = 50111;
	public string ServerMsg = "Hi Server!";
	public int Timeout = 3000;

	private List<string> msgs = new List<string>();
	private Vector2 msgsScrollPos = Vector2.zero;
	private Coroutine connectionCoroutine = null;

	public System.Collections.IEnumerator Coroutine_TestConnection()
	{
		msgs.Clear();
		yield return null;

		using (var client = new TcpClient())
		{
			msgs.Add("Connecting...");
			yield return null;
			if (!Try(() => client.Connect(IP, Port)))
				yield break;

			msgs.Add("Connected");
			yield return null;
			using (var serverStream = client.GetStream())
			using (var serverStreamReader = new BinaryReader(serverStream))
			using (var serverStreamWriter = new BinaryWriter(serverStream))
			{
				serverStream.ReadTimeout = Timeout;
				serverStream.WriteTimeout = Timeout;

				msgs.Add("Transmitting \"" + ServerMsg + "\"...");
				yield return null;
				if (!Try(() => serverStreamWriter.Write(ServerMsg)))
					yield break;

				msgs.Add("Receiving...");
				yield return null;
				string received = null;

				//Do this on another thread so it doesn't block.
				Thread thread = new Thread(() =>
				{
					Try(() => received = serverStreamReader.ReadString());
				});
				thread.Start();
				while (thread.IsAlive)
					yield return null;

				msgs.Add("\"" + received + "\"");
			}

			yield return new WaitForSeconds(0.5f);
			msgs.Add("Finished!");
		}
	}
	private bool Try(Action action)
	{
		try
		{
			action();
			return true;
		}
		catch (Exception e)
		{
			msgs.Add("Error: (" + e.GetType() + ") " + e.Message);
			return false;
		}
	}

	public void OnGUI()
	{
		GUILayout.BeginArea(new Rect(Screen.width * 0.5f, 0.0f,
									 Screen.width * 0.5f, Screen.height));

		GUILayout.Label("Client");
		GUILayout.Space(15.0f);
		if (GUILayout.Button("Run connection", GUILayout.MaxWidth(200.0f)))
		{
			if (connectionCoroutine != null)
				StopCoroutine(connectionCoroutine);
			connectionCoroutine = StartCoroutine(Coroutine_TestConnection());
		}

		//IP/Port:
		GUILayout.BeginHorizontal();
		IP = GUILayout.TextField(IP);
		GUILayout.Label(" : ");
		int p;
		if (int.TryParse(GUILayout.TextField(Port.ToString()), out p))
			Port = p;
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		//Timeout:
		GUILayout.BeginHorizontal();
		GUILayout.Label("Timeout (milliseconds, -1 for infinity):");
		int t;
		if (int.TryParse(GUILayout.TextField(Timeout.ToString()), out t))
			Timeout = t;
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(15.0f);

		//Server Message:
		GUILayout.BeginHorizontal();
		ServerMsg = GUILayout.TextField(ServerMsg);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(25.0f);

		//Log messages:
		msgsScrollPos = GUILayout.BeginScrollView(msgsScrollPos);
		for (int i = 0; i < msgs.Count; ++i)
			GUILayout.Label(msgs[i]);
		GUILayout.EndScrollView();

		GUILayout.EndArea();
	}
}