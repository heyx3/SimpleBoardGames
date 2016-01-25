using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Allows various sprites to be clicked on.
/// </summary>
public class SpriteSelector : Singleton<SpriteSelector>
{
	/// <summary>
	/// Returns true if this event should block any other sprites from being clicked on this frame.
	/// </summary>
	public delegate bool ClickedEventDelegate(SpriteRenderer spriteClickedOn, Vector2 tapWorldPos);


	public Camera SpriteCam;

	/// <summary>
	/// All objects currently being tracked for clicks/taps.
	/// NOTE: Make sure to remove destroyed objects from this collection! It won't happen automatically.
	/// </summary>
	public Dictionary<SpriteRenderer, ClickedEventDelegate> Objects;


	protected override void Awake()
	{
		base.Awake();
		Objects = new Dictionary<SpriteRenderer, ClickedEventDelegate>();
	}
	void Update()
	{
		//Get taps/mouse clicks and see if any sprites were clicked on.

		bool tapped = false;
		Vector2 tapPos = Vector2.zero;

		if (Input.GetMouseButtonDown(0))
		{
			tapped = true;
			tapPos = (Vector2)SpriteCam.ScreenToWorldPoint(Input.mousePosition);
		}
		else if (Input.touchCount > 0)
		{
			tapped = true;
			tapPos = (Vector2)SpriteCam.ScreenToWorldPoint((Vector3)Input.GetTouch(0).position);
		}

		if (tapped)
		{
			foreach (KeyValuePair<SpriteRenderer, ClickedEventDelegate> kvp in Objects)
			{
				SpriteRenderer spr = kvp.Key;
				ClickedEventDelegate onClicked = kvp.Value;

				if (spr.bounds.Contains((Vector3)tapPos))
				{
					if (onClicked(spr, tapPos))
						break;
				}
			}
		}
	}
}