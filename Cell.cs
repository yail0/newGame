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

	private Vector3 _pressedPosition;
	private Vector3 _originPosition;

	private bool _isSelected;

	public int _nCellY = -1;
	public int _nCellX = -1;
	public float _fThreshold = 0.05f;
	
	void OnDrag()
	{
		Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
		Vector3 objPosition = UICamera.mainCamera.ScreenToWorldPoint(mousePosition);
		Vector3 deltaPosition = objPosition - _pressedPosition;
		float deltaY = 0.0f, deltaX = 0.0f;

		if (deltaPosition.y * deltaPosition.y > deltaPosition.x * deltaPosition.x)
		{
			deltaPosition.x = 0;
			deltaY = deltaPosition.y;
		}
		else
		{
			deltaPosition.y = 0;
			deltaX = deltaPosition.x;
		}

		//transform.position = _originPosition + deltaPosition;
		
		if (deltaY > _fThreshold)
		{
			Table.instance.MoveCell(_nCellY, _nCellX, 0);
		}
		else if (deltaY < -_fThreshold)
		{
			Table.instance.MoveCell(_nCellY, _nCellX, 2);
		}
		else if (deltaX > _fThreshold)
		{
			Table.instance.MoveCell(_nCellY, _nCellX, 1);
		}
		else if (deltaX < -_fThreshold)
		{
			Table.instance.MoveCell(_nCellY, _nCellX, 3);
		}

	}
	void OnDragEnd()
	{
		transform.position = _originPosition;

	}
	void OnPress()
	{
		Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
		Vector3 objPosition = UICamera.mainCamera.ScreenToWorldPoint(mousePosition);

		_pressedPosition = objPosition;
		_originPosition = transform.position;

	}
	void OnSelect(bool selected)
	{
		if (selected)
		{
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
	}

	// Update is called once per frame
	void Update()
	{
		//_nAnimatingFrame = (int)(_nAnimatingSecond / Time.deltaTime);
	}

	public void SetColor(int colorIndex, int colorMax)
	{
		if (colorIndex < 0 || colorMax == 0)
		{
			_sprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			_label.color = _sprite.color;
			_label.text = "";

			return;
		}

		_nColor = colorIndex;
		_nColorMax = colorMax;

		float concentration = (float)_nColor / (_nColorMax - 1);
		Color tint = new Color();
		_fColorDiff = (_fColorMax - _fColorMin);

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
