using Fiber.Managers;
using Fiber.Utilities;
using Fiber.Utilities.Extensions;
using TriInspector;
using UnityEditor;
using UnityEngine;

namespace GridSystem
{
	public class GridManager : Singleton<GridManager>
	{
		[Title("Properties")]
		[SerializeField, ReadOnly] private GridCellMatrix gridCells;
		public GridCellMatrix GridCells => gridCells;

		[Title("References")]
		[SerializeField] private Transform cellHolder;

		[Space]
		[Title("Grid Settings")]
		[SerializeField] private Vector2 nodeSize = new Vector2(1, 1);
		[SerializeField] private float xSpacing = .1f;
		[SerializeField] private float ySpacing = .1f;
		[SerializeField, HideInInspector] private Vector2Int size;

		[Title("Setup")]
		[SerializeField] private Array2DGrid grid;

		private float xOffset, yOffset;

		#region Setup

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (gridCells == null) return;

			for (int y = 0; y < size.y; y++)
			{
				for (int x = 0; x < size.x; x++)
				{
					var gridCell = gridCells[x, y];
					if (!gridCell) return;
					if (gridCell.CellType == CellType.Empty) continue;
					if (gridCell.CurrentNode is null) continue;

					SceneVisibilityManager.instance.DisablePicking(gridCell.CurrentNode.GetTransform().gameObject, true);
				}
			}
		}

		[Button]
		private void Setup()
		{
			CleanGrid();

			size = grid.GridSize;
			gridCells = new GridCellMatrix(size.x, size.y);

			xOffset = (nodeSize.x * size.x + xSpacing * (size.x - 1)) / 2f - nodeSize.x / 2f;
			yOffset = (nodeSize.y * size.y + ySpacing * (size.y - 1)) / 2f - nodeSize.y / 2f;
			for (int y = 0; y < size.y; y++)
			{
				for (int x = 0; x < size.x; x++)
				{
					var gridCell = grid.GetCell(x, y);

					var cell = (GridCell)PrefabUtility.InstantiatePrefab(GameManager.Instance.PrefabsSO.GridCellPrefab, cellHolder);
					cell.Setup(x, y, gridCell);
					cell.gameObject.name = x + " - " + y;
					cell.transform.localPosition = new Vector3(x * (nodeSize.x + xSpacing) - xOffset, 0, -y * (nodeSize.y + ySpacing) + yOffset);
					gridCells[x, y] = cell;
				}
			}
		}

		[Button]
		private void CleanGrid()
		{
			cellHolder.DestroyImmediateChildren();
		}

		// [Button, Group("Randomizer")]
		// private void Randomize()
		// {
		// 	var weights = random.Select(x => x.Weight).ToList();
		// 	var amounts = random.Select(x => x.Amount).ToList();
		// 	for (int x = 0; x < size.x; x++)
		// 	{
		// 		for (int y = 0; y < size.y; y++)
		// 		{
		// 			var cell = gridCells[x, y];
		// 			if (!cell) continue;
		//
		// 			cell.ClearNode();
		// 			cell.Random(amounts.WeightedRandom(weights));
		// 			cell.AddNode();
		// 		}
		// 	}
		// }

		[System.Serializable]
		private class Randomizer
		{
			[Range(1, 9)]
			public int Amount = 1;
			public int Weight;
		}
#endif

		[System.Serializable]
		public class GridCellMatrix
		{
			public GridCellArray[] Arrays;
			public GridCell this[int x, int y]
			{
				get => Arrays[x][y];
				set => Arrays[x][y] = value;
			}

			public GridCellMatrix(int index0, int index1)
			{
				Arrays = new GridCellArray[index0];
				for (int i = 0; i < index0; i++)
					Arrays[i] = new GridCellArray(index1);
			}

			public int GetLength(int dimension)
			{
				return dimension switch
				{
					0 => Arrays.Length,
					1 => Arrays[0].Cells.Length,
					_ => 0
				};
			}
		}

		[System.Serializable]
		public class GridCellArray
		{
			public GridCell[] Cells;
			public GridCell this[int index]
			{
				get => Cells[index];
				set => Cells[index] = value;
			}

			public GridCellArray(int index0)
			{
				Cells = new GridCell[index0];
			}
		}

		#endregion
	}
}