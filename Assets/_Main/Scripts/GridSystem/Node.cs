using Fiber.Managers;
using GamePlay.Obstacles;
using Interfaces;
using TriInspector;
using UnityEditor;
using UnityEngine;

namespace GridSystem
{
	public class Node : MonoBehaviour, INode
	{
		public bool IsFalling { get; set; }

		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public GridCell CurrentGridCell { get; set; }

		[field: SerializeField, ReadOnly] public NodeObstacle Obstacle { get; set; }
		[field: SerializeField, ReadOnly] public NodeTileMatrix Tiles { get; set; }

		[Title("Parameters")]
		[SerializeField] private float jumpPower = 5;
		[SerializeField] private float jumpDuration = .5f;

		[Title("References")]
		[SerializeField] private Transform tileHolder;

		private float velocity;

		private const float FALL_SPEED = 20f;
		private const float ACCELERATION = .5f;
		private const float TILE_SIZE = .5F;

		public Transform GetTransform() => transform;

		#region Setup

#if UNITY_EDITOR

		public void Setup(NodeOption nodeOption, GridCell gridCell)
		{
			CurrentGridCell = gridCell;

			var size = nodeOption.Nodes.GridSize;
			var xOffset = TILE_SIZE * size.x * (size.x - 1) / 2f - TILE_SIZE / 2f;
			var yOffset = TILE_SIZE * size.y * (size.y - 1) / 2f - TILE_SIZE / 2f;

			Tiles = new NodeTileMatrix(size.x, size.y);
			for (int x = 0; x < size.x; x++)
			{
				for (int y = 0; y < size.y; y++)
				{
					var nodeType = nodeOption.Nodes.GetCell(x, y);
					if (x - 1 >= 0 && nodeType == Tiles[x - 1, y].NodeType)
					{
						var leftTile = Tiles[x - 1, y];
						leftTile.transform.localScale = new Vector3(TILE_SIZE * 2, leftTile.transform.localScale.y, leftTile.transform.localScale.z);
						leftTile.transform.localPosition = new Vector3(0, leftTile.transform.localPosition.y, leftTile.transform.localPosition.z);
						Tiles[x, y] = leftTile;
						continue;
					}

					if (y - 1 >= 0 && nodeType == Tiles[x, y - 1].NodeType)
					{
						var upTile = Tiles[x, y - 1];
						upTile.transform.localScale = new Vector3(upTile.transform.localScale.x, upTile.transform.localScale.y, TILE_SIZE * 2);
						upTile.transform.localPosition = new Vector3(upTile.transform.localPosition.x, upTile.transform.localPosition.y, 0);
						Tiles[x, y] = upTile;
						continue;
					}

					var tile = (NodeTile)PrefabUtility.InstantiatePrefab(GameManager.Instance.PrefabsSO.NodeTilePrefab, tileHolder);
					tile.transform.localScale = TILE_SIZE * Vector3.one;
					tile.transform.localPosition = new Vector3(x * TILE_SIZE + -xOffset, 0, -y * TILE_SIZE + yOffset);
					tile.Setup(nodeType);

					Tiles[x, y] = tile;
				}
			}
		}

		public void Setup(NodeOption nodeOption, GridCell gridCell, BaseObstacle obstacle)
		{
			Obstacle = (NodeObstacle)obstacle;
			Obstacle.Setup(this);

			Setup(nodeOption, gridCell);
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