using UnityEngine;

namespace Utilities
{
	public struct Direction
	{
		public static Vector2Int Up = new Vector2Int(0, -1);
		public static Vector2Int Down = new Vector2Int(0, 1);
		public static Vector2Int Left = new Vector2Int(-1, 0);
		public static Vector2Int Right = new Vector2Int(1, 0);

		public static Vector2Int[] Directions;

		static Direction()
		{
			Directions = new[] { Up, Down, Left, Right };
		}
	}
}