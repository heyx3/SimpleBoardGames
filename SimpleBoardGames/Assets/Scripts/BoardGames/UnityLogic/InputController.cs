using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BoardGames.UnityLogic
{
	/// <summary>
	/// Handles mouse/tap input.
	/// </summary>
	public abstract class InputController<LocationType> : Singleton<InputController<LocationType>>
	{
		/// <summary>
		/// Raised when the mouse clicks on a board position.
		/// </summary>
		public event Action<LocationType> OnBoardPosClicked;
		/// <summary>
		/// Raised when the mouse clicks on somewhere outside a board position.
		/// The argument is the mouse position.
		/// </summary>
		public event Action<Vector3> OnWorldClicked;


		/// <summary>
		/// Attempts to find the position on the game board the mouse is pointing to, given its position.
		/// Should return nothing if the mouse isn't pointing to a valid position.
		/// </summary>
		protected abstract Optional<LocationType> ToBoardPos(Vector3 mousePos);


		public void Callback_Click(Vector3 pos)
		{
			Optional<LocationType> clickedPos = ToBoardPos(pos);

			if (clickedPos.HasValue)
			{
				if (OnBoardPosClicked != null)
					OnBoardPosClicked(clickedPos.Value);
			}
			else
			{
				if (OnWorldClicked != null)
					OnWorldClicked(pos);
			}
		}
	}
}