using System;
using System.Collections.Generic;
using UnityEngine;


namespace BoardGames
{
	/// <summary>
	/// An expandable collection of GameObjects that represent possible movements that can be made
	/// on the board.
	/// </summary>
	public class MovementIndicators : Singleton<MovementIndicators>
	{
		public Sprite IndicatorSprite;
		public string GameObjectNames = "Possible Movement";
		public Transform ParentContainer = null;
		public int SortingLayer = 1;

		private List<SpriteRenderer> usedSprites = new List<SpriteRenderer>(),
									 unusedSprites = new List<SpriteRenderer>();


		public List<SpriteRenderer> AllocateSprites(int nSprites)
		{
			//Make sure there are enough unused sprites to allocate.
			while (unusedSprites.Count < nSprites)
			{
				SpriteRenderer spr = Utilities.CreateSprite(IndicatorSprite, GameObjectNames,
															null, ParentContainer,
															SortingLayer);
				spr.gameObject.SetActive(false);
				unusedSprites.Add(spr);
			}

			//Allocate the unused sprites.
			List<SpriteRenderer> used = new List<SpriteRenderer>();
			for (int i = 0; i < nSprites; ++i)
			{
				//Set up the GameObject for this move.
				unusedSprites[0].gameObject.SetActive(true);

				//Set up the various lists storing used/unused sprites.
				used.Add(unusedSprites[0]);
				usedSprites.Add(unusedSprites[0]);
				unusedSprites.RemoveAt(0);
			}
			return used;
		}
		public void DeallocateSprites(List<SpriteRenderer> sprites)
		{
			foreach (SpriteRenderer spr in sprites)
			{
				usedSprites.Remove(spr);
				unusedSprites.Add(spr);

				spr.gameObject.SetActive(false);
			}
		}
	}
}