using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace BoardGames.UnityLogic.GameMode
{
	public abstract class GameMode<LocationType> : Singleton<GameMode<LocationType>>
		where LocationType : IEquatable<LocationType>
	{
		public Board<LocationType> TheBoard { get; private set; }

		public Stat<Players, GameMode<LocationType>> CurrentTurn { get; private set; }


		protected abstract Board<LocationType> CreateNewBoard();
		protected abstract void OnMove(Movement<LocationType> move);
		protected abstract void OnPlace(Placement<LocationType> place);
		
		/// <summary>
		/// Returns whether it was successful.
		/// </summary>
		public bool SaveGame(string filePath)
		{
			try
			{
				using (BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.Truncate)))
				{
					SaveGame(writer);
				}
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError("Error writing " + filePath +
							       " (" + e.GetType().Name + "): " + e.Message);
				return false;
			}
		}
		/// <summary>
		/// Returns whether it was successful.
		/// </summary>
		public bool LoadGame(string filePath)
		{
			try
			{
				byte[] fileData = File.ReadAllBytes(filePath);
				using (BinaryReader reader = new BinaryReader(new MemoryStream(fileData, false)))
				{
					LoadGame(reader);
				}
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError("Error reading " + filePath +
							       " (" + e.GetType().Name + "): " + e.Message);
				return false;
			}
		}

		public virtual void LoadGame(System.IO.BinaryReader stream)
		{
			TheBoard.Deserialize(stream);
			CurrentTurn.Value = (Players)stream.ReadInt32();
		}
		public virtual void SaveGame(System.IO.BinaryWriter stream)
		{
			TheBoard.Serialize(stream);
			stream.Write((int)CurrentTurn.Value);
		}

		protected override void Awake()
		{
			base.Awake();

			TheBoard = CreateNewBoard();
			CurrentTurn = new Stat<Players, GameMode<LocationType>>(this, Players.One);

			TheBoard.OnMove += (board, move) => OnMove(move);
			TheBoard.OnPlace += (board, place) => OnPlace(place);
		}
	}
}
