using Array2DEditor;
using UnityEditor;

namespace GridSystem
{
	public class Array2DNodeDrawer
	{
		[CustomPropertyDrawer(typeof(Array2DNode))]
		public class Array2DExampleEnumDrawer : Array2DEnumDrawer<TileType>
		{
		}
	}
}