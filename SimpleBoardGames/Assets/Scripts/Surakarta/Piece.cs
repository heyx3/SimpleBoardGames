using System;
using System.Collections.Generic;
using UnityEngine;


namespace Surakarta
{
	public class Piece : BoardGames.Piece<Vector2i>
	{
		protected override void Awake()
		{
			base.Awake();
		}

		public override void OnStartClick(Vector2 mouseWorldPos)
		{
			//TODO: Pop-up movement arrows.
		}
	}
}