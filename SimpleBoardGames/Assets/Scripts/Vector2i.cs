using System;


public struct Vector2i
{
	public int x, y;


	public Vector2i(int _x, int _y) { x = _x; y = _y; }
	public Vector2i(UnityEngine.Vector2 v) { x = (int)v.x; y = (int)v.y; }


	public static bool operator==(Vector2i lhs, Vector2i rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}
	public static bool operator!=(Vector2i lhs, Vector2i rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}
	
	public static Vector2i operator+(Vector2i lhs, Vector2i rhs)
	{
		return new Vector2i(lhs.x + rhs.x, lhs.y + rhs.y);
	}
	public static Vector2i operator-(Vector2i lhs, Vector2i rhs)
	{
		return new Vector2i(lhs.x - rhs.x, lhs.y - rhs.y);
	}
	public static Vector2i operator*(Vector2i lhs, int rhs)
	{
		return new Vector2i(lhs.x * rhs, lhs.y * rhs);
	}
	public static Vector2i operator/(Vector2i lhs, int rhs)
	{
		return new Vector2i(lhs.x / rhs, lhs.y / rhs);
	}

	
	public Vector2i LessX { get { return new Vector2i(x - 1, y); } }
	public Vector2i LessY { get { return new Vector2i(x, y - 1); } }
	public Vector2i MoreX { get { return new Vector2i(x + 1, y); } }
	public Vector2i MoreY { get { return new Vector2i(x, y + 1); } }

	public float Distance(Vector2i other)
	{
		return UnityEngine.Mathf.Sqrt((float)DistanceSqr(other));
	}
	public int DistanceSqr(Vector2i other)
	{
		int x2 = x - other.x,
			y2 = y - other.y;
		return (x2 * x2) + (y2 * y2);
	}


	public override string ToString()
	{
		return "{" + x + ", " + y + "}";
	}
	public override int GetHashCode()
	{
		return (x * 73856093) ^ (y * 19349663);
	}
	public int GetHashCode(int z)
	{
		return (this * z).GetHashCode();
	}
	public override bool Equals(object obj)
	{
		return (obj is Vector2i) && ((Vector2i)obj) == this;
	}
}