using UnityEngine;
using System.Collections;

public class Table : MonoBehaviour {
	
	private int _nWidth;
	private int _nHeight;

	public GameObject _cell;
	public Cell[,] _table;

	public void Init()
	{
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

					_table[i, j] = newCell;
				}
			}
		}
	}

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_nWidth == 0 || _nHeight == 0)
		{
			Init();
		}
	}
}
