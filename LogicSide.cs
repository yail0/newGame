﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CellBoomType
{
	NORMAL,
	SQUARE,
	HORZ,
	VERT,
	CROSS,
	COLOR,
	SCENE,
	CANTBOOM,

}
public enum Explosion
{
	HORZFOUR,
	HORZFIVE,
	HORZOVERSIX,
	VERTFOUR,
	VERTFIVE,
	VERTOVERSIX,
	CROSS,

}// TODO: xml 같은 거로 빼자

public class LogicalCell
{
	private int _nCellColor; // -1이면 세팅되지 않은 상태
	private CellBoomType _BoomType;
	private bool _matchability;
	private bool _willExplode;
	private int _originalY; // 0이면 떨어지지 않는 상태, -1이면 화면 밖에서부터 떨어지는 상태

	public LogicalCell(int color = -1, CellBoomType BoomType = CellBoomType.NORMAL)
	{
		_nCellColor = color;
		_BoomType = BoomType;
		_matchability = false;
		_willExplode = false;
		_originalY = 0;

	}

	public int GetColor() { return _nCellColor; }
	public void SetColor(int color) { _nCellColor = color; }

	public CellBoomType GetBoomType() { return _BoomType; }
	public void SetBoomType(CellBoomType BoomType) { _BoomType = BoomType; }

	public bool GetMatchability() { return _matchability; }
	public void SetMatchability(bool matchability) { _matchability = matchability; }

	public bool GetWillExplode() { return _willExplode; }
	public void SetWillExplode(bool boom) { _willExplode = boom; }

	// 0이면 떨어지지 않는 상태, -1이면 화면 밖에서부터 떨어지는 상태
	public int GetOriginalY() { return _originalY; }
	public void SetOriginalY(int y) { _originalY = y; }

}

public class LogicalTable
{
	#region 변수 및 생성자
	private int _nWidth;
	private int _nHeight;
	private int _nColorMax;
	private int _nMatchableCellMin;

	private LogicalCell[,] _table;
	private int[] _colorQtt;
	private List<int> _colorList;
	private List<Vector2> _matchableCellList;

	private class ExplodingInfo
	{
		public int _y { get; set; }
		public int _x { get; set; }
		public Explosion _exp { get; set; }
		public int _color { get; set; }

		public ExplodingInfo(int y, int x, Explosion exp, int color)
		{
			_y = y;
			_x = x;
			_exp = exp;
			_color = color;
		}
	}
	Queue<ExplodingInfo> _expInfo;

	public LogicalTable(int width, int height, int colorMax, int matchableCellMin)
	{
		_nWidth = width;
		_nHeight = height;
		_nColorMax = colorMax;
		_nMatchableCellMin = matchableCellMin;

		_colorQtt = new int[_nColorMax];

		_expInfo = new Queue<ExplodingInfo>();

		_colorList = new List<int>();
		_matchableCellList = new List<Vector2>();

		FillTable(true);

	}
	#endregion

	#region 색깔 세팅
	public bool FillTable(bool isInitState = false)
	{
		int i, j;

		if (!isInitState)
		{
			_colorQtt.Initialize();
			_colorList.Clear();

			for (i = 0; i < _nHeight; ++i)
			{
				for (j = 0; j < _nWidth; ++j)
				{
					int color = GetColor(i, j);
					if (color >= 0)
					{
						++_colorQtt[color];
						_colorList.Add(color);

					}
				}
			}
		}

		bool isFillable = true;
		do
		{
			_table = new LogicalCell[_nWidth, _nHeight];

			for (int n = 0; n < _nMatchableCellMin; ++n)
			{
				do
				{
					i = Random.Range(1, _nHeight - 1);
					j = Random.Range(1, _nWidth - 1);

				} while (!MakeMatchableCellAt(i, j, isInitState));

			}

			for (i = 0; i < _nHeight && isFillable; ++i)
			{
				for (j = 0; j < _nWidth && isFillable; ++j)
				{
					if (_table[i, j] == null)
					{
						isFillable &= RandColor(i, j, isInitState, !isInitState);
						_table[i, j].SetOriginalY(0);
					}
				}
			}

			// 만약 어떻게 해도 채울 수 없는 셀이 존재하는 경우 다시 처음부터.
		} while (!isFillable);

		ChkMatchability();

		Table.instance.RedrawTable();

		return true;

	}

	public bool MakeMatchableCellAt(int y, int x, bool isInitState)
	{
		int i, j;

		for (i = y - 1; i < y + 2; ++i)
		{
			for (j = x - 1; j < x + 2; ++j)
			{
				if (_table[i, j] != null)
				{
					return false; // 해당 셀을 중심으로 한 3x3 안에 이미 만들어진 셀이 있다면: 초기 매치끼리 겹치거나 붙어있지 않기 위해 다른 셀을 선택하도록.
				}
			}
		}

		int color;

		if (isInitState)
		{
			color = Random.Range(0, _nColorMax);
		}
		else
		{
			List<int> _colorsOverThreeCells = new List<int>();

			for (i = 0; i < _nColorMax; ++i)
			{
				if (_colorQtt[i] > 2)
				{
					_colorsOverThreeCells.Add(i);
				}
			}

			color = _colorsOverThreeCells[Random.Range(0, _colorsOverThreeCells.Count)];

			_colorQtt[color] -= 3;

			_colorList.Remove(color);
			_colorList.Remove(color);
			_colorList.Remove(color);

		}

		_table[y, x] = new LogicalCell(color);

		// 여기서는 랜덤을 통해 이 셀을 매치 가능한 상태 중 일부로 만든다.
		/*
		 * 0.	*		1.	 **		2.	**		3.	  *		4.	* *		5.	 *	
		 *		 **			*			  *			**			 *			* *	
		 *		 
		 * 6. 	 *		7.	*		8.	*		9.	 *		10.	*		11.	 *
		 *		 *			*			 *			*			 *			*
		 *		*			 *			 *			*			*			 *
		*/
		switch (Random.Range(0, 12))
		{
			case 0:
				_table[y + 1, x - 1] = new LogicalCell(color);
				_table[y, x + 1] = new LogicalCell(color);
				break;
			case 1:
				_table[y - 1, x - 1] = new LogicalCell(color);
				_table[y, x + 1] = new LogicalCell(color);
				break;
			case 2:
				_table[y, x - 1] = new LogicalCell(color);
				_table[y + 1, x + 1] = new LogicalCell(color);
				break;
			case 3:
				_table[y, x - 1] = new LogicalCell(color);
				_table[y - 1, x + 1] = new LogicalCell(color);
				break;
			case 4:
				_table[y + 1, x - 1] = new LogicalCell(color);
				_table[y + 1, x + 1] = new LogicalCell(color);
				break;
			case 5:
				_table[y - 1, x - 1] = new LogicalCell(color);
				_table[y - 1, x + 1] = new LogicalCell(color);
				break;
			case 6:
				_table[y - 1, x - 1] = new LogicalCell(color);
				_table[y + 1, x] = new LogicalCell(color);
				break;
			case 7:
				_table[y - 1, x + 1] = new LogicalCell(color);
				_table[y + 1, x] = new LogicalCell(color);
				break;
			case 8:
				_table[y - 1, x] = new LogicalCell(color);
				_table[y + 1, x - 1] = new LogicalCell(color);
				break;
			case 9:
				_table[y - 1, x] = new LogicalCell(color);
				_table[y + 1, x + 1] = new LogicalCell(color);
				break;
			case 10:
				_table[y - 1, x - 1] = new LogicalCell(color);
				_table[y + 1, x - 1] = new LogicalCell(color);
				break;
			case 11:
				_table[y - 1, x + 1] = new LogicalCell(color);
				_table[y + 1, x + 1] = new LogicalCell(color);
				break;
		}

		return true;
	}

	public bool RandColor(int i, int j, bool isInitState = false, bool isShuffling = false)
	{
		int color = Random.Range(0, _nColorMax);

		if (!isInitState && !isShuffling)
		{
			// 새 셀이 필요한 경우: 아무 색이나 가져와도 됨.
			_table[i, j] = new LogicalCell(color);

			return true;
		}

		// 테이블 초기화 단계: 자동으로 터지는 셀이 없어야 함.
		int upCellColor = GetColor(i + 1, j);
		int downCellColor = GetColor(i - 1, j);
		int leftCellColor = GetColor(i, j - 1);
		int rightCellColor = GetColor(i, j + 1);

		bool areUpTwoCellsSame = (upCellColor != -1 && upCellColor == GetColor(i + 2, j));
		bool areDownTwoCellsSame = (downCellColor != -1 && downCellColor == GetColor(i - 2, j));
		bool areLeftTwoCellsSame = (leftCellColor != -1 && leftCellColor == GetColor(i, j - 2));
		bool areRightTwoCellsSame = (rightCellColor != -1 && rightCellColor == GetColor(i, j + 2));

		bool areHorizTwoCellsSame = (leftCellColor != -1 && leftCellColor == rightCellColor);
		bool areVertTwoCellsSame = (upCellColor != -1 && upCellColor == downCellColor);

		if (!areUpTwoCellsSame
			&& !areDownTwoCellsSame
			&& !areLeftTwoCellsSame
			&& !areRightTwoCellsSame
			&& !areHorizTwoCellsSame
			&& !areVertTwoCellsSame)
		{
			if (isShuffling)
			{
				int index = Random.Range(0, _colorList.Count);
				color = _colorList[index];

				--_colorQtt[color];
				_colorList.RemoveAt(index);
			}

			_table[i, j] = new LogicalCell(color);

			return true;
		}

		List<int> colorList = new List<int>();
		for (int k = 0; k < _nColorMax; ++k)
		{
			if ((k == upCellColor && (areUpTwoCellsSame || areVertTwoCellsSame))
				|| (k == downCellColor && areDownTwoCellsSame)
				|| (k == leftCellColor && (areLeftTwoCellsSame || areHorizTwoCellsSame))
				|| (k == rightCellColor && areRightTwoCellsSame))
			{
				continue;
			}

			if (isShuffling)
			{
				if (_colorQtt[k] <= 0)
				{
					continue;
				}

				--_colorQtt[k];
				_colorList.Remove(k);

			}

			colorList.Add(k);

		}

		if (colorList.Count <= 0)
		{
			_table[i, j] = new LogicalCell();

			return false;
		}

		_table[i, j] = new LogicalCell(colorList[Random.Range(0, colorList.Count)]);

		return true;

	}
	#endregion

	#region 체크
	public IEnumerator ChkProcess()
	{
		int nCombo = 0;
		do
		{
			LogicSide.instance._fCurrentCellChgDelaySecond = LogicSide.instance._fCellChangingDelaySecond;
			Table.instance.RedrawTable();
			yield return new WaitForSeconds(LogicSide.instance._fCellChangingDelaySecond / 2);

			int explodedNo = 0;

			for (int i = 0; i < _nHeight; ++i)
			{
				for (int j = 0; j < _nWidth; ++j)
				{
					if (_table[i, j] != null && _table[i, j].GetWillExplode())
					{
						_table[i, j] = null;
						++explodedNo;
					}

				}
			}

			LogicSide.instance.AddScore(explodedNo, nCombo++);

			while (_expInfo.Count > 0)
			{
				ExplodingInfo info = _expInfo.Dequeue();
				int i = info._y, j = info._x;
				int color = info._color;

				if (_table[i, j] != null)
				{
					List<Vector2> nullCellList = new List<Vector2>();
					if (i > 0 && _table[i - 1, j] == null)
					{
						nullCellList.Add(new Vector2(i - 1, j));
					}
					if (i < _nHeight - 1 && _table[i + 1, j] == null)
					{
						nullCellList.Add(new Vector2(i + 1, j));
					}
					if (j > 0 && _table[i, j - 1] == null)
					{
						nullCellList.Add(new Vector2(i, j - 1));
					}
					if (j < _nWidth - 1 && _table[i, j + 1] == null)
					{
						nullCellList.Add(new Vector2(i, j + 1));
					}

					if (nullCellList.Count == 0)
					{
						continue;
					}

					Vector2 nullCellCoord = nullCellList[Random.Range(0, nullCellList.Count)];
					i = (int)nullCellCoord.x;
					j = (int)nullCellCoord.y;
				}

				switch (info._exp)
				{
					case Explosion.HORZOVERSIX:
					case Explosion.VERTOVERSIX:
						_table[i, j] = new LogicalCell(color);
						_table[i, j].SetBoomType(CellBoomType.SCENE);
						break;
					case Explosion.VERTFIVE:
					case Explosion.HORZFIVE:
						_table[i, j] = new LogicalCell(_nColorMax);
						_table[i, j].SetBoomType(CellBoomType.COLOR);
						break;
					case Explosion.CROSS:
						_table[i, j] = new LogicalCell(color);
						_table[i, j].SetBoomType(CellBoomType.SQUARE);
						break;
					case Explosion.HORZFOUR:
						_table[i, j] = new LogicalCell(color);
						_table[i, j].SetBoomType(CellBoomType.HORZ);
						break;
					case Explosion.VERTFOUR:
						_table[i, j] = new LogicalCell(color);
						_table[i, j].SetBoomType(CellBoomType.VERT);
						break;
				}

			}

			Table.instance.RedrawTable();
			Table.instance.RedrawCombo(nCombo);
			yield return new WaitForSeconds(LogicSide.instance._fCellChangingDelaySecond / 2);

		} while (ChkFalling());

		Table.instance.RedrawTable();
		LogicSide.instance._fCurruntGuidingSecond = LogicSide.instance._fGuidingSecond;

		if (!ChkMatchability())
		{
			LogicSide.instance.GameOver();
		}

	}
	public bool ChangeCell(int fromY, int fromX, int toY, int toX)
	{
		if (_table == null)
		{
			return false;
		}

		int fromColor = GetColor(fromY, fromX);
		int toColor = GetColor(toY, toX);

		if (GetBoomType(fromY, fromX) == CellBoomType.COLOR && GetBoomType(toY, toX) == CellBoomType.COLOR)
		{
			for (int i = 0; i < _nHeight; ++i)
			{
				for (int j = 0; j < _nWidth; ++j)
				{
					ChkBoomType(i, j);
				}
			}

			return true;
		}

		if (GetBoomType(fromY, fromX) == CellBoomType.COLOR && toColor >= 0)
		{
			_table[fromY, fromX].SetWillExplode(true);

			for (int i = 0; i < _nHeight; ++i)
			{
				for (int j = 0; j < _nWidth; ++j)
				{
					if (GetColor(i, j) == toColor)
					{
						ChkBoomType(i, j);
					}
				}
			}

			return true;
		}

		if (GetBoomType(toY, toX) == CellBoomType.COLOR && fromColor >= 0)
		{
			_table[toY, toX].SetWillExplode(true);

			for (int i = 0; i < _nHeight; ++i)
			{
				for (int j = 0; j < _nWidth; ++j)
				{
					if (GetColor(i, j) == fromColor)
					{
						ChkBoomType(i, j);
					}
				}
			}

			return true;
		}

		if (fromColor < 0 || toColor < 0)
		{
			return false;
		}

		_table[fromY, fromX].SetColor(-1);
		_table[toY, toX].SetColor(-1);

		if (!ChkMatchabilityOfCellNColor(toY, toX, fromColor) && !ChkMatchabilityOfCellNColor(fromY, fromX, toColor))
		{
			_table[fromY, fromX].SetColor(fromColor);
			_table[toY, toX].SetColor(toColor);

			return false;
		}

		_table[fromY, fromX].SetColor(fromColor);
		_table[toY, toX].SetColor(toColor);

		LogicalCell tempCell = _table[fromY, fromX];
		_table[fromY, fromX] = _table[toY, toX];
		_table[toY, toX] = tempCell;

		return true;
	}

	public bool ChkFalling()
	{
		bool hasExplodingCell = false;

		for (int i = 0; i < _nHeight; ++i)
		{
			for (int j = 0; j < _nWidth; ++j)
			{
				if (GetColor(i, j) < 0)
				{
					int k = i;
					while (++k < _nHeight && GetColor(k, j) < 0)
					{
					}

					if (k < _nHeight)
					{
						_table[i, j] = _table[k, j];
						_table[i, j].SetOriginalY(k);
						_table[k, j] = null;

					}
				}
			}
		}

		for (int i = 0; i < _nHeight; ++i)
		{
			for (int j = 0; j < _nWidth; ++j)
			{
				if (GetColor(i, j) < 0)
				{
					for (int k = i; k < _nHeight; ++k)
					{
						RandColor(k, j);
						_table[k, j].SetOriginalY(-k + i - 1);
						Debug.Log(string.Format("{0} {1}", k, -k + i - 1));
					}
				}

				if (_table[i, j].GetOriginalY() != 0)
				{
					hasExplodingCell |= ChkExplosionOfCell(i, j);
				}
			}
		}

		for (int i = 0; i < _nHeight; ++i)
		{
			for (int j = 0; j < _nWidth; ++j)
			{
				Table.instance.FallCell(i, j, _table[i, j].GetOriginalY());
				_table[i, j].SetOriginalY(0);
			}
		}

		return hasExplodingCell;
	}
	public bool ChkExplosionOfCell(int y, int x)
	{
		if (_table[y, x].GetWillExplode())
		{
			return true;
		}

		int color = GetColor(y, x);

		if (color < 0)
		{
			return false;
		}

		int vertMatch = 1;
		int horzMatch = 1;

		// 세로에 몇 개가 매칭되었나 확인
		int i = y;
		while (GetColor(++i, x) == color)
		{
			++vertMatch;
		}
		i = y;
		while (GetColor(--i, x) == color)
		{
			++vertMatch;
		}

		// 가로에 몇 개가 매칭되었나 확인
		int j = x;
		while (GetColor(y, ++j) == color)
		{
			++horzMatch;
		}
		j = x;
		while (GetColor(y, --j) == color)
		{
			++horzMatch;
		}

		// 매칭 수가 부족하다면?
		if (horzMatch < 3 && vertMatch < 3)
		{
			return false;
		}

		// 매칭된 것들을 제거하기 위해 플래그를 true
		if (vertMatch >= 3)
		{
			ChkBoomType(y, x);

			i = y;
			while (GetColor(++i, x) == color)
			{
				ChkBoomType(i, x);
			}
			i = y;
			while (GetColor(--i, x) == color)
			{
				ChkBoomType(i, x);
			}
		}
		if (horzMatch >= 3)
		{
			ChkBoomType(y, x);

			j = x;
			while (GetColor(y, ++j) == color)
			{
				ChkBoomType(y, j);
			}
			j = x;
			while (GetColor(y, --j) == color)
			{
				ChkBoomType(y, j);
			}
		}

		// 특수한 경우 폭발 태그를 테이블에 추가. 일괄적으로 적용.
		if (horzMatch >= 3 && vertMatch >= 3)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.CROSS, color));
		}

		if (horzMatch >= 6)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.HORZOVERSIX, color));
		}
		else if (horzMatch == 5)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.HORZFIVE, color));
		}
		else if (horzMatch == 4)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.HORZFOUR, color));
		}

		if (vertMatch >= 6)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.VERTOVERSIX, color));
		}
		else if (vertMatch == 5)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.VERTFIVE, color));
		}
		else if (vertMatch == 4)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.VERTFOUR, color));
		}

		return true;
	}
	public bool ChkBoomType(int y, int x)
	{
		if (GetBoomType(y, x) == CellBoomType.CANTBOOM)
		{
			return false;
		}

		if (_table == null)
		{
			return false;
		}

		_table[y, x].SetWillExplode(true);

		switch (_table[y, x].GetBoomType())
		{
			case CellBoomType.HORZ:
				for (int j = 0; j < _nWidth; ++j)
				{
					if (_table[y, j] != null && !_table[y, j].GetWillExplode())
					{
						ChkBoomType(y, j);
					}
				}
				break;
			case CellBoomType.VERT:
				for (int i = 0; i < _nHeight; ++i)
				{
					if (_table[i, x] != null && !_table[i, x].GetWillExplode())
					{
						ChkBoomType(i, x);
					}
				}
				break;
			case CellBoomType.CROSS:
				for (int j = 0; j < _nWidth; ++j)
				{
					if (_table[y, j] != null && !_table[y, j].GetWillExplode())
					{
						ChkBoomType(y, j);
					}
				}
				for (int i = 0; i < _nHeight; ++i)
				{
					if (_table[i, x] != null && !_table[i, x].GetWillExplode())
					{
						ChkBoomType(i, x);
					}
				}
				break;
			case CellBoomType.SQUARE:
				for (int i = y - 1; i < y + 2; ++i)
				{
					for (int j = x - 1; j < x + 2; ++j)
					{
						if (i < 0 || i >= _nHeight || j < 0 || j >= _nWidth || _table[i, j] == null || _table[i, j].GetWillExplode())
						{
							continue;
						}

						ChkBoomType(i, j);
					}
				}
				break;
			case CellBoomType.SCENE:
				for (int i = 0; i < _nHeight; ++i)
				{
					for (int j = 0; j < _nWidth; ++j)
					{
						if (_table[i, j] != null)
						{
							_table[i, j].SetWillExplode(true);
						}
					}
				}
				break;
			default:
				break;
		}

		return true;
	}

	public bool ChkMatchability()
	{
		bool hasMatchableCell = false;

		_matchableCellList.Clear();

		for (int i = 0; i < _nHeight; ++i)
		{
			for (int j = 0; j < _nWidth; ++j)
			{
				bool matchability = false;
				int color = GetColor(i, j);
				if (color < 0)
				{
					continue;
				}

				_table[i, j].SetColor(-1);

				if (ChkMatchabilityOfCellNColor(i - 1, j, color)
					|| ChkMatchabilityOfCellNColor(i + 1, j, color)
					|| ChkMatchabilityOfCellNColor(i, j - 1, color)
					|| ChkMatchabilityOfCellNColor(i, j + 1, color))
				{
					matchability = true;
					hasMatchableCell = true;
					_matchableCellList.Add(new Vector2(i, j));
				}

				_table[i, j].SetColor(color);

				_table[i, j].SetMatchability(matchability);

			}
		}

		return hasMatchableCell;
	}
	public bool ChkMatchabilityOfCellNColor(int y, int x, int color)
	{
		int nDownDownColor = GetColor(y - 2, x);
		int nDownColor = GetColor(y - 1, x);
		int nUpColor = GetColor(y + 1, x);
		int nUpUpColor = GetColor(y + 2, x);

		int nLeftLeftColor = GetColor(y, x - 2);
		int nLeftColor = GetColor(y, x - 1);
		int nRightColor = GetColor(y, x + 1);
		int nRightRightColor = GetColor(y, x + 2);

		if ((nDownDownColor == nDownColor && nDownColor == color)
			|| (nDownColor == nUpColor && nUpColor == color)
			|| (nUpColor == nUpUpColor && nUpColor == color)
			|| (nLeftLeftColor == nLeftColor && nLeftColor == color)
			|| (nLeftColor == nRightColor && nRightColor == color)
			|| (nRightColor == nRightRightColor && nRightColor == color))
		{
			return true;
		}

		return false;
	}
	#endregion

	#region 겟셋
	public int GetColor(int y, int x)
	{
		if (y < 0 || y >= _nHeight || x < 0 || x >= _nWidth || _table[y, x] == null)
		{
			return -1;
		}

		return _table[y, x].GetColor();
	}
	public CellBoomType GetBoomType(int y, int x)
	{
		if (y < 0 || y >= _nHeight || x < 0 || x >= _nWidth || _table[y, x] == null)
		{
			return CellBoomType.CANTBOOM;
		}

		return _table[y, x].GetBoomType();
	}
	public bool GetMatchability(int y, int x)
	{
		if (y < 0 || y >= _nHeight || x < 0 || x >= _nWidth || _table[y, x] == null)
		{
			return false;
		}

		return _table[y, x].GetMatchability();
	}
	public Vector2 GetRandomMatchableCell()
	{
		if (_matchableCellList.Count == 0)
		{
			return new Vector2(-1, -1);
		}

		return _matchableCellList[Random.Range(0, _matchableCellList.Count)];
	}
	#endregion

}

public class LogicSide : MonoBehaviour
{
	public static LogicSide instance = null;

	public int _nTableWidth = 8;                // 가로 셀 갯수
	public int _nTableHeight = 8;           // 세로 셀 갯수
	public int _nCellColorMax = 6;          // 셀 종류
	public int _nMatchableCellMin = 2;      // 뒤섞거나 처음으로 세팅할 때 최소한으로 필요한 매치 가능 셀의 수

	public float _fCellChangingDelaySecond = 1.0f;
	public float _fCurrentCellChgDelaySecond = 0.0f;
	public float _fGuidingSecond = 4.0f;
	public float _fCurruntGuidingSecond = 0.0f;

	private int _nCurrentScore = 0;

	public bool _isInfinityMode = false;

	public LogicalTable _mainTable; // 테이블

	// Use this for initialization
	void Start()
	{
		instance = this;

		_mainTable = new LogicalTable(_nTableWidth, _nTableHeight, _nCellColorMax, _nMatchableCellMin);

		_fCurruntGuidingSecond = _fGuidingSecond;

		StartCoroutine(SetMatchGuide());
	}

	// Update is called once per frame
	void Update()
	{
		if (_mainTable == null)
		{
			return;
		}

		_fCurrentCellChgDelaySecond -= Time.deltaTime;
		_fCurruntGuidingSecond -= Time.deltaTime;

	}

	public bool ChangeCell(int fromY, int fromX, int toY, int toX)
	{
		if (!_mainTable.ChangeCell(fromY, fromX, toY, toX))
		{
			return false;
		}

		_mainTable.ChkExplosionOfCell(fromY, fromX);
		_mainTable.ChkExplosionOfCell(toY, toX);

		StartCoroutine(_mainTable.ChkProcess());
		_fCurruntGuidingSecond = _fGuidingSecond;

		return true;
	}

	public void AddScore(int explodingNumber, int combo)
	{
		_nCurrentScore += explodingNumber * 100 + (combo - 1) * 10 * explodingNumber;

		Table.instance.SetScore(_nCurrentScore);

	}

	IEnumerator SetMatchGuide()
	{
		while (true)
		{
			if (_mainTable == null || Table.instance == null)
			{
				yield return null;
				continue;
			}

			if (_fCurruntGuidingSecond < 0.0f)
			{
				Vector2 selectedCell = _mainTable.GetRandomMatchableCell();
				if (selectedCell != null)
				{
					Table.instance.SetMatchGuide((int)selectedCell.x, (int)selectedCell.y);
				}

				_fCurruntGuidingSecond = _fGuidingSecond;

			}

			yield return null;

		}

	}

	public void GameOver()
	{
		if (LogicSide.instance._isInfinityMode)
		{
			Table.instance.RedrawText("Shuffling....");
			_mainTable.FillTable();
		}
		else
		{
			Table.instance.RedrawText("GAME OVER");
			Table.instance.SetScore(0);
			_mainTable.FillTable(true);
		}
	}

}
