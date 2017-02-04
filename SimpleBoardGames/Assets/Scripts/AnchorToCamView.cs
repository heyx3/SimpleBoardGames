using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AnchorToCamView : MonoBehaviour
{
	public ScaleCameraToFit CameraView;

	private Transform tr;
	private Vector2 posT;


	private void Awake()
	{
		tr = transform;

		posT = new Vector2(Mathf.InverseLerp(CameraView.RegionToFit.xMin, CameraView.RegionToFit.xMax,
											 tr.position.x),
						   Mathf.InverseLerp(CameraView.RegionToFit.yMin, CameraView.RegionToFit.yMax,
											 tr.position.y));
	}
	private void LateUpdate()
	{
		tr.position = new Vector3(Mathf.Lerp(CameraView.RegionToFit.xMin, CameraView.RegionToFit.xMax,
											 posT.x),
								  Mathf.Lerp(CameraView.RegionToFit.yMin, CameraView.RegionToFit.yMax,
											 posT.y),
								  tr.position.z);
	}
}