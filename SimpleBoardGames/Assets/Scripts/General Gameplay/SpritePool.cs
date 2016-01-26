using System;
using System.Collections.Generic;
using UnityEngine;


namespace BoardGames
{
	/// <summary>
	/// An expandable collection of GameObjects with sprites.
	/// on the board.
	/// </summary>
	public class SpritePool : Singleton<SpritePool>
	{
		private List<SpriteRenderer> usedSprites = new List<SpriteRenderer>(),
									 unusedSprites = new List<SpriteRenderer>();


		public List<SpriteRenderer> AllocateSprites(int nSprites, Sprite spr,
													int sortingLayer = 1,
													Transform parentContainer = null,
													string names = "Pooled Sprite")
		{
			//Make sure there are enough unused sprites to allocate.
			while (unusedSprites.Count < nSprites)
			{
				GameObject go = new GameObject();
				SpriteRenderer sprR = go.AddComponent<SpriteRenderer>();
				go.SetActive(false);
				unusedSprites.Add(sprR);
			}

			//Allocate the unused sprites.
			List<SpriteRenderer> used = new List<SpriteRenderer>();
			for (int i = 0; i < nSprites; ++i)
			{
				//Set up the GameObject for this move.
				unusedSprites[0].gameObject.SetActive(true);
				unusedSprites[0].gameObject.name = names;
				unusedSprites[0].transform.parent = parentContainer;
				unusedSprites[0].sprite = spr;
				unusedSprites[0].sortingOrder = sortingLayer;

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