using UnityEngine;

/// <summary>
/// Tween the object's color.
/// </summary>

public class ColorTween : Tweener
{
	public Color from = Color.white;
	public Color to = Color.white;

	SpriteRenderer mSprite;
	Material mMat;

	/// <summary>
	/// Current color.
	/// </summary>

	public Color color
	{
		get
		{
			if (mSprite != null) return mSprite.color;
			if (mMat != null) return mMat.color;
			return Color.black;
		}
		set
		{
			if (mMat != null) mMat.color = value;

			if (mSprite != null)
				mSprite.color = value;
		}
	}

	/// <summary>
	/// Find all needed components.
	/// </summary>

	void Awake ()
	{
		Renderer ren = renderer;
		if (ren != null) 
		{
			if (ren.material.HasProperty("_Color"))
				mMat = ren.material;

			SpriteRenderer sRen = ren as SpriteRenderer;
			if (sRen != null)
				mSprite = sRen;
		}
	}

	/// <summary>
	/// Interpolate and update the color.
	/// </summary>

	protected override void OnUpdate(float factor, bool isFinished) { color = Color.Lerp(from, to, factor); }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public ColorTween Begin (GameObject go, float duration, Color color)
	{
		ColorTween comp = Tweener.Begin<ColorTween>(go, duration);
		comp.from = comp.color;
		comp.to = color;

		if (duration <= 0f)
		{
			comp.color = color;
			comp.enabled = false;
		}
		return comp;
	}
}
