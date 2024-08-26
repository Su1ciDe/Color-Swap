using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fiber.Managers;
using Fiber.Utilities;
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

		public static float BLAST_DURATION = .15F;
		private const float GROW_DURATION = .25f;
		private const string BLAST_PARTICLE_NAME = "Blast";

		public void Setup(Node node, TileType tileType)
		{
			Node = node;
			TileType = tileType;

			modelRenderer.material = GameManager.Instance.ColorsSO.ColorMaterials[tileType];
		}

		public async UniTask Blast()
		{
			await UniTask.WaitUntil(() => !Node.IsRearranging);
			await UniTask.Yield();

			Node.OnTileBlast(this);
			await transform.DOScale(1.25f, BLAST_DURATION).SetEase(Ease.OutExpo).OnComplete(() =>
			{
				// ParticlePooler.Instance.Spawn(BLAST_PARTICLE_NAME, transform.position);
				Destroy(gameObject);
			}).AsyncWaitForCompletion();

			Node.Rearrange();
		}

		public async void Grow(Vector3 scale, Vector3 position)
		{
			transform.DOComplete();
			transform.DOLocalMove(position, GROW_DURATION).SetEase(Ease.OutExpo);
			await transform.DOScale(scale, GROW_DURATION).SetEase(Ease.OutExpo).AsyncWaitForCompletion();
		}
	}
}