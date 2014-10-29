using UnityEngine;

public class InitStruct
{
	public int levelIndex { get; private set; }
	public bool propSwap { get; private set; }
	public bool propTopFall { get; private set; }

	public InitStruct(int levelIndex, bool propSwap, bool propTopFall)
	{
		this.levelIndex = levelIndex;
		this.propSwap = propSwap;
		this.propTopFall = propTopFall;
	}
}
