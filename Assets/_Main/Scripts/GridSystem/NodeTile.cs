using Cysharp.Threading.Tasks;
using Fiber.Managers;
using TriInspector;
using UnityEngine;

namespace GridSystem
{
	public class NodeTile : MonoBehaviour
	{
		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public Node Node { get; private set; }
		[field: SerializeField, ReadOnly] public TileType TileType { get; private set; }
		[field: SerializeField, ReadOnly] public Vector2 Size { get; set; }

		[Title("References")]
		[SerializeField] private Renderer modelRenderer;

		public const float BLAST_DURATION = .2F;

		public void Setup(Node node, TileType tileType)
		{
			Node = node;
			TileType = tileType;

			modelRenderer.material = GameManager.Instance.ColorsSO.ColorMaterials[tileType];
		}

		public async UniTask Blast()
		{
			Destroy(gameObject);
		}
	}
}