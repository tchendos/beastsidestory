public struct Recti
{
	public Vector2i lt;
	public Vector2i rb;

	public int width { get { return System.Math.Abs(rb.x - lt.x); } }
	public int height { get { return System.Math.Abs(rb.y - lt.y); } }

	public Recti (int x1, int y1, int x2, int y2)
	{
		lt.x = x1;
		lt.y = y1;
		rb.x = x2;
		rb.y = y2;
	}

	public override bool Equals (object other)
	{
		if (!(other is Recti))
		{
			return false;
		}
		Recti vector = (Recti)other;
		return lt == vector.lt && rb == vector.rb;
	}
	
	public override int GetHashCode ()
	{
		return lt.GetHashCode () ^ rb.GetHashCode () << 2;
	}

	public override string ToString ()
	{
		return lt.ToString() + " / " + rb;
	}

	public static bool operator == (Recti lhs, Recti rhs)
	{
		return lhs.lt == rhs.lt && lhs.rb == rhs.rb;
	}

	public static bool operator != (Recti lhs, Recti rhs)
	{
		return lhs.lt != rhs.lt || lhs.rb != rhs.rb;
	}
}
