using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {

	public int _nColor = 0;
	public UISprite _sprite;
	private int _colorMax = 6;

	private float _fColorDiff;
	private float _fColorMax;
	private float _fColorMin;

	// Use this for initialization
	void Start () {
		_fColorDiff = _sprite.gradientTop.r - _sprite.gradientTop.g;
		_fColorMax = _sprite.gradientTop.r;
		_fColorMin = _sprite.gradientTop.g;
	}
	
	// Update is called once per frame
	void Update ()
	{
		_colorMax = LogicSide.instance._nCellColorMax;

		float concentration = (float)_nColor / _colorMax;
		Color color = new Color();

		if (concentration < 0.5f)
		{
			float delta = (concentration * 2) * _fColorDiff;

			color.r = _fColorMax - delta;
			color.g = _fColorMin + delta;
			color.b = _fColorMin;
			color.a = 1.0f;

			_sprite.color = color;
		}
		else
		{
			float delta = ((concentration - 0.5f) * 2) * _fColorDiff;

			color.g = _fColorMax - delta;
			color.b = _fColorMin + delta;
			color.r = _fColorMin;
			color.a = 1.0f;

			_sprite.color = color;
		}

	}
}
