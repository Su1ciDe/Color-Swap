using AYellowpaper.SerializedCollections;
using GridSystem;
using UnityEngine;

namespace ScriptableObjects
{
	[CreateAssetMenu(fileName = "Colors", menuName = "Color Swap/Colors", order = 0)]
	public class ColorsSO : ScriptableObject
	{
		public SerializedDictionary<TileType, Material> ColorMaterials = new SerializedDictionary<TileType, Material>();
		public SerializedDictionary<TileType, Color> Colors = new SerializedDictionary<TileType, Color>();
	}
}