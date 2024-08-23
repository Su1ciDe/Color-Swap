using System.Collections.Generic;
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

		// [field: SerializeField, ReadOnly] public INode CurrentNode { get; set; }

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
				node.Setup(nodeOptions, this);
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

		private IEnumerable<BaseObstacle> GetObstacles()
		{
			const string path = "Assets/_Main/Prefabs/Obstacles";
			var obstacle = EditorUtilities.LoadAllAssetsFromPath<BaseObstacle>(path);
			return obstacle;
		}
#endif

		#endregion

		public void SetNode(Node node)
		{
			CurrentNode = node;
			node.CurrentGridCell = this;
			node.transform.SetParent(transform);
		}
	}
}