using System;
using System.Collections.Generic;
using UnityEngine;


namespace BoardGames
{
	/// <summary>
	/// Responds to tap/click inputs.
	/// </summary>
	[RequireComponent(typeof(Collider2D))]
	public abstract class InputResponder : MonoBehaviour
	{
		public Transform MyTr { get; private set; }
		public Collider2D MyCollider { get; private set; }


		/// <summary>
		/// The minimum distance in world units
		/// before the mouse is considered to be dragging this item.
		/// </summary>
		public float MinDragDistance = 0.1f;


		/// <summary>
		/// Called when this item is first clicked on.
		/// </summary>
		public abstract void OnStartClick(Vector2 mouseWorldPos);

		/// <summary>
		/// Called every frame as this item gets dragged around.
		/// Only happens once the mouse moves more than "MinDragDistance".
		/// </summary>
		public abstract void OnDrag(Vector2 startMouseWorldPos,
									Vector2 currentMouseWorldPos);
		
		/// <summary>
		/// Called when this item just stopped being clicked on.
		/// Only gets called if the mouse *wasn't* dragging.
		/// </summary>
		public abstract void OnStopClick(Vector2 mouseWorldPos);
		/// <summary>
		/// Called when this item stopped being dragged around.
		/// </summary>
		public abstract void OnStopDrag(Vector2 startMouseWorldPos,
										Vector2 endMouseWorldPos);


		protected virtual void Awake()
		{
			MyCollider = GetComponent<Collider2D>();
			MyTr = transform;
		}
	}
}