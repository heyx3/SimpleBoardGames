using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AnchorToCamEdges : MonoBehaviour
{
	public ScaleCameraToFit CameraView;

	private Vector2i sideID = new Vector2i();
	private Vector2 sideOffset = Vector2.zero;
	private Transform tr;


	private void Awake()
	{
		tr = transform;

		Vector2 posT = new Vector2(Mathf.InverseLerp(CameraView.RegionToFit.xMin, CameraView.RegionToFit.xMax,
					  								 tr.position.x),
								   Mathf.InverseLerp(CameraView.RegionToFit.yMin, CameraView.RegionToFit.yMax,
													 tr.position.y));
		if (posT.x > 0.5f)
		{
			sideID.x = 1;
			sideOffset.x = tr.position.x - CameraView.RegionToFit.xMax;
		}
		else
		{
			sideID.x = -1;
			sideOffset.x = tr.position.x - CameraView.RegionToFit.xMin;
		}
		if (posT.y > 0.5f)
		{
			sideID.y = 1;
			sideOffset.y = tr.position.y - CameraView.RegionToFit.yMax;
		}
		else
		{
			sideID.y = -1;
			sideOffset.y = tr.position.y - CameraView.RegionToFit.yMin;
		}
	}
	private void LateUpdate()
	{
		Vector2 newPos = new Vector2(sideID.x == 1 ?
										 CameraView.RegionToFit.xMax + sideOffset.x :
										 CameraView.RegionToFit.xMin + sideOffset.x,
									 sideID.y == 1 ?
										 CameraView.RegionToFit.yMax + sideOffset.y :
										 CameraView.RegionToFit.yMin + sideOffset.y);
		tr.position = new Vector3(newPos.x, newPos.y, tr.position.z);
	}
}