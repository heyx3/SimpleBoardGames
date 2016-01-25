using System;
using System.Collections.Generic;
using UnityEngine;


public static class Utilities
{
	/// <summary>
	/// Creates a GameObject with a sprite at the given position.
	/// </summary>
	public static SpriteRenderer CreateSprite(Sprite spr, string name = "Sprite",
											  Vector3? pos = null, Transform parent = null,
											  int sortOrder = 0)
	{
		GameObject go = new GameObject(name);
		Transform tr = go.transform;
		tr.parent = parent;

		if (pos.HasValue)
			tr.position = pos.Value;

		SpriteRenderer sprR = go.AddComponent<SpriteRenderer>();
		sprR.sprite = spr;
		sprR.sortingOrder = sortOrder;

		return sprR;
	}
}