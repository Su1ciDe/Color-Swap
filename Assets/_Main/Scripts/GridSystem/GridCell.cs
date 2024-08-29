using System.Collections.Generic;
using System.Linq;
using Fiber.Managers;
using Fiber.Utilities;
using Fiber.Utilities.Extensions;
using GamePlay.Obstacles;
using TriInspector;
using UnityEditor;
using UnityEngine;

namespace GridSystem
{
	[SelectionBase]
	[DeclareBoxGroup("Setup")]
	[DeclareToggleGroup("Setup/Obstacles", Title = "Obstacle")]
	public class GridCell : MonoBehaviour
	{
		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; set; }
		[field: SerializeField, ReadOnly] public CellType CellType { get; set; }
		[field: SerializeField, ReadOnly] public Node CurrentNode { get; set; }
		[field: SerializeField, ReadOnly] public CellObstacle CurrentObstacle { get; set; }
		
		[Title("References")]
		[SerializeField, PropertySpace(spaceAfter: 10)] private Transform nodeHolder;

		#region Setup

#if UNITY_EDITOR
		[Group("Setup"), HideLabel, InlineProperty] [SerializeField] private NodeOption nodeOptions;

		[Group("Setup/Obstacles")] [SerializeField] private bool hasObstacle;
		[Group("Setup/Obstacles"), Dropdown(nameof(GetObstacles))] [SerializeField] private BaseObstacle obstacle;

		[Group("Setup"), Button]
		public void AddNode()
		{
			nodeHolder.DestroyImmediateChildren();

			if (hasObstacle)
			{
				if (obstacle is CellObstacle)
				{
					var obs = (CellObstacle)PrefabUtility.InstantiatePrefab(obstacle, nodeHolder);
					obs.Setup(this);
					CurrentObstacle = obs;
					SceneVisibilityManager.instance.DisablePicking(obs.gameObject, true);
				}
				else if (obstacle is NodeObstacle)
				{
					var node = (Node)PrefabUtility.InstantiatePrefab(GameManager.Instance.PrefabsSO.NodePrefab, nodeHolder);
					node.Setup(nodeOptions, this, obstacle);
					CurrentNode = node;
					SceneVisibilityManager.instance.DisablePicking(node.gameObject, true);
				}
			}
			else
			{
				var node = (Node)PrefabUtility.InstantiatePrefab(GameManager.Instance.PrefabsSO.NodePrefab, nodeHolder);
				node.Setup(nodeOptions.Nodes, this);
				CurrentNode = node;
				SceneVisibilityManager.instance.DisablePicking(node.gameObject, true);
			}
		}

		[Group("Setup"), Button]
		public void ClearNode()
		{
			nodeHolder.DestroyImmediateChildren();
			CurrentNode = null;
		}

		[Group("Setup"), Button]
		private void EmptyCell()
		{
			ClearNode();
			CellType = CellType.Empty;
			transform.GetChild(0).gameObject.SetActive(false);
		}

		public void Setup(int x, int y, CellType cellType)
		{
			Coordinates = new Vector2Int(x, y);
			CellType = cellType;
			if (CellType == CellType.Empty)
				transform.GetChild(0).gameObject.SetActive(false);
		}

		public void Random(List<TileType> tileTypes, List<int> weights)
		{
			const int tileCount = 4;
			var w = new List<int>(weights);
			var types = new List<TileType>(tileTypes);

			var randomCount = UnityEngine.Random.Range(1, tileCount + 1); // count of colors
			var tileDictionary = new Dictionary<TileType, int>();

			for (int i = 0; i < randomCount; i++) // populate dictionary with random colors
			{
				var randomType = types.PickWeightedRandom(ref w);
				if (tileDictionary.TryAdd(randomType, 1))
				{
					if (randomCount.Equals(1)) // if there is only 1 color, increase the count fill all the tiles
						tileDictionary[randomType] = tileCount;
				}
			}

			if (tileDictionary.Count.Equals(2)) // if there are 2 colors, increase the count of each
			{
				var keys = tileDictionary.Keys.ToArray();
				for (var i = 0; i < randomCount; i++)
					tileDictionary[keys[i]]++;
			}
			else if (tileDictionary.Count.Equals(3)) // if there are 3 colors, increase the count of one
			{
				var keys = tileDictionary.Keys.ToArray();
				tileDictionary[keys.RandomItem()]++;
			}

			// Choose a direction
			tileDictionary = UnityEngine.Random.Range(0, 2) == 0
				? new Dictionary<TileType, int>(tileDictionary.OrderBy(x => x.Value))
				: new Dictionary<TileType, int>(tileDictionary.OrderByDescending(x => x.Value));

			var directions = UnityEngine.Random.Range(0, 2) == 0 ? new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down } : new[] { Vector2Int.right, Vector2Int.up, Vector2Int.left };
			var coordinates = Vector2Int.zero;

			int addedTiles = 0;
			var typeArray = tileDictionary.Keys.ToList();
			for (var i = 0; i < randomCount; i++)
			{
				var type = typeArray.PickRandomItem();
				for (int j = 0; j < tileDictionary[type]; j++)
				{
					nodeOptions.Nodes.SetCell(coordinates.x, coordinates.y, type);

					if (addedTiles < tileCount - 1)
					{
						coordinates += directions[addedTiles];
						addedTiles++;
					}
				}
			}
		}

		private IEnumerable<BaseObstacle> GetObstacles()
		{
			const string path = "Assets/_Main/Prefabs/Obstacles";
			var obstacles = EditorUtilities.LoadAllAssetsFromPath<BaseObstacle>(path);
			return obstacles;
		}
#endif

		#endregion

		public void SetNode(Node node)
		{
			CurrentNode = node;
			node.CurrentGridCell = this;
			node.transform.SetParent(nodeHolder);
		}
	}
}