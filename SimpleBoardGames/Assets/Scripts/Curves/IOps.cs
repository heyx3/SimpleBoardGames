using System;
using System.Collections;
using UnityEngine;


namespace Curves
{
	public interface IOps<V>
	{
		V Interpolate(V start, V end, float t);
		
		V Add(V first, V second);
		V Subtract(V first, V second);

		V Multiply(V first, float f);
	}

	public abstract class Ops_Basic : IOps<float>, IOps<Vector2>, IOps<Vector3>
	{
		public abstract float Interpolate(float start, float end, float t);
		public abstract Vector2 Interpolate(Vector2 start, Vector2 end, float t);
		public abstract Vector3 Interpolate(Vector3 start, Vector3 end, float t);

		public float Add(float first, float second) { return first + second; }
		public Vector2 Add(Vector2 first, Vector2 second) { return first + second; }
		public Vector3 Add(Vector3 first, Vector3 second) { return first + second; }
		
		public float Subtract(float first, float second) { return first - second; }
		public Vector2 Subtract(Vector2 first, Vector2 second) { return first - second; }
		public Vector3 Subtract(Vector3 first, Vector3 second) { return first - second; }
		
		public float Multiply(float first, float f) { return first * f; }
		public Vector2 Multiply(Vector2 first, float f) { return new Vector2(first.x * f, first.y * f); }
		public Vector3 Multiply(Vector3 first, float f) { return new Vector3(first.x * f, first.y * f, first.z * f); }
	}
	public class Ops_Lerp : Ops_Basic
	{
		public override float Interpolate(float start, float end, float t)
		{
			return Mathf.Lerp(start, end, t);
		}
		public override Vector2 Interpolate(Vector2 start, Vector2 end, float t)
		{
			return Vector2.Lerp(start, end, t);
		}
		public override Vector3 Interpolate(Vector3 start, Vector3 end, float t)
		{
			return Vector3.Lerp(start, end, t);
		}

		public Ops_Lerp() { }
	}
	public class Ops_Smoothstep : Ops_Basic
	{
		public override float Interpolate(float start, float end, float t)
		{
			return Mathf.SmoothStep(start, end, t);
		}
		public override Vector2 Interpolate(Vector2 start, Vector2 end, float t)
		{
			return Vector2.Lerp(start, end, Mathf.SmoothStep(0.0f, 1.0f, t));
		}
		public override Vector3 Interpolate(Vector3 start, Vector3 end, float t)
		{
			return Vector3.Lerp(start, end, Mathf.SmoothStep(0.0f, 1.0f, t));
		}

		public Ops_Smoothstep() { }
	}
}