using Array2DEditor;
using UnityEditor;

namespace GridSystem
{
	public class Array2DGridDrawer
	{
		[CustomPropertyDrawer(typeof(Array2DGrid))]
		public class Array2DExampleEnumDrawer : Array2DEnumDrawer<CellType>
		{
		}
	}
}