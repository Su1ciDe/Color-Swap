using AYellowpaper.SerializedCollections;
using GridSystem;
using UnityEngine;

namespace ScriptableObjects
{
	[CreateAssetMenu(fileName = "Colors", menuName = "Color Swap/Colors", order = 0)]
	public class ColorsSO : ScriptableObject
	{
		public SerializedDictionary<NodeType, Material> ColorMaterials = new SerializedDictionary<NodeType, Material>();
	}
}