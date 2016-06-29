using System;
using System.Collections.Generic;
using UnityEngine;


namespace Curves
{
	[Serializable]
	public class Curve<V, Ops>
		where Ops : IOps<V>, new()
	{
		private static Ops ops = new Ops();


		[SerializeField]
		private V[] nodes;

		public int NNodes { get { return nodes.Length; } }


		public Curve(V constantVal = default(V))
		{
			nodes = new V[1] { constantVal };
		}
		public Curve(V start, V end)
		{
			nodes = new V[2] { start, end };
		}
		public Curve(V start, V startControlPoint,
					 V end, V endControlPoint)
		{
			nodes = new V[4] { start, startControlPoint, endControlPoint, end };
		}


		public V this[int i]
		{
			get { return nodes[i]; }
			set { nodes[i] = value; }
		}

		public V GetVal(float t)
		{
			switch (nodes.Length)
			{
				case 0:
					return default(V);
				case 1:
					return nodes[0];

				case 2:
					return ops.Interpolate(nodes[0], nodes[1], t);

				case 4:
					float tSqr = t * t,
						  oneMt = 1.0f - t,
						  oneMtSqr = oneMt * oneMt;
					return ops.Add(ops.Multiply(nodes[0], oneMt * oneMtSqr),
								   ops.Add(ops.Multiply(nodes[1], 3.0f * oneMtSqr * t),
										   ops.Add(ops.Multiply(nodes[2], 3.0f * oneMt * tSqr),
												   ops.Multiply(nodes[3], tSqr * t))));

				default:
					throw new NotImplementedException("Unexpected " + nodes.Length + " nodes!");
			}
		}
		public V GetDerivative(float t)
		{
			switch (nodes.Length)
			{
				case 0:
				case 1:
					return default(V);
				
				case 2:
					return ops.Subtract(nodes[1], nodes[0]);

				case 4:
					float oneMt = 1.0f - t;
					return ops.Add(ops.Multiply(ops.Subtract(nodes[1], nodes[0]), 3.0f * oneMt * oneMt),
								   ops.Add(ops.Multiply(ops.Subtract(nodes[2], nodes[1]), 6.0f * oneMt * t),
										   ops.Multiply(ops.Subtract(nodes[3], nodes[2]), 3.0f * t * t)));

				default:
					throw new NotImplementedException("Unexpected " + nodes.Length + " nodes!");
			}
		}
		
		public void Reset(V constantVal) { nodes = new V[] { constantVal }; }
		public void Reset(V start, V end) { nodes = new V[] { start, end }; }
		public void Reset(V start, V startControlPoint,
						  V end, V endControlPoint)
		{
			nodes = new V[] { start, startControlPoint, endControlPoint, end };
		}
	}
}