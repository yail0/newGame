using UnityEngine;
using System.Collections;

public class Table : MonoBehaviour
{

	public static Table instance;

	private int _nWidth = 0;
	private int _nHeight = 0;

	public GameObject _cell;
	public Cell[,] _table;

	public UILabel _scoreText;
	private int _nDestScore = 0;
	private int _nCurrentScore = 0;
	private int _nStartScore = 0;

	private int _nCurrentGuidingCellX;
	private int _nCurrentGuidingCellY;

	public UILabel _comboText;

	private bool _willDrawTable = false;

	public void Init()
	{
		if (LogicSide.instance == null || LogicSide.instance._mainTable == null)
		{
			return;
		}

		_nWidth = LogicSide.instance._nTableWidth;
		_nHeight = LogicSide.instance._nTableHeight;

		if (_nWidth > 0 && _nHeight > 0)
		{
			_table = new Cell[_nHeight, _nWidth];

			for (int i = 0; i < _nHeight; ++i)
			{
				for (int j = 0; j < _nWidth; ++j)
				{
					GameObject newObject = NGUITools.AddChild(this.gameObject, _cell);
					Cell newCell = newObject.GetComponent<Cell>();

					Vector3 coord = new Vector3(j * newCell._btnSprite.localSize.x, i * newCell._btnSprite.localSize.y, 0.0f);
					newCell.transform.localPosition = transform.localPosition + coord * 0.9f;

					newCell._nCellX = j;
					newCell._nCellY = i;

					_table[i, j] = newCell;
				}
			}

			RedrawTable();

		}
	}

	// Use this for initialization
	void Start()
	{
		instance = this;
	}

	// Update is called once per frame
	void Update()
	{
		if (_table == null)
		{
			Init();
		}
		if (_willDrawTable)
		{
			RedrawTable();
		}

		if (_nCurrentScore < _nDestScore)
		{
			int delta = (int)((_nDestScore - _nStartScore) * Time.deltaTime);

			_nCurrentScore += delta;
		}
		if (_nCurrentScore > _nDestScore)
		{
			_nCurrentScore = _nDestScore;
		}
		if (_scoreText != null)
		{
			_scoreText.text = "Score: " + (_nCurrentScore).ToString("D6");
		}
	}

	// direction은 up:0 right:1 down:2 left:3
	public void MoveCell(int y, int x, int direction)
	{
		int i = y, j = x;

		switch (direction)
		{
			case 0:
				++i;
				break;
			case 1:
				++j;
				break;
			case 2:
				--i;
				break;
			case 3:
				--j;
				break;
			default:
				return;
		}

		UICamera.selectedObject = null;

		LogicSide.instance.ChangeCell(y, x, i, j);

	}

	public void CellChangingAnimation(int fromY, int fromX, int toY, int toX, float time = 0.5f)
	{
		Vector3 aPosition = _table[fromY, fromX].transform.localPosition;
		Vector3 bPosition = _table[toY, toX].transform.localPosition;
		TweenPosition aTween = _table[fromY, fromX].GetComponent<TweenPosition>();
		TweenPosition bTween = _table[toY, toX].GetComponent<TweenPosition>();

		aTween.from = aPosition;
		aTween.to = bPosition;
		aTween.duration = time;
		aTween.ResetToBeginning();
		aTween.enabled = true;

		bTween.from = bPosition;
		bTween.to = aPosition;
		bTween.duration = time;
		bTween.ResetToBeginning();
		bTween.enabled = true;
	}

	public bool RedrawTable()
	{
		if (_table == null)
		{
			_willDrawTable = true;
			return false;
		}

		_willDrawTable = false;

		for (int i = 0; i < _nHeight; ++i)
		{
			for (int j = 0; j < _nWidth; ++j)
			{
				if (_table[i, j] == null)
				{
					continue;
				}

				_table[i, j].SetColor(LogicSide.instance._mainTable.GetColor(i, j), LogicSide.instance._nCellColorMax);

				switch (LogicSide.instance._mainTable.GetBoomType(i, j))
				{
					case CellBoomType.SCENE:
						_table[i, j].SetText("*");
						break;
					case CellBoomType.COLOR:
						_table[i, j].SetText("C");
						break;
					case CellBoomType.CROSS:
						_table[i, j].SetText("+");
						break;
					case CellBoomType.SQUARE:
						_table[i, j].SetText("[]");
						break;
					case CellBoomType.HORZ:
						_table[i, j].SetText("-");
						break;
					case CellBoomType.VERT:
						_table[i, j].SetText("I");
						break;
					default:
						break;
				}

			}

		}

		return true;
	}

	public void RedrawCombo(int combo)
	{
		if (_comboText != null && combo > 1)
		{
			_comboText.enabled = true;
			_comboText.fontSize = 80 + 20 * combo;
			_comboText.GetComponent<TweenPosition>().ResetToBeginning();
			_comboText.GetComponent<TweenPosition>().enabled = true;
			_comboText.GetComponent<TweenAlpha>().ResetToBeginning();
			_comboText.GetComponent<TweenAlpha>().enabled = true;
			_comboText.text = combo.ToString() + " Combo!";
		}
	}
	public void RedrawText(string text)
	{
		_comboText.enabled = true;
		_comboText.fontSize = 100;
		_comboText.GetComponent<TweenPosition>().ResetToBeginning();
		_comboText.GetComponent<TweenPosition>().enabled = true;
		_comboText.GetComponent<TweenAlpha>().ResetToBeginning();
		_comboText.GetComponent<TweenAlpha>().enabled = true;
		_comboText.text = text;
	}

	public void SetMatchGuide(int y, int x)
	{
		if (y < 0 || y >= _nHeight || x < 0 || x >= _nWidth)
		{
			return;
		}

		_table[_nCurrentGuidingCellY, _nCurrentGuidingCellX]._isGuidingCell = false;

		_nCurrentGuidingCellY = y;
		_nCurrentGuidingCellX = x;

		_table[y, x]._isGuidingCell = true;

	}

	public void SetScore(int score)
	{
		if (score <= _nStartScore)
		{
			return;
		}

		_nDestScore = score;
		_nStartScore = _nCurrentScore;

		if (_scoreText != null)
		{
			_scoreText.GetComponent<TweenScale>().ResetToBeginning();
			_scoreText.GetComponent<TweenScale>().enabled = true;
		}

	}

	public void FallCell(int y, int x, int originY)
	{
		if (y < 0 || y >= _nHeight || x < 0 || x >= _nWidth || _table[y, x] == null || originY == 0)
		{
			return;
		}

		TweenPosition tweenPos = _table[y, x].GetComponent<TweenPosition>();

		int startingY = (originY < 0) ? _nHeight - originY - 1 : originY;

		Vector3 originCoord = new Vector3(x * _table[y, x]._btnSprite.localSize.x, startingY * _table[y, x]._btnSprite.localSize.y, 0.0f);
		Vector3 destCoord = new Vector3(x * _table[y, x]._btnSprite.localSize.x, y * _table[y, x]._btnSprite.localSize.y, 0.0f);

		tweenPos.from = transform.localPosition + originCoord * 0.9f;
		tweenPos.to = transform.localPosition + destCoord * 0.9f;

		tweenPos.duration = (LogicSide.instance._fCellChangingDelaySecond) * (startingY - y) / _nHeight;

		print(string.Format("{0} {1} {2} {3}", y, originY, startingY, tweenPos.duration));

		tweenPos.ResetToBeginning();
		tweenPos.enabled = true;

	}

}