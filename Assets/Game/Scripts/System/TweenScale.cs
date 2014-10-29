using UnityEngine;

/// <summary>
/// Tween the object's local scale.
/// </summary>

public class ScaleTween : Tweener
{
	public Vector3 from = Vector3.one;
	public Vector3 to = Vector3.one;
	public bool updateTable = false;

	Transform mTrans;

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	public Vector3 scale { get { return cachedTransform.localScale; } set { cachedTransform.localScale = value; } }

	protected override void OnUpdate (float factor, bool isFinished)
	{
		cachedTransform.localScale = from * (1f - factor) + to * factor;
	}

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public ScaleTween Begin (GameObject go, float duration, Vector3 scale)
	{
		ScaleTween comp = Tweener.Begin<ScaleTween>(go, duration);
		comp.from = comp.scale;
		comp.to = scale;

		if (duration <= 0f)
		{
			comp.scale = scale;
			comp.enabled = false;
		}
		return comp;
	}
}
