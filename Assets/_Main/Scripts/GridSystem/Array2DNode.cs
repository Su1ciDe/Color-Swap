using Array2DEditor;
using UnityEngine;

namespace GridSystem
{
	[System.Serializable]
	public class Array2DNode : Array2D<NodeType>
	{
		[SerializeField] private CellRowNode[] cells = new CellRowNode[3];

		protected override CellRow<NodeType> GetCellRow(int idx)
		{
			return cells[idx];
		}
	}

	[System.Serializable]
	public class CellRowNode : CellRow<NodeType>
	{
	}
}