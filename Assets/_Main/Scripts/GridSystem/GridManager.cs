using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fiber.Managers;
using Fiber.Utilities;
using Fiber.AudioSystem;
using Fiber.LevelSystem;
using Fiber.Utilities.Extensions;
using Interfaces;
using Lofelt.NiceVibrations;
using TriInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace GridSystem
{
	[DeclareFoldoutGroup("Randomizer")]
	public class GridManager : Singleton<GridManager>
	{
		public bool IsBusy { get; private set; }

		[Title("Properties")]
		[SerializeField, ReadOnly] private GridCellMatrix gridCells;
		public GridCellMatrix GridCells => gridCells;

		[Title("References")]
		[SerializeField] private Transform cellHolder;

		[Title("Grid Settings")]
		[SerializeField] private Vector2 nodeSize = new Vector2(1, 1);
		[SerializeField] private float xSpacing = .1f;
		[SerializeField] private float ySpacing = .1f;
		[SerializeField, HideInInspector] private Vector2Int size;

		[Title("Setup")]
		[SerializeField] private Array2DGrid grid;
		[SerializeField] private Spawner[] spawner;

		private readonly List<Node> nodesToSpawn = new List<Node>();
		private List<int> randomWeights = new List<int>();
		private bool isFirstSpawn = true;

		private readonly List<Node> spawnerNodes = new List<Node>();
		private List<int> spawnerRandomWeights;

		private float xOffset, yOffset;

		private const int BLAST_COUNT = 3;

		// private CancellationTokenSource destroyCancellation = new CancellationTokenSource();
		// private CancellationTokenSource inputCancellation = new CancellationTokenSource();

		public static event UnityAction<int> OnBlast;
		public static event UnityAction OnAfterBlast;

		private void Awake()
		{
			spawnerRandomWeights = spawner.Select(x => x.Weight).ToList();
			foreach (var spawn in spawner)
			{
				var spawnerNode = Instantiate(GameManager.Instance.PrefabsSO.NodePrefab, transform);
				spawnerNode.Setup(spawn.Nodes, null);
				spawnerNode.gameObject.SetActive(false);
				spawnerNodes.Add(spawnerNode);
			}

			CreateNewNodes();
		}

		private void OnDestroy()
		{
			StopAllCoroutines();

			// destroyCancellation.Cancel();
			// destroyCancellation.Dispose();
		}

		private Coroutine busyCoroutine;

		private IEnumerator BusyCoroutine()
		{
			IsBusy = true;

			yield return new WaitUntil(() => !IsAnyNodeFalling());
			yield return new WaitForSeconds(0.75f);
			yield return new WaitUntil(() => !IsAnyNodeFalling());

			IsBusy = false;
		}

		private void Busy()
		{
			if (busyCoroutine is not null)
			{
				StopCoroutine(busyCoroutine);
				busyCoroutine = null;
			}

			busyCoroutine = StartCoroutine(BusyCoroutine());
		}

		// private async void Busy()
		// {
		// 	try
		// 	{
		// 		IsBusy = true;
		// 		await UniTask.WaitUntil(() => !IsAnyNodeFalling(), cancellationToken: inputCancellation.Token);
		// 		await UniTask.WaitForSeconds(0.75f, cancellationToken: inputCancellation.Token);
		// 		await UniTask.WaitUntil(() => !IsAnyNodeFalling(), cancellationToken: inputCancellation.Token);
		// 		IsBusy = false;
		// 	}
		// 	catch (OperationCanceledException _)
		// 	{
		// 	}
		// }

		#region Match3

		public async UniTask OnSwap(GridCell gridCell)
		{
			IsBusy = true;

			await CheckMatch3(gridCell.CurrentNode);
		}

		public async UniTask CheckMatch3(Node node)
		{
			if (StateManager.Instance.CurrentState != GameState.OnStart) return;
			
			UniTask? task = null;

			var tempTiles = new Dictionary<TileType, List<Vector2Int>>(node.TilesDictionary);
			foreach (var (tileType, tileCoordinates) in tempTiles)
			{
				var traversedNodes = new List<Node>();
				var match3 = FindMatch3(node, tileType, ref traversedNodes);

				if (match3.nodes.Count >= BLAST_COUNT)
				{
					if (task is null)
						task = Blast(match3.tiles);
					else
						Blast(match3.tiles);
				}
			}

			if (task is null)
			{
				Busy();
				return;
			}

			if (busyCoroutine is not null)
			{
				StopCoroutine(busyCoroutine);
				busyCoroutine = null;
			}

			try
			{
				await ((UniTask)task);

				await UniTask.Yield();
				await FallAndFill();
			}
			catch (OperationCanceledException e)
			{
			}
		}

		private (List<Node> nodes, List<NodeTile> tiles) FindMatch3(Node node, TileType tileType, ref List<Node> traversedNodes)
		{
			var nodeList = new List<Node>();
			var tileList = new List<NodeTile>();
			if (traversedNodes.Contains(node))
				return (nodeList, tileList);

			traversedNodes.Add(node);

			foreach (var (_tileType, tileCoordinates) in node.TilesDictionary)
			{
				if (tileType != _tileType) continue;

				for (var i = 0; i < tileCoordinates.Count; i++)
				{
					tileList.Add(node.GetTile(tileCoordinates[i]));
					nodeList.AddIfNotContains(node);

					var dirs = GetDirections(tileCoordinates[i]);
					if (dirs.Count <= 0) continue;
					for (var j = 0; j < dirs.Count; j++)
					{
						var neighbourNodeCoordinate = node.CurrentGridCell.Coordinates + dirs[j];
						if (!IsInGridBoundaries(neighbourNodeCoordinate)) continue;

						var neighbourNode = GetCell(neighbourNodeCoordinate).CurrentNode;
						if (neighbourNode is null) continue;
						if (neighbourNode.Obstacle) continue;

						var coor = tileCoordinates[i] - dirs[j];
						var neighbourTile = neighbourNode.GetTile(coor);
						if (!neighbourTile) continue;
						if (neighbourTile.TileType != tileType) continue;

						var match3 = FindMatch3(neighbourNode, tileType, ref traversedNodes);
						tileList.AddRangeIfNotContains(match3.tiles);
						nodeList.AddRangeIfNotContains(match3.nodes);
					}
				}
			}

			return (nodeList, tileList);
		}

		#endregion

		private async UniTask Blast(List<NodeTile> nodeTiles)
		{
			IsBusy = true;

			await UniTask.WaitUntil(() => !IsAnyNodeFalling());
			await UniTask.WaitForSeconds(0.25f);

			IsBusy = true;

			var node = nodeTiles[0].Node;
			fallingNodes.Add(node);

			for (var i = 0; i < nodeTiles.Count; i++)
			{
				if (nodeTiles[i])
					nodeTiles[i].Blast();
			}

			await UniTask.WaitUntil(() => !node.IsRearranging);
			await UniTask.Yield();
			await UniTask.WaitUntil(() => !node.IsFalling);
			await UniTask.Yield();

			HapticManager.Instance.PlayHaptic(HapticPatterns.PresetType.RigidImpact);
			AudioManager.Instance.PlayAudio(AudioName.Blast);

			await UniTask.WaitForSeconds(NodeTile.BLAST_DURATION * 2, cancellationToken: destroyCancellationToken);
			await UniTask.Yield();

			OnBlast?.Invoke(nodeTiles.Count);
		}

		private async UniTask FallAndFill()
		{
			Fall();

			try
			{
				await UniTask.Yield();
				await Fill().AttachExternalCancellation(destroyCancellationToken);
				await UniTask.Yield();
			}
			catch (OperationCanceledException _)
			{
			}

			CheckBlastAfterFalling();
		}

		private readonly List<INode> fallingNodes = new List<INode>();

		private async UniTask Fall()
		{
			IsBusy = true;

			UniTask? task = null;
			for (int x = 0; x < size.x; x++)
			{
				for (int y = size.y - 1; y >= 0; y--)
				{
					var cell = gridCells[x, y];
					if (!cell) continue;
					if (cell.CellType == CellType.Empty) continue;
					INode node;
					if (cell.CurrentNode)
						node = cell.CurrentNode;
					else if (cell.CurrentObstacle)
						node = cell.CurrentObstacle;
					else
						continue;

					// Check if there is any empty cell under
					int emptyY = GetFirstEmptyRow(x, y);
					if (emptyY < 0) continue;
					if (emptyY.Equals(node.CurrentGridCell.Coordinates.y)) continue;
					node.SwapCell(gridCells[x, emptyY]);

					if (task is null)
						task = node.Fall(gridCells[x, emptyY].transform.position);
					else
						node.Fall(gridCells[x, emptyY].transform.position);

					if (node is Node fallingNode)
						AddToFallingNodes(fallingNode);
				}
			}

			if (task is not null)
				await (UniTask)task;
		}

		private async UniTask Fill()
		{
			for (int x = 0; x < size.x; x++)
			{
				var emptyRowCount = GetEmptyRows(x);
				for (int i = 0; i < emptyRowCount; i++)
				{
					var emptyCellY = GetFirstEmptyRow(x, 0);
					if (emptyCellY < 0) continue;

					var node = SpawnNode();
					node.transform.position = gridCells[x, 0].transform.position + (i + 1) * 1.05f * Vector3.forward;

					node.PlaceToGrid(gridCells[x, emptyCellY]);
					node.Fall(gridCells[x, emptyCellY].transform.position);
					fallingNodes.Add(node);
				}
			}

			try
			{
				await UniTask.WaitUntil(() => !IsAnyNodeFalling(), cancellationToken: destroyCancellationToken);
			}
			catch (OperationCanceledException _)
			{
			}
		}

		public void AddToFallingNodes(Node node)
		{
			fallingNodes.AddIfNotContains(node);
		}

		private async void CheckBlastAfterFalling()
		{
			IsBusy = true;

			await UniTask.WaitUntil(() => !IsAnyNodeRearranging());

			for (int i = 0; i < fallingNodes.Count; i++)
			{
				if (fallingNodes[i] is not null && fallingNodes[i] is Node node && !node.Obstacle)
					CheckMatch3(node);
			}

			fallingNodes.Clear();
		}

		#region Spawn

		private Node SpawnNode()
		{
			if (nodesToSpawn.Count <= 0)
			{
				CreateNewNodes();
				isFirstSpawn = false;
			}

			Node node = null;
			if (isFirstSpawn)
			{
				node = nodesToSpawn[0];
				nodesToSpawn.RemoveAt(0);
			}
			else
			{
				node = nodesToSpawn.PickWeightedRandom(ref randomWeights);
			}

			node.gameObject.SetActive(true);
			return node;
		}

		private void CreateNewNodes()
		{
			for (var i = 0; i < spawnerNodes.Count; i++)
			{
				var node = Instantiate(spawnerNodes[i], transform);
				nodesToSpawn.Add(node);
			}

			randomWeights = new List<int>(spawnerRandomWeights);
		}

		[DeclareHorizontalGroup("Spawner")]
		[System.Serializable]
		private class Spawner
		{
			[Group("Spawner")] public Array2DNode Nodes;
			[Group("Spawner")] public int Weight;
		}

		#endregion

		#region Helpers

		public int GetEmptyRows(int x)
		{
			int count = 0;
			for (int y = 0; y < size.y; y++)
			{
				if (gridCells[x, y].CellType != CellType.Empty && gridCells[x, y].CurrentNode is null && gridCells[x, y].CurrentObstacle is null)
					count++;
			}

			return count;
		}

		public bool IsAnyNodeFalling()
		{
			for (int x = 0; x < size.x; x++)
			{
				for (int y = 0; y < size.y; y++)
				{
					if (gridCells[x, y] && ((gridCells[x, y].CurrentNode && gridCells[x, y].CurrentNode.IsFalling) || (gridCells[x, y].CurrentObstacle && gridCells[x, y].CurrentObstacle.IsFalling)))
						return true;
				}
			}

			return false;
		}

		public bool IsAnyNodeRearranging()
		{
			for (int x = 0; x < size.x; x++)
			{
				for (int y = 0; y < size.y; y++)
				{
					if (gridCells[x, y] && (gridCells[x, y].CurrentNode && gridCells[x, y].CurrentNode.IsRearranging))
						return true;
				}
			}

			return false;
		}

		private int GetFirstEmptyRow(int x, int y, bool findUnderObstacle = false)
		{
			int yy = -1;
			bool foundFirst = false;
			if (findUnderObstacle)
				y++;
			for (int i = size.y - 1; i >= y; i--)
			{
				if (!foundFirst && gridCells[x, i].CellType != CellType.Empty && gridCells[x, i].CurrentNode is null && gridCells[x, i].CurrentObstacle is null)
				{
					foundFirst = true;
					yy = i;
				}
			}

			return yy;
		}

		private int GetFirstEmptyRow(Vector2Int coordinates, bool findUnderObstacle = false)
		{
			return GetFirstEmptyRow(coordinates.x, coordinates.y, findUnderObstacle);
		}

		public List<Vector2Int> GetDirections(int x, int y)
		{
			var list = new List<Vector2Int>();
			if (x.Equals(0))
			{
				list.Add(Direction.Left);
			}
			else if (x.Equals(1))
			{
				list.Add(Direction.Right);
			}

			if (y.Equals(0))
			{
				list.Add(Direction.Up);
			}
			else if (y.Equals(1))
			{
				list.Add(Direction.Down);
			}

			return list;
		}

		public List<Vector2Int> GetDirections(Vector2Int coordinate)
		{
			return GetDirections(coordinate.x, coordinate.y);
		}

		public GridCell GetCell(Vector2Int coordinates)
		{
			return gridCells[coordinates.x, coordinates.y];
		}

		public bool IsInGridBoundaries(Vector2Int coordinates)
		{
			return coordinates.x >= 0 && coordinates.x < size.x && coordinates.y >= 0 && coordinates.y < size.y;
		}

		#endregion

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

		[Group("Randomizer")] [SerializeField] private Randomizer[] random;

		[Group("Randomizer"), Button]
		private void Randomize()
		{
			var colors = random.Select(x => x.TileType).ToList();
			var weights = random.Select(x => x.Weight).ToList();
			for (int x = 0; x < size.x; x++)
			{
				for (int y = 0; y < size.y; y++)
				{
					var cell = gridCells[x, y];
					if (!cell) continue;
					if (cell.CellType == CellType.Empty) continue;

					cell.ClearNode();
					cell.Random(colors, weights);
					cell.AddNode();
				}
			}
		}

		[DeclareHorizontalGroup("Random")]
		[System.Serializable]
		private class Randomizer
		{
			[Group("Random")] public TileType TileType;
			[Group("Random")] public int Weight;
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