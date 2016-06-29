using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Curves
{
	[CustomEditor(typeof(UnityCurve))]
	public class UnityCurveEditor : Editor
	{
		private static GUIContent[] curveTypes =
		{
			new GUIContent("Empty"),
			new GUIContent("Constant"),
			new GUIContent("Linear"),
			new GUIContent("Cubic")
		};
		private static int GUIPopup_NNodes(int currentNNodes)
		{
			int[] nNodesByCurveType = { 1, 2, 4 };
			int[] curveTypeIndexByNNodes = { -1, 0, 1, -1, 2 };

			return nNodesByCurveType[EditorGUILayout.Popup(curveTypeIndexByNNodes[currentNNodes],
														   curveTypes)];
		}


		private static bool PointInspectorGUI(Curve<Vector3, Ops_Lerp> curve,
											  int node, bool isSelected, bool isControlPoint)
		{
			if (isControlPoint)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(15.0f);
			}

			if (isSelected)
			{
				curve[node] = EditorGUILayout.Vector3Field("Value", curve[node]);
			}
			else
			{
				if (GUILayout.Button("Select"))
					return true;
			}

			if (isControlPoint)
			{
				GUILayout.EndHorizontal();
			}

			return isSelected;
		}
		/// <summary>
		/// Returns which curve node should be selected, or -1 if nothing was selected.
		/// </summary>
		/// <param name="currentlySelected">
		/// The currently-selected node for the given curve, or -1 if no node was selected.
		/// </param>
		/// <param name="editEndNode">
		/// Whether to expose the end node of the curve for editing.
		/// Disabling this is useful for MultiCurve editing.
		/// </param>
		public static int RunEditorGUI(Curve<Vector3, Ops_Lerp> curve, int currentlySelected, bool editEndNode)
		{
			int newNNodes;
			switch (curve.NNodes)
			{
				case 1:
					if (editEndNode)
						if (PointInspectorGUI(curve, 0, (currentlySelected == 0), false))
							currentlySelected = 0;
					newNNodes = GUIPopup_NNodes(1);
					switch (newNNodes)
					{
						case 2:
							curve.Reset(curve[0], curve[0] + new Vector3(1.0f, 0.0f, 0.0f));
							break;
						case 4:
							Vector3 nextPos = curve[0] + new Vector3(1.0f, 0.0f, 0.0f);
							curve.Reset(curve[0], nextPos, nextPos, curve[0]);
							break;

						case 1: break;
						default: throw new NotImplementedException("Unexpected number of nodes: " + newNNodes);
					}
					break;

				case 2:
					if (PointInspectorGUI(curve, 0, (currentlySelected == 0), false))
						currentlySelected = 0;
					if (editEndNode)
						if (PointInspectorGUI(curve, 1, (currentlySelected == 1), false))
							currentlySelected = 1;
					newNNodes = GUIPopup_NNodes(2);
					switch (newNNodes)
					{
						case 1:
							curve.Reset(curve[0]);
							break;
						case 4:
							Vector3 midpoint = (curve[0] + curve[1]) * 0.5f;
							curve.Reset(curve[0], midpoint, curve[1], midpoint);
							break;

						case 2: break;
						default: throw new NotImplementedException("Unexpected number of nodes: " + newNNodes);
					}
					break;

				case 4:
					if (PointInspectorGUI(curve, 0, (currentlySelected == 0), false))
						currentlySelected = 0;
					if (PointInspectorGUI(curve, 1, (currentlySelected == 1), true))
						currentlySelected = 1;
					if (PointInspectorGUI(curve, 2, (currentlySelected == 2), true))
						currentlySelected = 2;
					if (editEndNode)
						if (PointInspectorGUI(curve, 3, (currentlySelected == 3), false))
							currentlySelected = 3;
					newNNodes = GUIPopup_NNodes(4);
					switch (newNNodes)
					{
						case 1:
							curve.Reset(curve[0]);
							break;
						case 2:
							curve.Reset(curve[0], curve[3]);
							break;

						case 4: break;
						default: throw new NotImplementedException("Unexpected number of nodes: " + newNNodes);
					}
					break;

				default:
					throw new NotImplementedException("Unexpected number of nodes: " + curve.NNodes);
			}

			return currentlySelected;
		}


		int selectedNode = -1;


		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			UnityCurve uc = (UnityCurve)target;

			selectedNode = RunEditorGUI(uc.Curve, selectedNode, true);
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

			if (selectedNode != -1)
			{
				UnityCurve uc = (UnityCurve)target;
				Vector3 currentPos = uc.Curve[selectedNode];
				Quaternion handleRot = (Tools.pivotRotation == PivotRotation.Global ?
											Quaternion.identity :
											uc.transform.rotation);
				uc.Curve[selectedNode] = Handles.PositionHandle(currentPos, handleRot);
			}

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "Move curve point");
			}
		}
	}
}