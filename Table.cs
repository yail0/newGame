using UnityEngine;
using System.Collections;

public class Table : MonoBehaviour
{

	public static Table instance;

	private int _nWidth;
	private int _nHeight;

	public GameObject _cell;
	public Cell[,] _table;

	public bool _canControl = true;

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

					Vector3 coord = new Vector3(j * newCell._sprite.localSize.x, i * newCell._sprite.localSize.y, 0.0f);
					newCell.transform.position = transform.position + UICamera.mainCamera.ScreenToWorldPoint(coord) * 2 / 3;

					newCell._nCellX = j;
					newCell._nCellY = i;

					_table[i, j] = newCell;
				}
			}

			CallLogicTable();

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
		if (_nWidth == 0 || _nHeight == 0)
		{
			Init();
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

		_canControl = false;
		UICamera.selectedObject = null;

		if (LogicSide.instance.ChangeCell(y, x, i, j))
		{
			CallLogicTable();
		}

	}

	public bool CallLogicTable()
	{
		if (_table == null)
		{
			return false;
		}

		for (int i = 0; i < _nHeight; ++i)
		{
			for (int j = 0; j < _nWidth; ++j)
			{
				if (_table[i, j] == null)
				{
					return false;
				}

				_table[i, j].SetColor(LogicSide.instance._mainTable.GetColor(i, j), LogicSide.instance._nCellColorMax);

			}

		}

		return true;
	}

}