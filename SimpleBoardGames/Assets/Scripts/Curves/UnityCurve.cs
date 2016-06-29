using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Curves
{
	public class UnityCurve : MonoBehaviour
	{
		[Serializable]
		public class GizmoSettings
		{
			public Color Col = Color.red;
			public float Radius = 0.15f;
			public int Segments = 10;
			public float DerivativeLength = 0.0f;
			public bool Enabled = true;

			public void DrawGizmos(Curve<Vector3, Ops_Lerp> c, bool drawEnd)
			{
				if (!Enabled)
					return;

				Gizmos.color = Col;
				for (int i = 0; i < c.NNodes; ++i)
					if (i < c.NNodes - 1 || drawEnd)
						Gizmos.DrawSphere(c[i], Radius);

				for (int i = 0; i < c.NNodes - 1; ++i)
				{
					float prevT = 0.0f;
					Vector3 prevVal = c.GetVal(prevT);
					for (int j = 1; j < Segments; ++j)
					{
						float t = j / (Segments - 1);
						Vector3 val = c.GetVal(t);

						Gizmos.DrawLine(prevVal, val);
						if (DerivativeLength != 0.0f)
							Gizmos.DrawLine(val, val + (c.GetDerivative(t) * DerivativeLength));

						prevT = t;
						prevVal = val;
					}
				}
			}
#if UNITY_EDITOR
			public void GUIGizmos()
			{
				Enabled = EditorGUILayout.Foldout(Enabled, "Curve Visualization");
				if (Enabled)
				{
					Col = EditorGUILayout.ColorField(Col);
					Radius = EditorGUILayout.FloatField("Node Radius:", Radius);
					Segments = EditorGUILayout.IntSlider("Segments:", Segments, 2, 50);
					DerivativeLength = EditorGUILayout.Slider("Derivative Length:", DerivativeLength, 0.0f, 10.0f);
				}
			}
#endif
		}


		public Curve<Vector3, Ops_Lerp> Curve;

		public GizmoSettings GizmoDrawSettings;


		void OnDrawGizmos()
		{
			GizmoDrawSettings.DrawGizmos(Curve, true);
		}
	}
}