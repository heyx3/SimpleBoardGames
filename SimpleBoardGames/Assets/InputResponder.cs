using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Responds to tap/click inputs.
/// </summary>
public class InputResponder : MonoBehaviour
{
	public Transform MyTr { get; private set; }
	public Collider2D MyCollider { get; private set; }


	/// <summary>
	/// The minimum distance in world units
	/// before the mouse is considered to be dragging this item.
	/// </summary>
	public float MinDragDistance = 0.5f;


	public UnityEngine.Events.UnityEvent Unity_OnStartClick =
				new UnityEngine.Events.UnityEvent();
	public UnityEngine.Events.UnityEvent Unity_OnStopClick =
				new UnityEngine.Events.UnityEvent();

	/// <summary>
	/// Called when this item is first clicked on.
	/// </summary>
	public event Action<InputResponder, Vector2> OnStartClick;
	/// <summary>
	/// Called every frame as this item gets dragged around.
	/// Only happens once the mouse moves more than "MinDragDistance".
	/// The first Vector2 is the previous mouse position.
	/// The second Vector2 is the current mouse position.
	/// </summary>
	public event Action<InputResponder, Vector2, Vector2> OnDrag;

	/// <summary>
	/// Called when this item just stopped being clicked on.
	/// Only gets called if the mouse *wasn't* dragging.
	/// </summary>
	public event Action<InputResponder, Vector2> OnStopClick;
	/// <summary>
	/// Called when this item stopped being dragged around.
	/// The first Vector2 is the original mouse position.
	/// The second Vector2 is the final mouse position.
	/// </summary>
	public event Action<InputResponder, Vector2, Vector2> OnStopDrag;


	public void RaiseEvent_StartClick(Vector2 mPos)
	{
		if (OnStartClick != null)
			OnStartClick(this, mPos);
		Unity_OnStartClick.Invoke();
	}
	public void RaiseEvent_Drag(Vector2 lastMPos, Vector2 currentMPos)
	{
		if (OnDrag != null)
			OnDrag(this, lastMPos, currentMPos);
	}
	public void RaiseEvent_StopClick(Vector2 mPos)
	{
		if (OnStopClick != null)
			OnStopClick(this, mPos);
		Unity_OnStopClick.Invoke();
	}
	public void RaiseEvent_StopDrag(Vector2 originalMPos, Vector2 finalMPos)
	{
		if (OnStopDrag != null)
			OnStopDrag(this, originalMPos, finalMPos);
	}

	protected virtual void Awake()
	{
		MyCollider = GetComponent<Collider2D>();
		MyTr = transform;

		if (MyCollider == null)
			Debug.LogError("InputResponder \"" + gameObject.name + "\" doesn't have a Collider2D");
	}
}