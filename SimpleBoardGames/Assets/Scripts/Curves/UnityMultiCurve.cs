using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Curves
{
	public class UnityMultiCurve : MonoBehaviour
	{
		public List<Curve<Vector3, Ops_Lerp>> Curves;

		public bool DrawGizmos = true;
		public UnityCurve.GizmoSettings GizmoDrawSettings;


		void OnDrawGizmos()
		{
			foreach (Curve<Vector3, Ops_Lerp> c in Curves)
				GizmoDrawSettings.DrawGizmos(c, false);
		}
	}
}