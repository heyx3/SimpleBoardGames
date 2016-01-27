using System;
using System.Collections;
using UnityEngine;


namespace BoardGames
{
	/// <summary>
	/// A piece on a game board. Generally has a collider (for click detection) and a sprite.
	/// </summary>
	public abstract class Piece<LocationType> : InputResponder
	{
		public Players Owner;

		[NonSerialized]
		public LocationType CurrentPos;


		public SpriteRenderer MySpr { get; private set; }


		protected override void Awake()
		{
			base.Awake();

			MySpr = GetComponent<SpriteRenderer>();
		}
	}
}