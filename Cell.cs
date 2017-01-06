using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour
{

	public UISprite _btnSprite;
	public UILabel _label;

	public UISprite _smallSprite;
	public UISprite _outlineSprite;

	public int _nAnimatingSecond;

	public int _nColor;
	private int _nColorMax;

	private float _fColorDiff;
	private Color _originColor;
	public float _fColorMax;
	public float _fColorMin;

	private Vector3 _pressedPosition;
	private Vector3 _originPosition;

	public int _nCellY = -1;
	public int _nCellX = -1;
	public float _fThreshold = 0.05f;

	public bool _isGuidingCell = false;

	void OnDrag()
	{
		if (LogicSide.instance == null)
		{
			return;
		}

		if (LogicSide.instance._fCurrentCellChgDelaySecond > 0.0f)
		{
			return;
		}

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
			_btnSprite.depth += 2;
			_label.depth += 2;

		}
		else
		{
			_btnSprite.depth -= 2;
			_label.depth -= 2;

		}

	}

	IEnumerator TintAnimation()
	{
		Color delta;
		bool toggle = true;

		while (true)
		{
			if (_btnSprite == null || _smallSprite == null)
			{
				yield return null;
				continue;
			}

			if (!_isGuidingCell)
			{
				_btnSprite.color = _originColor;
				_smallSprite.color = _originColor;

				yield return null;
				continue;
			}

			delta = Color.white - _originColor;

			if (_btnSprite.color.r > 1.0f || _btnSprite.color.r < _originColor.r)
			{
				toggle = !toggle;
			}

			_btnSprite.color += (toggle ? 1 : -1) * delta * Time.deltaTime / _nAnimatingSecond;
			_smallSprite.color = _btnSprite.color;

			yield return null;
		}
	}

	// Use this for initialization
	void Start()
	{
		StartCoroutine(TintAnimation());
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void SetColor(int colorIndex, int colorMax)
	{
		if (_smallSprite == null || _btnSprite == null || _outlineSprite == null || _label == null)
		{
			return;
		}

		_smallSprite.enabled = false;
		_outlineSprite.enabled = false;

		if (colorIndex < 0 || colorMax == 0 || colorIndex >= colorMax)
		{
			_btnSprite.color = Color.white;
			_smallSprite.color = Color.white;
			_label.color = Color.white;
			_label.text = "";

			_btnSprite.enabled = false;

			return;
		}

		_btnSprite.enabled = true;

		_nColor = colorIndex;
		_nColorMax = colorMax;

		float concentration = (float)_nColor / (_nColorMax - 1);
		Color tint = new Color();
		_fColorDiff = (_fColorMax - _fColorMin);

		if (concentration < 0.5f)
		{
			concentration = concentration * concentration * 2;

			float delta = (concentration * 2) * _fColorDiff;

			tint.r = _fColorMax - delta;
			tint.g = _fColorMin + delta;
			tint.b = _fColorMin;
		}
		else
		{
			concentration = (concentration - 0.5f) * (concentration - 0.5f) * 2;

			float delta = (concentration * 2) * _fColorDiff;

			tint.g = _fColorMax - delta;
			tint.b = _fColorMin + delta;
			tint.r = _fColorMin;
		}

		tint.a = 1.0f;

		_btnSprite.color = tint;
		_smallSprite.color = tint;
		_originColor = tint;

		tint.r -= _fColorMin / 2;
		tint.g -= _fColorMin / 2;
		tint.b -= _fColorMin / 2;

		_label.color = tint;
		_label.text = (_nColor + 1).ToString();

		_isGuidingCell = false;

	}
	public void SetText(string text)
	{
		_label.text = text;
		_btnSprite.enabled = false;
		_smallSprite.enabled = true;
		_outlineSprite.enabled = true;
	}
	public void SetSize(float scale)
	{
		_btnSprite.transform.localScale = new Vector3(scale, scale, 1);
		_label.fontSize = (int)(50 * scale);

	}

}
