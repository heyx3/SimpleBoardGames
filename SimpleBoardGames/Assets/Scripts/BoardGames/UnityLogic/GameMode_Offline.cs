using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


//TODO: A "NetworkReplay" GameMode that just shows a single move for the player to watch.
//TODO: A "NetworkPlay" GameMode that lets the player take the next move.

namespace BoardGames.UnityLogic.GameMode
{
	/// <summary>
	/// A local match, where the game takes place on a single device.
	/// Loads a game-in-progress from a specific file if it exists,
	///     otherwise starts a new one from scratch.
	/// Every time the turn changes, the current game state is written to the file.
	/// </summary>
	public abstract class GameMode_Offline<LocationType> : GameMode<LocationType>
		where LocationType : IEquatable<LocationType>
	{
		/// <summary>
		/// NOTE: This is the name of the game's save file,
		///     so make sure it doesn't use any strange characters.
		/// </summary>
		protected abstract string GameName { get; }

		private string filePath { get { return Path.Combine(Application.dataPath, GameName + ".save"); } }
		

		public void Quit(bool saveProgress)
		{
			if (saveProgress)
				SaveGame(filePath);
			else if (File.Exists(filePath))
				File.Delete(filePath);

			UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
		}

		protected override void Awake()
		{
			base.Awake();

			//If the save file exists, load the game.
			if (File.Exists(filePath))
				LoadGame(filePath);

			//Whenever the turn changes, write the new game state to the file.
			CurrentTurn.OnChanged += (thisMode, oldTurn, newTurn) =>
			{
				SaveGame(filePath);
			};
		}
	}
}
