using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using UnityEngine;

using Regex = System.Text.RegularExpressions.Regex;


namespace BoardGames
{
	public static class Utilities
	{
		/// <summary>
		/// Creates a GameObject with a sprite at the given position.
		/// </summary>
		public static SpriteRenderer CreateSprite(Sprite spr, string name = "Sprite",
												  Vector3? pos = null, Transform parent = null,
												  int sortOrder = 0)
		{
			GameObject go = new GameObject(name);
			Transform tr = go.transform;
			tr.parent = parent;

			if (pos.HasValue)
				tr.position = pos.Value;

			SpriteRenderer sprR = go.AddComponent<SpriteRenderer>();
			sprR.sprite = spr;
			sprR.sortingOrder = sortOrder;

			return sprR;
		}

		public static IPAddress GetLocalIP()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			return host.AddressList.FirstOrDefault(
				ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
		}
		public static IPAddress GetPublicIP()
		{
			string externalIP = webClient.DownloadString("http://checkip.dyndns.org/");
			return IPAddress.Parse(new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")
								         .Matches(externalIP)[0].ToString());
		}
		private static WebClient webClient = new WebClient();

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
}