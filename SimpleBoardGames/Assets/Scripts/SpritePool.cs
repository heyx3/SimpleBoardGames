using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An expandable collection of GameObjects with sprites.
/// on the board.
/// </summary>
public class SpritePool : Singleton<SpritePool>
{
	private List<SpriteRenderer> unusedSprites = new List<SpriteRenderer>();


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
			unusedSprites.RemoveAt(0);
		}
		return used;
	}
	public void DeallocateSprites(List<SpriteRenderer> sprites)
	{
		foreach (SpriteRenderer spr in sprites)
		{
			//Remove all components other than the SpriteRenderer and Transform.
			foreach (Component c in spr.gameObject.GetComponents<Component>())
				if (!(c is SpriteRenderer) && !(c is Transform))
					Destroy(c);

			//Remove all child objects.
			Transform tr = spr.transform;
			while (tr.childCount > 0)
				Destroy(tr.GetChild(0).gameObject);
						
			spr.gameObject.SetActive(false);

			unusedSprites.Add(spr);
		}
	}
}