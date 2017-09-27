using System;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// Handles click and touch inputs on GameObects.
/// </summary>
public class InputManager : Singleton<InputManager>
{
	/// <summary>
	/// Gets whether the mouse is currently down (not necessarily on an actual object).
	/// </summary>
	public bool IsClicking { get; private set; }

	/// <summary>
	/// Gets the object currently being clicked on (or null if nothing is currently clicked on).
	/// </summary>
	public InputResponder CurrentlyClicked { get; private set; }
	/// <summary>
	/// Whether the InputResponder currently being clicked on is now being dragged around.
	/// </summary>
	public bool IsDragging { get; private set; }

	/// <summary>
	/// Gets the world position of the mouse the last time it was initially pressed down.
	/// </summary>
	public Vector2 StartClickWorldPos { get; private set; }
	/// <summary>
	/// Gets the world position of the mouse when it most recently was pressed down.
	/// </summary>
	public Vector2 CurrentClickWorldPos { get; private set; }


	/// <summary>
	/// Used to convert clicks/taps to world coordinates.
	/// </summary>
	public Camera MainCam;


	protected override void Awake()
	{
		base.Awake();

		IsClicking = false;
		IsDragging = false;
		CurrentlyClicked = null;

		Assert.IsNotNull(MainCam, "'MainCam' field in InputManager");
	}
	void Update()
	{
		//Get whether a click happened.

		bool tapped = false;
		Vector2 tapPos = Vector3.zero;

		if (Input.GetMouseButton(0))
		{
			tapped = true;
			tapPos = (Vector2)MainCam.ScreenToWorldPoint(Input.mousePosition);
		}
		else if (Input.touchCount > 0)
		{
			tapped = true;
			tapPos = (Vector2)MainCam.ScreenToWorldPoint((Vector3)Input.GetTouch(0).position);
		}


		//Respond to the click (or lack thereof).
		if (tapped)
		{
			//Continue the previous frame's click.
			if (IsClicking)
			{
				CurrentClickWorldPos = tapPos;

				//Check whether the mouse is dragging or not.
				if (CurrentlyClicked != null)
				{
					if (IsDragging)
					{
						CurrentlyClicked.RaiseEvent_Drag(StartClickWorldPos, CurrentClickWorldPos);
					}
					else if ((CurrentClickWorldPos - StartClickWorldPos).sqrMagnitude >
							 (CurrentlyClicked.MinDragDistance * CurrentlyClicked.MinDragDistance))
					{
						IsDragging = true;
						CurrentlyClicked.RaiseEvent_Drag(StartClickWorldPos, CurrentClickWorldPos);
					}
				}
			}
			//Start a new click.
			else
			{
				IsClicking = true;
				StartClickWorldPos = tapPos;

				Collider2D coll = Physics2D.OverlapPoint(tapPos);
				if (coll != null)
				{
					InputResponder ir = coll.GetComponent<InputResponder>();
					if (ir != null)
					{
						CurrentlyClicked = ir;
						ir.RaiseEvent_StartClick(StartClickWorldPos);
					}
				}
			}
		}
		else
		{
			//Stop clicking.
			if (IsClicking)
			{
				IsClicking = false;

				if (CurrentlyClicked != null)
				{
					if (IsDragging)
					{
						IsDragging = false;
						CurrentlyClicked.RaiseEvent_StopDrag(StartClickWorldPos, CurrentClickWorldPos);
					}
					else
					{
						CurrentlyClicked.RaiseEvent_StopClick(StartClickWorldPos);
					}

					CurrentlyClicked = null;
				}
			}
		}
	}
}