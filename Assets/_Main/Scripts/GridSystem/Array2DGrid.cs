using Array2DEditor;
using UnityEngine;

namespace GridSystem
{
	[System.Serializable]
	public class Array2DGrid : Array2D<CellType>
	{
		[SerializeField] private CellRowGrid[] cells = new CellRowGrid[Consts.defaultGridSize];

		protected override CellRow<CellType> GetCellRow(int idx)
		{
			return cells[idx];
		}
	}

	[System.Serializable]
	public class CellRowGrid : CellRow<CellType>
	{
	}
}