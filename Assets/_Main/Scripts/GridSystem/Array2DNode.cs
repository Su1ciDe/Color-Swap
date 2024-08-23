using Array2DEditor;
using UnityEngine;

namespace GridSystem
{
	[System.Serializable]
	public class Array2DNode : Array2D<TileType>
	{
		[SerializeField] private CellRowNode[] cells = new CellRowNode[3];

		protected override CellRow<TileType> GetCellRow(int idx)
		{
			return cells[idx];
		}
	}

	[System.Serializable]
	public class CellRowNode : CellRow<TileType>
	{
	}
}