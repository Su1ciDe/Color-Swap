using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fiber.Managers;
using Fiber.Utilities;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

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
		[SerializeField] private Outline outline;

		private bool blasted;

		public static float BLAST_DURATION = .2F;
		public static float GROW_DURATION = .25f;
		private const float GROW_SCALE = 1.2f;
		private const string BLAST_PARTICLE_NAME = "Blast";

		public static event UnityAction<NodeTile> OnTileBlast;

		private void Awake()
		{
			SetupOutline();
		}

		private void OnDestroy()
		{
			transform.DOKill();
		}

		public void Setup(Node node, TileType tileType)
		{
			Node = node;
			TileType = tileType;

			modelRenderer.material = GameManager.Instance.ColorsSO.ColorMaterials[tileType];
		}

		private void SetupOutline()
		{
			outline.OutlineMode = Outline.Mode.OutlineAll;
			outline.OutlineWidth = 0f;

			var color = GameManager.Instance.ColorsSO.ColorMaterials[TileType].color;
			Color.RGBToHSV(color, out var h, out var s, out var v);
			v -= .25f;
			var newColor = Color.HSVToRGB(h, s, v);
			outline.OutlineColor = newColor;
		}

		public async UniTask Blast()
		{
			if (blasted) return;
			blasted = true;

			// Show outline
			DOVirtual.Float(0, 5, .1f, value => outline.OutlineWidth = value).SetEase(Ease.OutSine);

			// Brighten the color
			var mat = modelRenderer.materials[0];
			var color = mat.color;
			Color.RGBToHSV(color, out var h, out var s, out var v);
			v += .25f;
			var newColor = Color.HSVToRGB(h, s, v);
			mat.DOColor(newColor, .1f).SetEase(Ease.OutSine);

			try
			{
				await UniTask.WaitUntil(() => !Node.IsRearranging, cancellationToken: this.GetCancellationTokenOnDestroy());
				await UniTask.Yield(this.GetCancellationTokenOnDestroy());
				await UniTask.WaitUntil(() => !Node.IsFalling, cancellationToken: this.GetCancellationTokenOnDestroy());
				await UniTask.Yield(this.GetCancellationTokenOnDestroy());

				Node.OnTileBlast(this);

				// transform.DOShakeRotation(BLAST_DURATION, 10 * Vector3.up, 25, 0, false, ShakeRandomnessMode.Harmonic).SetEase(Ease.InQuart);
				var seq = DOTween.Sequence();

				seq.Append(transform.DOScale(GROW_SCALE * transform.localScale, BLAST_DURATION).SetEase(Ease.OutExpo));
				seq.AppendCallback(() =>
				{
					var particle = ParticlePooler.Instance.Spawn(BLAST_PARTICLE_NAME, transform.position).GetComponent<BlastParticle>();
					particle.Setup(modelRenderer.material.color);
				});
				seq.Append(transform.DOScale(0, BLAST_DURATION));
				seq.AppendCallback(() =>
				{
					OnTileBlast?.Invoke(this);

					DestroyImmediate(gameObject);
					Node.Rearrange();
				});
				seq.AsyncWaitForCompletion().AsUniTask();
				seq.SetLink(gameObject);
			}
			catch (OperationCanceledException e)
			{
			}
		}

		public async void Grow(Vector3 scale, Vector3 position)
		{
			transform.DOComplete();
			transform.DOLocalMove(position, GROW_DURATION).SetEase(Ease.OutExpo);
			await transform.DOScale(scale, GROW_DURATION).SetEase(Ease.OutExpo).AsyncWaitForCompletion();
		}
	}
}