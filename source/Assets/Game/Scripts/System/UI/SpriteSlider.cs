using System;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteSlider:MonoBehaviour
{
	public bool Vertical = false;

	float _value;
	Rect _originalRect;
	Texture2D _originalTexture;

	public float sliderValue
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;

				if (_originalTexture != null)
				{
					SpriteRenderer r = renderer as SpriteRenderer;
					if (Vertical)
					{
						r.sharedMaterial.SetFloat("_CutStart", _originalRect.y / _originalTexture.height);
						r.sharedMaterial.SetFloat("_CutEnd", (_originalRect.y + _originalRect.height * _value) / _originalTexture.height);
					}
					else
					{
						r.sharedMaterial.SetFloat("_CutStart", _originalRect.x / _originalTexture.width);
						r.sharedMaterial.SetFloat("_CutEnd", (_originalRect.x + _originalRect.width * _value) / _originalTexture.width);
					}
				}
			}
		}
	}

	void Awake()
	{
		SpriteRenderer r = renderer as SpriteRenderer;
		_originalRect = r.sprite.textureRect;
		_originalTexture = r.sprite.texture;
		r.sharedMaterial = new Material(r.sharedMaterial);
		float value = _value;
		_value = -1;
		sliderValue = value; 
	}
}
