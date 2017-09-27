using System;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class ScaleCameraToFit : MonoBehaviour
{
	public Rect RegionToFit = new Rect(0.0f, 0.0f, 100.0f, 100.0f);

	public Camera Cam { get; private set; }
	public Transform Tr { get; private set; }


	private void Awake()
	{
		Cam = GetComponent<Camera>();
		Tr = transform;
	}
	private void LateUpdate()
	{
		Rect regionToFit = RegionToFit;

		Tr.position = RegionToFit.center;

		//Choose the vertical scale to cover the whole region.
		float orthoHeight = regionToFit.height;
		float orthoWidth = orthoHeight * Cam.aspect;
		//If the horizontal region isn't fully covered, scale up further.
		if (orthoWidth < regionToFit.width)
		{
			float scale = regionToFit.width / orthoWidth;
			orthoHeight *= scale;
		}

		Cam.orthographicSize = orthoHeight * 0.5f;
	}
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(RegionToFit.center, RegionToFit.size);
	}
}