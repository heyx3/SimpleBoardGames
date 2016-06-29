using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Curves
{
	[CustomEditor(typeof(UnityMultiCurve))]
	public class UnityMultiCurveEditor : Editor
	{
		int selectedCurve = -1;
		int selectedNode = -1;


		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			UnityMultiCurve uc = (UnityMultiCurve)target;

			for (int i = 0; i < uc.Curves.Count; ++i)
			{
				int sel = UnityCurveEditor.RunEditorGUI(uc.Curves[i],
														(selectedCurve == i ? selectedNode : -1),
														false);
				if (sel > -1)
				{
					selectedCurve = i;
					selectedNode = sel;
				}
			}
			if (uc.Curves.Count > 1 && GUILayout.Button("Remove last curve"))
			{
				uc.Curves.RemoveAt(uc.Curves.Count - 1);
			}


			GUILayout.Space(25.0f);

			uc.GizmoDrawSettings.GUIGizmos();

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "Edit " + uc.gameObject.name);
			}
		}
		void OnSceneGUI()
		{
			EditorGUI.BeginChangeCheck();

			if (selectedCurve > -1 && selectedNode > -1)
			{
				UnityMultiCurve uc = (UnityMultiCurve)target;

				Vector3 currentPos = uc.Curves[selectedCurve][selectedNode];
				Quaternion handleRot = (Tools.pivotRotation == PivotRotation.Global ?
											Quaternion.identity :
											uc.transform.rotation);
				uc.Curves[selectedCurve][selectedNode] = Handles.PositionHandle(currentPos, handleRot);
			}

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "Move curve point");
			}
		}
	}
}