using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BoardGames.Networking
{
	/// <summary>
	/// Thread-safe database of players looking for matches.
	/// </summary>
	public class PlayerMatcher
	{
		public struct Player : IEquatable<Player>
		{
			public string Name;
			public ulong PlayerID, GameID;

			public Player(string name, ulong playerID, ulong gameID)
			{
				Name = name;
				PlayerID = playerID;
				GameID = gameID;
			}

			public override int GetHashCode()
			{
				return unchecked((int)((ulong)Name.GetHashCode() + PlayerID + PlayerID));
			}
			public bool Equals(Player p)
			{
				return Name.Equals(p.Name) && PlayerID == p.PlayerID && GameID == p.GameID;
			}
		}


		private Dictionary<Player, Player?> opponents = new Dictionary<Player, Player?>();

		private object locker = new object();


		/// <summary>
		/// Adds a player to the matchmaking queue.
		/// </summary>
		public void Push(Player playerData)
		{
			lock (locker)
			{
				opponents.Add(playerData, null);
			}
		}
		/// <summary>
		/// Adds a player to the matchmaking queue, and immediately gives him the given opponent.
		/// Does NOT add the opponent to the "queue" in the same way.
		/// </summary>
		public void Push(Player player, Player opponent)
		{
			lock (locker)
			{
				opponents.Add(player, opponent);
			}
		}
		/// <summary>
		/// Removes all players from the queue.
		/// </summary>
		public void Clear()
		{
			lock (locker)
			{
				opponents.Clear();
			}
		}

		/// <summary>
		/// Does the given action to each player/opponent pair in the matchmaking queue.
		/// </summary>
		/// <param name="doGivenCount">
		/// This action is executed before "doToEach".
		/// It takes in the number of players in the queue.
		/// </param>
		public void Foreach(System.Action<int> doGivenCount,
							System.Action<Player, Player?> doToEach)
		{
			lock (locker)
			{
				doGivenCount(opponents.Count);
				foreach (var kvp in opponents)
					doToEach(kvp.Key, kvp.Value);
			}
		}

		/// <summary>
		/// Tries to find the Player with the given ID.
		/// </summary>
		public Player? TryGetKey(ulong playerID)
		{
			lock (locker)
			{
				return opponents.Keys.FirstOrDefault(p => p.PlayerID == playerID);
			}
		}
		/// <summary>
		/// If the given player finally has a match,
		///     removes him from this queue and returns his opponent.
		/// Otherwise, returns null.
		/// </summary>
		/// <param name="wasFirst">
		/// If an opponent exists, this variable represents
		///     whether the given player is player 1 or player 2.
		/// </param>
		public Player? TryPop(Player playerData, out bool wasFirst)
		{
			lock (locker)
			{
				if (opponents[playerData].HasValue)
				{
					var opponent = opponents[playerData];
					opponents.Remove(playerData);

					wasFirst = opponents.ContainsKey(opponent.Value);
					return opponent;
				}
			}

			wasFirst = false;
			return null;
		}

		/// <summary>
		/// Runs through the database to find player matches.
		/// </summary>
		public void FindMatches()
		{
			lock (locker)
			{
				//Find any two waiting players with the same game ID.
				var waitingPlayers = opponents.Keys.ToList();
				foreach (var player1 in waitingPlayers)
					if (opponents[player1] == null)
						foreach (var player2 in waitingPlayers)
							if (opponents[player2] == null)
								if (player1.PlayerID != player2.PlayerID && player1.GameID == player2.GameID)
								{
									opponents[player1] = player2;
									opponents[player2] = player1;
									break;
								}
			}
		}
	}
}
