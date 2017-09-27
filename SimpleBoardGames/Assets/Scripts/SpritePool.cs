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


	public SpriteRenderer AllocateSprite(Sprite spr, int sortingLayer = 1,
										 Transform parentContainer = null,
										 string names = "Pooled Sprite")
	{
		//Make sure there are enough unused sprites to allocate.
		if (unusedSprites.Count == 0)
		{
			GameObject go = new GameObject();
			SpriteRenderer sprR = go.AddComponent<SpriteRenderer>();
			go.SetActive(false);
			unusedSprites.Add(sprR);
		}

		//Allocate the unused sprites.

		var sprRnd = unusedSprites[0];
		unusedSprites.RemoveAt(0);

		sprRnd.gameObject.SetActive(true);
		sprRnd.gameObject.name = names;
		sprRnd.transform.parent = parentContainer;
		sprRnd.sprite = spr;
		sprRnd.sortingOrder = sortingLayer;

		return sprRnd;
	}
	public List<SpriteRenderer> AllocateSprites(int nSprites, Sprite spr,
												int sortingLayer = 1,
												Transform parentContainer = null,
												string names = "Pooled Sprite")
	{
		List<SpriteRenderer> used = new List<SpriteRenderer>();
		for (int i = 0; i < nSprites; ++i)
			used.Add(AllocateSprite(spr, sortingLayer, parentContainer, names));
		return used;
	}

	public void DeallocateSprite(SpriteRenderer spr)
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
	public void DeallocateSprites(List<SpriteRenderer> sprites)
	{
		foreach (SpriteRenderer spr in sprites)
			DeallocateSprite(spr);
	}
}