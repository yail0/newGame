﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CellBoomType
{
	NORMAL,
	SQUARE,
	LINE,
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

	private class ExplodingInfo
	{
		public int _y { get; set; }
		public int _x { get; set; }
		public Explosion _exp { get; set; }

		public ExplodingInfo(int y, int x, Explosion exp)
		{
			_y = y;
			_x = x;
			_exp = exp;
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

			if (_colorList == null)
			{
				_colorList = new List<int>();
			}
			else
			{
				_colorList.Clear();
			}

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
						_table[i, j].SetOriginalY(-1);
					}
				}
			}

			// 만약 어떻게 해도 채울 수 없는 셀이 존재하는 경우 다시 처음부터.
		} while (!isFillable);

		ChkMatchability();

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
				_table[y + 1, x - 1]	= new LogicalCell(color);
				_table[y, x + 1]		= new LogicalCell(color);
				break;
			case 1:
				_table[y - 1, x - 1]	= new LogicalCell(color);
				_table[y, x + 1]		= new LogicalCell(color);
				break;
			case 2:
				_table[y, x - 1]		= new LogicalCell(color);
				_table[y + 1, x + 1]	= new LogicalCell(color);
				break;
			case 3:
				_table[y, x - 1]		= new LogicalCell(color);
				_table[y - 1, x + 1]	= new LogicalCell(color);
				break;
			case 4:
				_table[y + 1, x - 1]	= new LogicalCell(color);
				_table[y + 1, x + 1]	= new LogicalCell(color);
				break;
			case 5:
				_table[y - 1, x - 1]	= new LogicalCell(color);
				_table[y - 1, x + 1]	= new LogicalCell(color);
				break;
			case 6:
				_table[y - 1, x - 1]	= new LogicalCell(color);
				_table[y + 1, x]		= new LogicalCell(color);
				break;
			case 7:
				_table[y - 1, x + 1]	= new LogicalCell(color);
				_table[y + 1, x]		= new LogicalCell(color);
				break;
			case 8:
				_table[y - 1, x]		= new LogicalCell(color);
				_table[y + 1, x - 1]	= new LogicalCell(color);
				break;
			case 9:
				_table[y - 1, x]		= new LogicalCell(color);
				_table[y + 1, x + 1]	= new LogicalCell(color);
				break;
			case 10:
				_table[y - 1, x - 1]	= new LogicalCell(color);
				_table[y + 1, x - 1]	= new LogicalCell(color);
				break;
			case 11:
				_table[y - 1, x + 1]	= new LogicalCell(color);
				_table[y + 1, x + 1]	= new LogicalCell(color);
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

	public bool ChangeCell(int fromY, int fromX, int toY, int toX)
	{
		int fromColor = GetColor(fromY, fromX);
		int toColor = GetColor(toY, toX);

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

	#region 체크
	public void ChkProcess()
	{
		int index = 0;
		while (ChkExplosion())
		{
			while (_expInfo.Count > 0)
			{
				ExplodingInfo info = _expInfo.Dequeue();
				// TODO: 제거 경우에 따라
			}

			int explodedNo = 0;

			string tableText = "";

			for (int i = 0; i < _nHeight; ++i)
			{
				for (int j = 0; j < _nWidth; ++j)
				{
					if (_table[i, j].GetWillExplode())
					{
						tableText += ". ";
						_table[i, j] = null;
						++explodedNo;
					}
					else
						tableText += GetColor(i, j).ToString() + " ";

				}
				tableText += "\n";
			}
			
			Debug.Log(string.Format("{0}: {1}", index++, explodedNo));
			Debug.Log(tableText);

			ChkFalling();

		}

		if (!ChkMatchability())
		{
			Debug.Log("섞");
			FillTable();
		}

	}

	public bool ChkFalling()
	{
		bool hasFallingCell = false;

		for (int i = 0; i < _nHeight; ++i)
		{
			for (int j = 0; j < _nWidth; ++j)
			{
				if (GetColor(i, j) < 0)
				{
					hasFallingCell = true;

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
					RandColor(i, j);
					_table[i, j].SetOriginalY(-1);
				}
			}
		}

		return hasFallingCell;
	}

	public bool ChkExplosion()
	{
		bool willExplode = false;
		for (int i = 0; i < _nHeight; ++i)
		{
			for (int j = 0; j < _nWidth; ++j)
			{
				if (_table[i, j].GetWillExplode())
				{
					continue;
				}

				willExplode |= ChkExplosionOfCell(i, j);

			}

		}

		return willExplode;
	}
	public bool ChkExplosionOfCell(int y, int x)
	{
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
			_table[y, x].SetWillExplode(true);

			i = y;
			while (GetColor(++i, x) == color)
			{
				_table[i, x].SetWillExplode(true);
			}
			i = y;
			while (GetColor(--i, x) == color)
			{
				_table[i, x].SetWillExplode(true);
			}
		}
		if (horzMatch >= 3)
		{
			_table[y, x].SetWillExplode(true);

			j = x;
			while (GetColor(y, ++j) == color)
			{
				_table[y, j].SetWillExplode(true);
			}
			j = x;
			while (GetColor(y, --j) == color)
			{
				_table[y, j].SetWillExplode(true);
			}
		}

		// 특수한 경우 폭발 태그를 테이블에 추가. 일괄적으로 적용.
		if (horzMatch >= 3 && vertMatch >= 3)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.CROSS));
		}

		if (horzMatch >= 6)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.HORZOVERSIX));
		}
		else if (horzMatch == 5)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.HORZFIVE));
		}
		else if (horzMatch == 4)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.HORZFOUR));
		}

		if (vertMatch >= 6)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.VERTOVERSIX));
		}
		else if (vertMatch == 5)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.VERTFIVE));
		}
		else if (vertMatch == 4)
		{
			_expInfo.Enqueue(new ExplodingInfo(y, x, Explosion.VERTFOUR));
		}

		return true;
	}

	public bool ChkMatchability()
	{
		bool hasMatchableCell = false;

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
			return CellBoomType.NORMAL;
		}

		return _table[x, y].GetBoomType();
	}
	public bool GetMatchability(int y, int x)
	{
		if (y < 0 || y >= _nHeight || x < 0 || x >= _nWidth || _table[y, x] == null)
		{
			return false;
		}

		return _table[y, x].GetMatchability();
	}
	#endregion

}

public class LogicSide : MonoBehaviour
{
	public static LogicSide instance = null;

	public int _nTableWidth = 8;				// 가로 셀 갯수
	public int _nTableHeight = 8;			// 세로 셀 갯수
	public int _nCellColorMax = 6;			// 셀 종류
	public int _nMatchableCellMin = 2;		// 뒤섞거나 처음으로 세팅할 때 최소한으로 필요한 매치 가능 셀의 수

	public LogicalTable _mainTable; // 테이블

	private bool _isGraphicTableInitiated = false;

	// Use this for initialization
	void Start()
	{
		instance = this;

		_mainTable = new LogicalTable(_nTableWidth, _nTableHeight, _nCellColorMax, _nMatchableCellMin);

	}

	// Update is called once per frame
	void Update()
	{		
		if (_mainTable == null)
		{
			return;
		}

	}

	public bool ChangeCell(int fromY, int fromX, int toY, int toX)
	{
		if (!_mainTable.ChangeCell(fromY, fromX, toY, toX))
		{
			return false;
		}

		_mainTable.ChkProcess();

		Table.instance._canControl = true;

		return true;
	}

}
