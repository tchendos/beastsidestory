using System;
using UnityEngine;

public class ShadowLabel:MonoBehaviour
{
	public string text { get { return _label.text; } set { _label.text = value; _shadowLabel.text = value; } }
	public Color color { get { return _label.color; } set { _label.color = value; } }

	TextMesh _label;
	TextMesh _shadowLabel;
	void Awake()
	{
		_label = GetComponent<TextMesh>();

		Transform shadowT = gameObject.transform.FindChild("ShadowLabel");
		if (shadowT == null)
		{
			GameObject go = new GameObject();
			go.name = "ShadowLabel";
			go.transform.parent = _label.transform;
			go.transform.position = _label.transform.position + new Vector3(1, -1, 0);
			go.transform.localScale = Vector3.one;

			_shadowLabel = go.AddComponent<TextMesh>();
			_shadowLabel.alignment = _label.alignment;
			_shadowLabel.anchor = _label.anchor;
			_shadowLabel.color = Color.black;
			_shadowLabel.font = _label.font;
			_shadowLabel.fontSize = _label.fontSize;
			_shadowLabel.fontStyle = _label.fontStyle;
			_shadowLabel.characterSize = _label.characterSize;
			_shadowLabel.lineSpacing = _label.lineSpacing;
			_shadowLabel.richText = false;
			_shadowLabel.tabSize = _label.tabSize;
			_shadowLabel.text = _label.text;

			_shadowLabel.renderer.sortingLayerID = _label.renderer.sortingLayerID;
			_shadowLabel.renderer.sortingOrder = _label.renderer.sortingOrder - 1;

			_shadowLabel.renderer.material = _label.renderer.material;
		}
		else _shadowLabel = shadowT.GetComponent<TextMesh>();
	}
}
