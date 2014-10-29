using System;

public class DebugUtils
{
	public static void Assert(bool condition)
	{
#if DEBUG
		if (!condition) 
			throw new Exception("Assert!");
#endif
	}
}
