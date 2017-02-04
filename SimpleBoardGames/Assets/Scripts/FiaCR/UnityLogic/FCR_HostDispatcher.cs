using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FiaCR.UnityLogic
{
	public class FCR_HostDispatcher : MonoBehaviour
	{
		public GameObject HostPrefab_Enemy, HostPrefab_Friend;

		private Dictionary<Vector2i, GameObject> HostsByPos = new Dictionary<Vector2i, GameObject>();


		private void Start()
		{
			var gameMode = (GameMode.FCR_Game_Offline)GameMode.FCR_Game_Offline.Instance;
			var board = (Board)gameMode.TheBoard;

			board.OnHostCreated += Callback_HostCreated;
			board.OnHostDestroyed += Callback_HostDestroyed;

			foreach (var hostPos in board.Hosts)
				Callback_HostCreated(board, hostPos, board.GetHost(hostPos).Value);
		}
		private void OnDestroy()
		{
			var gameMode = (GameMode.FCR_Game_Offline)GameMode.FCR_Game_Offline.Instance;
			if (gameMode == null)
				return;

			var board = (Board)gameMode.TheBoard;

			board.OnHostCreated -= Callback_HostCreated;
			board.OnHostDestroyed -= Callback_HostDestroyed;
		}
		
		private void Callback_HostCreated(Board board, Vector2i pos, BoardGames.Players owner)
		{
			if (owner == Board.Player_Humans)
				HostsByPos.Add(pos, Instantiate(HostPrefab_Friend));
			else if (owner == Board.Player_TC)
				HostsByPos.Add(pos, Instantiate(HostPrefab_Enemy));
			else
				throw new NotImplementedException(owner.ToString());

			HostsByPos[pos].transform.position = board.ToWorld(pos);
		}
		private void Callback_HostDestroyed(Board board, Vector2i pos, BoardGames.Players owner)
		{
			Destroy(HostsByPos[pos]);
			HostsByPos.Remove(pos);
		}
	}
}