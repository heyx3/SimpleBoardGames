using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Fitchneil.UnityLogic
{
	public class FN_InputController : BoardGames.UnityLogic.InputController<Vector2i>
	{
		protected override Optional<Vector2i> ToBoardPos(Vector3 mousePos)
		{
			Vector2i pos = Board.ToBoard(mousePos);
			if (Board.IsInBounds(pos))
				return pos;
			else
				return null;
		}
	}
}
