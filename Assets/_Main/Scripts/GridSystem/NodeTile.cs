using Fiber.Managers;
using TriInspector;
using UnityEngine;

namespace GridSystem
{
	public class NodeTile : MonoBehaviour
	{
		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public NodeType NodeType { get; private set; }

		[Title("References")]
		[SerializeField] private Renderer modelRenderer;

		public const float BLAST_DURATION = .2F;

		public void Setup(NodeType nodeType)
		{
			NodeType = nodeType;

			modelRenderer.material = GameManager.Instance.ColorsSO.ColorMaterials[nodeType];
		}
	}
}