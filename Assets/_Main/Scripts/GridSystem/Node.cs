using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fiber.Managers;
using Fiber.Utilities.Extensions;
using GamePlay.Obstacles;
using Interfaces;
using TriInspector;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace GridSystem
{
	public class Node : MonoBehaviour, INode
	{
		public bool IsFalling { get; private set; }
		public bool IsRearranging { get; private set; }

		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public GridCell CurrentGridCell { get; set; }

		[field: SerializeField, ReadOnly] public NodeObstacle Obstacle { get; set; }
		[field: SerializeField, ReadOnly] public NodeTileMatrix Tiles { get; set; }
		[field: SerializeField, ReadOnly] public SerializedDictionary<TileType, List<Vector2Int>> TilesDictionary { get; set; } = new SerializedDictionary<TileType, List<Vector2Int>>();

		[Title("Parameters")]
		[SerializeField] private float jumpPower = 5;
		[SerializeField] private float jumpDuration = .5f;
		[Space]
		[SerializeField] private float fallSpeed = 25;
		[SerializeField] private float fallAcceleration = .5f;

		[Title("References")]
		[SerializeField] private Transform tileHolder;

		public float Velocity { get; private set; }

		private const float TILE_SIZE = .5F;

		public Tween JumpTo(Vector3 position)
		{
			return transform.DOJump(position, jumpPower, 1, jumpDuration);
		}

		public void PlaceToGrid(GridCell gridCell)
		{
			if (gridCell)
			{
				CurrentGridCell = gridCell;
				CurrentGridCell.CurrentNode = this;
				gridCell.SetNode(this);
			}
		}

		public void SwapCell(GridCell gridCell)
		{
			if (CurrentGridCell)
				CurrentGridCell.CurrentNode = null;

			PlaceToGrid(gridCell);
		}

		public void OnObstacleDestroyed()
		{
			Obstacle = null;
			GridManager.Instance.AddToFallingNodes(this);
		}

		public void OnTileBlast(NodeTile nodeTile)
		{
			if (!TilesDictionary.TryGetValue(nodeTile.TileType, out var coordinates)) return;
			for (var i = 0; i < coordinates.Count; i++)
				Tiles[coordinates[i].x, coordinates[i].y] = null;

			TilesDictionary.Remove(nodeTile.TileType);

			CheckObstacles();
		}

		public async void Rearrange()
		{
			if (IsRearranging) return;
			IsRearranging = true;
			var tileCount = GetTileCount();
			if (tileCount.Equals(0))
			{
				if (CurrentGridCell)
				{
					CurrentGridCell.CurrentNode = null;
					CurrentGridCell = null;
				}

				DestroyImmediate(gameObject);

				return;
			}

			await UniTask.WaitForSeconds(0.1f);

			for (int x = 0; x < Tiles.GetLength(0); x++)
			{
				for (int y = 0; y < Tiles.GetLength(1); y++)
				{
					var tile = Tiles[x, y];
					if (!tile) continue;
					if (TilesDictionary.Count.Equals(1))
					{
						tile.Grow(new Vector3(1, 0.5f, 1), Vector3.zero);

						for (int x1 = 0; x1 < Tiles.GetLength(0); x1++)
						{
							for (int y1 = 0; y1 < Tiles.GetLength(1); y1++)
							{
								TilesDictionary[tile.TileType].AddIfNotContains(new Vector2Int(x1, y1));
								Tiles[x1, y1] = tile;
							}
						}
					}

					if (TilesDictionary[tile.TileType].Count < 2)
					{
						if (x + 1 < 2 && !Tiles[x + 1, y])
						{
							tile.Grow(new Vector3(tile.transform.localScale.x * 2, tile.transform.localScale.y, tile.transform.localScale.z),
								new Vector3(0, tile.transform.localPosition.y, tile.transform.localPosition.z));
							TilesDictionary[tile.TileType].AddIfNotContains(new Vector2Int(x + 1, y));
							Tiles[x + 1, y] = tile;
						}

						if (y + 1 < 2 && !Tiles[x, y + 1])
						{
							tile.Grow(new Vector3(tile.transform.localScale.x, tile.transform.localScale.y, tile.transform.localScale.z * 2),
								new Vector3(tile.transform.localPosition.x, tile.transform.localPosition.y, 0));
							TilesDictionary[tile.TileType].AddIfNotContains(new Vector2Int(x, y + 1));
							Tiles[x, y + 1] = tile;
						}

						if (x - 1 >= 0 && !Tiles[x - 1, y])
						{
							tile.Grow(new Vector3(tile.transform.localScale.x * 2, tile.transform.localScale.y, tile.transform.localScale.z),
								new Vector3(0, tile.transform.localPosition.y, tile.transform.localPosition.z));
							TilesDictionary[tile.TileType].AddIfNotContains(new Vector2Int(x - 1, y));
							Tiles[x - 1, y] = tile;
						}

						if (y - 1 >= 0 && !Tiles[x, y - 1])
						{
							tile.Grow(new Vector3(tile.transform.localScale.x, tile.transform.localScale.y, tile.transform.localScale.z * 2),
								new Vector3(tile.transform.localPosition.x, tile.transform.localPosition.y, 0));
							TilesDictionary[tile.TileType].AddIfNotContains(new Vector2Int(x, y - 1));
							Tiles[x, y - 1] = tile;
						}
					}
				}
			}

			IsRearranging = false;

			await UniTask.Yield();
			await UniTask.WaitForSeconds(NodeTile.GROW_DURATION);
			
			GridManager.Instance.AddToFallingNodes(this);
		}

		public async UniTask Fall(Vector3 position)
		{
			if (position.Equals(transform.position)) return;

			await UniTask.WaitUntil(() => !IsFalling, cancellationToken: this.GetCancellationTokenOnDestroy());
			IsFalling = true;

			var currentPos = transform.position;
			while (gameObject && currentPos.z > position.z)
			{
				Velocity += fallAcceleration;
				Velocity = Velocity >= fallSpeed ? fallSpeed : Velocity;

				currentPos = transform.position;

				currentPos.z -= Velocity * Time.deltaTime;
				transform.position = currentPos;

				await UniTask.Yield(this.GetCancellationTokenOnDestroy());
			}

			currentPos.z = position.z;
			transform.position = currentPos;
			Velocity = 0;

			IsFalling = false;
		}

		private void CheckObstacles()
		{
			var dirCount = Direction.Directions.Length;
			for (int i = 0; i < dirCount; i++)
			{
				var dir = CurrentGridCell.Coordinates + Direction.Directions[i];
				if (!GridManager.Instance.IsInGridBoundaries(dir)) continue;
				var cell = GridManager.Instance.GetCell(dir);
				if (!cell) continue;

				if (cell.CurrentNode && cell.CurrentNode.Obstacle)
				{
					cell.CurrentNode.Obstacle.OnBlastNear(this);
				}

				if (cell.CurrentObstacle)
				{
					cell.CurrentObstacle.OnBlastNear(this);
				}
			}
		}

		public Transform GetTransform() => transform;

		#region Helpers

		public NodeTile GetTile(int x, int y)
		{
			return Tiles[x, y];
		}

		public NodeTile GetTile(Vector2Int coordinate)
		{
			return Tiles[coordinate.x, coordinate.y];
		}

		public IEnumerable<NodeTile> GetAllTiles()
		{
			for (int x = 0; x < Tiles.GetLength(0); x++)
			{
				for (int y = 0; y < Tiles.GetLength(1); y++)
					yield return Tiles[x, y];
			}
		}

		public int GetTileCount()
		{
			int count = 0;
			for (int x = 0; x < Tiles.GetLength(0); x++)
			{
				for (int y = 0; y < Tiles.GetLength(1); y++)
				{
					if (Tiles[x, y])
						count++;
				}
			}

			return count;
		}

		#endregion

		#region Setup

		public void Setup(Array2DNode nodeArray, GridCell gridCell)
		{
			CurrentGridCell = gridCell;

			var size = nodeArray.GridSize;
			var xOffset = TILE_SIZE * size.x * (size.x - 1) / 2f - TILE_SIZE / 2f;
			var yOffset = TILE_SIZE * size.y * (size.y - 1) / 2f - TILE_SIZE / 2f;

			Tiles = new NodeTileMatrix(size.x, size.y);
			for (int x = 0; x < size.x; x++)
			{
				for (int y = 0; y < size.y; y++)
				{
					var nodeType = nodeArray.GetCell(x, y);
					if (x - 1 >= 0 && nodeType == Tiles[x - 1, y].TileType)
					{
						var leftTile = Tiles[x - 1, y];
						leftTile.transform.localScale = new Vector3(TILE_SIZE * 2, leftTile.transform.localScale.y, leftTile.transform.localScale.z);
						leftTile.transform.localPosition = new Vector3(0, leftTile.transform.localPosition.y, leftTile.transform.localPosition.z);
						leftTile.Size = new Vector2(leftTile.transform.localScale.x, leftTile.transform.localScale.z);
						Tiles[x, y] = leftTile;
						TilesDictionary[nodeType].Add(new Vector2Int(x, y));
						continue;
					}

					if (y - 1 >= 0 && nodeType == Tiles[x, y - 1].TileType)
					{
						var upTile = Tiles[x, y - 1];
						upTile.transform.localScale = new Vector3(upTile.transform.localScale.x, upTile.transform.localScale.y, TILE_SIZE * 2);
						upTile.transform.localPosition = new Vector3(upTile.transform.localPosition.x, upTile.transform.localPosition.y, 0);
						upTile.Size = new Vector2(upTile.transform.localScale.x, upTile.transform.localScale.z);
						Tiles[x, y] = upTile;
						TilesDictionary[nodeType].Add(new Vector2Int(x, y));
						continue;
					}

					NodeTile tile = null;
#if UNITY_EDITOR
					if (Application.isPlaying)
						tile = Instantiate(GameManager.Instance.PrefabsSO.NodeTilePrefab, tileHolder);
					else
						tile = (NodeTile)PrefabUtility.InstantiatePrefab(GameManager.Instance.PrefabsSO.NodeTilePrefab, tileHolder);
#else
						tile = Instantiate(GameManager.Instance.PrefabsSO.NodeTilePrefab, tileHolder);
#endif
					tile.transform.localScale = TILE_SIZE * Vector3.one;
					tile.transform.localPosition = new Vector3(x * TILE_SIZE + -xOffset, 0, -y * TILE_SIZE + yOffset);
					tile.Size = TILE_SIZE * Vector2.one;
					tile.Setup(this, nodeType);
					TilesDictionary.Add(nodeType, new List<Vector2Int> { new Vector2Int(x, y) });

					Tiles[x, y] = tile;
				}
			}
		}
#if UNITY_EDITOR
		public void Setup(NodeOption nodeOption, GridCell gridCell, BaseObstacle obstacle)
		{
			Obstacle = (NodeObstacle)PrefabUtility.InstantiatePrefab(obstacle, transform);
			Obstacle.Setup(this);

			Setup(nodeOption.Nodes, gridCell);
		}
#endif

		[System.Serializable]
		public class NodeTileMatrix
		{
			public NodeTileArray[] Arrays;
			public NodeTile this[int x, int y]
			{
				get => Arrays[x][y];
				set => Arrays[x][y] = value;
			}

			public NodeTileMatrix(int index0, int index1)
			{
				Arrays = new NodeTileArray[index0];
				for (int i = 0; i < index0; i++)
					Arrays[i] = new NodeTileArray(index1);
			}

			public int GetLength(int dimension)
			{
				return dimension switch
				{
					0 => Arrays.Length,
					1 => Arrays[0].Tiles.Length,
					_ => 0
				};
			}
		}

		[System.Serializable]
		public class NodeTileArray
		{
			public NodeTile[] Tiles;
			public NodeTile this[int index]
			{
				get => Tiles[index];
				set => Tiles[index] = value;
			}

			public NodeTileArray(int index0)
			{
				Tiles = new NodeTile[index0];
			}
		}

		#endregion
	}
}