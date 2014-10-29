public struct Vector2i
{
	public int x;
	public int y;

	public static Vector2i[] Directions = new Vector2i[4]
	{
		new Vector2i(1,0),
		new Vector2i(0,1),
		new Vector2i(-1,0),
		new Vector2i(0,-1)
	};

	public Vector2i (int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public override bool Equals (object other)
	{
		if (!(other is Vector2i))
		{
			return false;
		}
		Vector2i vector = (Vector2i)other;
		return x == vector.x && y == vector.y;
	}
	
	public override int GetHashCode ()
	{
		return x.GetHashCode () ^ y.GetHashCode () << 2;
	}

	public override string ToString ()
	{
		return x.ToString() + ", " + y;
	}

	public int MagnitudeSqr() { return x * x + y * y; }

	public static Vector2i operator + (Vector2i a, Vector2i b)
	{
		return new Vector2i (a.x + b.x, a.y + b.y);
	}

	public static Vector2i operator / (Vector2i a, int d)
	{
		return new Vector2i (a.x / d, a.y / d);
	}

	public static bool operator == (Vector2i lhs, Vector2i rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;;
	}

	public static bool operator != (Vector2i lhs, Vector2i rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}

	public static Vector2i operator * (int d, Vector2i a)
	{
		return new Vector2i (a.x * d, a.y * d);
	}
	
	public static Vector2i operator * (Vector2i a, int d)
	{
		return new Vector2i (a.x * d, a.y * d);
	}
	
	public static Vector2i operator * (float d, Vector2i a)
	{
		return new Vector2i ((int)(a.x * d), (int)(a.y * d));
	}
	
	public static Vector2i operator * (Vector2i a, float d)
	{
		return new Vector2i ((int)(a.x * d), (int)(a.y * d));
	}
	
	public static Vector2i operator - (Vector2i a, Vector2i b)
	{
		return new Vector2i (a.x - b.x, a.y - b.y);
	}
	
	public static Vector2i operator - (Vector2i a)
	{
		return new Vector2i (-a.x, -a.y);
	}
}
