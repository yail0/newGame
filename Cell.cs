using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour
{

	public UISprite _sprite;
	public UILabel _label;

	//public int _nAnimatingSecond;
	public int _nAnimatingFrame;

	public int _nColor;
	private int _nColorMax;

	private float _fColorDiff;
	public float _fColorMax;
	public float _fColorMin;

	private bool _isColorSet;

	private Vector3 _pressedPosition;
	private Vector3 _originPosition;

	private bool _isSelected;
	
	void OnDrag()
	{
		Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
		Vector3 objPosition = UICamera.mainCamera.ScreenToWorldPoint(mousePosition);
		Vector3 deltaPosition = objPosition - _originPosition;
		Vector3 pressedPositionAxis = _pressedPosition;

		if (deltaPosition.y * deltaPosition.y > deltaPosition.x * deltaPosition.x)
		{
			deltaPosition.x = 0;
			pressedPositionAxis.x = 0;
		}
		else
		{
			deltaPosition.y = 0;
			pressedPositionAxis.y = 0;
		}

		transform.position = _originPosition + deltaPosition + pressedPositionAxis;

	}
	void OnDragEnd()
	{
		transform.position = _originPosition;

	}
	void OnSelect(bool selected)
	{
		if (selected)
		{
			Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
			Vector3 objPosition = UICamera.mainCamera.ScreenToWorldPoint(mousePosition);

			_pressedPosition = transform.position - objPosition;
			_originPosition = transform.position;

			_sprite.depth += 2;
			_label.depth += 2;

		}
		else
		{
			_sprite.depth -= 2;
			_label.depth -= 2;

		}

		/*if (selected)
		{
			if (!_isSelected)
			{
				StartCoroutine(TintAnimation());
			}
		}
		else
		{
			StopCoroutine(TintAnimation());
		}

		_isSelected = selected;*/
	}

	IEnumerator TintAnimation()
	{
		Color delta = new Color(1.0f, 1.0f, 1.0f, 1.0f) - _sprite.color;

		while (true)
		{
			int i = 0;
			while (++i < _nAnimatingFrame)
			{
				_sprite.color += delta / _nAnimatingFrame;

				yield return null;
			}
			i = 0;
			while (++i < _nAnimatingFrame)
			{
				_sprite.color -= delta / _nAnimatingFrame;

				yield return null;
			}
		}
	}

	// Use this for initialization
	void Start()
	{
		_fColorDiff = (_fColorMax - _fColorMin);
	}

	// Update is called once per frame
	void Update()
	{
		//_nAnimatingFrame = (int)(_nAnimatingSecond / Time.deltaTime);

		if (!_isColorSet)
		{
			SetColor(_nColor, LogicSide.instance._nCellColorMax);
		}
	}

	public void SetColor(int colorCurrent, int colorMax)
	{
		_nColor = colorCurrent;

		if (_nColor < 0)
		{
			_sprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			_label.color = _sprite.color;
			_label.text = "";

			return;
		}

		_nColorMax = colorMax;

		if (_nColorMax == 0)
		{
			return;
		}

		float concentration = (float)_nColor / (_nColorMax - 1);
		Color tint = new Color();

		if (concentration < 0.5f)
		{
			float delta = (concentration * 2) * _fColorDiff;

			tint.r = _fColorMax - delta;
			tint.g = _fColorMin + delta;
			tint.b = _fColorMin;
		}
		else
		{
			float delta = ((concentration - 0.5f) * 2) * _fColorDiff;

			tint.g = _fColorMax - delta;
			tint.b = _fColorMin + delta;
			tint.r = _fColorMin;
		}

		tint.a = 1.0f;

		_sprite.color = tint;

		tint.r -= _fColorMin / 2;
		tint.g -= _fColorMin / 2;
		tint.b -= _fColorMin / 2;

		_label.color = tint;
		_label.text = (_nColor + 1).ToString();

		_isColorSet = true;

	}
	public void SetText(string text)
	{
		_label.text = text;
	}
	public void SetSize(float scale)
	{
		_sprite.transform.localScale = new Vector3(scale, scale, 1);
		_label.fontSize = (int)(50 * scale);

	}

}
