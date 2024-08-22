using GridSystem;
using UnityEngine;

namespace ScriptableObjects
{
	[CreateAssetMenu(fileName = "Prefabs", menuName = "Color Swap/Prefabs", order = 0)]
	public class PrefabsSO : ScriptableObject
	{
		public Node NodePrefab;
		public NodeTile NodeTilePrefab;
		public GridCell GridCellPrefab;
	}
}