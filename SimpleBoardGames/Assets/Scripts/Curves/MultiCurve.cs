using System;
using System.Collections.Generic;
using UnityEngine;


namespace Curves
{
	/// <summary>
	/// Combines multiple curves together.
	/// Note that each curve's endpoint is always set to the beginning point of the next curve.
	/// </summary>
	public class MultiCurve<V, Ops>
		where Ops : IOps<V>, new()
	{
		public struct CurveElement
		{
			public Curve<V, Ops> Curve;
			public float EndT;

			public CurveElement(Curve<V, Ops> curve, float endT)
			{
				Curve = curve;
				EndT = endT;
			}
		}

		public List<CurveElement> Curves;


		public MultiCurve(params CurveElement[] curves) { Curves = new List<CurveElement>(curves); }


		public void AddCurve(Curve<V, Ops> curve, float endT)
		{
			Curves.Add(new CurveElement(curve, endT));
			
			if (Curves.Count > 1)
				UpdateCurve(Curves.Count - 2);
		}

		public Curve<V, Ops> GetCurve(float t, out float curveT)
		{
			if (Curves.Count == 0)
			{
				curveT = float.NaN;
				return null;
			}

			int index = 0;
			while (index < Curves.Count && Curves[index].EndT <= t)
				index += 1;

			if (index >= Curves.Count)
			{
				curveT = 1.0f;
				return Curves[Curves.Count - 1].Curve;
			}

			UpdateCurve(index - 1);
			UpdateCurve(index);
			curveT = Mathf.InverseLerp((index > 0 ? Curves[index - 1].EndT : 0.0f),
									   Curves[index].EndT,
									   t);
			return Curves[index].Curve;
		}

		public V GetValue(float t)
		{
			float curveT;
			Curve<V, Ops> c = GetCurve(t, out curveT);
			if (c == null)
				return default(V);
			return c.GetVal(curveT);
		}
		public V GetDerivative(float t)
		{
			float curveT;
			Curve<V, Ops> c = GetCurve(t, out curveT);
			if (c == null)
				return default(V);
			return c.GetDerivative(curveT);
		}

		private void UpdateCurve(int i)
		{
			if (Curves.Count > (i + 1))
			{
				Curves[i].Curve[Curves[i].Curve.NNodes - 1] = Curves[i + 1].Curve[0];
			}
		}
	}
}