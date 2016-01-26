using System;


public struct Vector3i
{
	public int x, y, z;


	public Vector3i(int _x, int _y, int _z) { x = _x; y = _y; z = _z; }
	public Vector3i(UnityEngine.Vector3 v) { x = (int)v.x; y = (int)v.y; z = (int)v.z; }


	public static bool operator==(Vector3i lhs, Vector3i rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
	}
	public static bool operator!=(Vector3i lhs, Vector3i rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
	}
	
	public static Vector3i operator+(Vector3i lhs, Vector3i rhs)
	{
		return new Vector3i(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
	}
	public static Vector3i operator-(Vector3i lhs, Vector3i rhs)
	{
		return new Vector3i(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
	}
	public static Vector3i operator*(Vector3i lhs, int rhs)
	{
		return new Vector3i(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
	}
	public static Vector3i operator/(Vector3i lhs, int rhs)
	{
		return new Vector3i(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
	}

	
	public Vector3i LessX { get { return new Vector3i(x - 1, y, z); } }
	public Vector3i LessY { get { return new Vector3i(x, y - 1, z); } }
	public Vector3i LessZ { get { return new Vector3i(x, y, z - 1); } }
	public Vector3i MoreX { get { return new Vector3i(x + 1, y, z); } }
	public Vector3i MoreY { get { return new Vector3i(x, y + 1, z); } }
	public Vector3i MoreZ { get { return new Vector3i(x, y, z + 1); } }

	public float Distance(Vector3i other)
	{
		return UnityEngine.Mathf.Sqrt((float)DistanceSqr(other));
	}
	public int DistanceSqr(Vector3i other)
	{
		int x2 = x - other.x,
			y2 = y - other.y,
			z2 = z - other.z;
		return (x2 * x2) + (y2 * y2) + (z2 * z2);
	}


	public override string ToString()
	{
		return "{" + x + ", " + y + ", " + z + "}";
	}
	public override int GetHashCode()
	{
		return (x * 73856093) ^ (y * 19349663) ^ (z * 15485863);
	}
	public int GetHashCode(int w)
	{
		return (this * w).GetHashCode();
	}
	public override bool Equals(object obj)
	{
		return (obj is Vector3i) && ((Vector3i)obj) == this;
	}
}