using Cysharp.Threading.Tasks;
using GridSystem;
using Interfaces;
using TriInspector;
using UnityEngine;

namespace GamePlay.Obstacles
{
	public abstract class CellObstacle : BaseObstacle, INode
	{
		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public GridCell CurrentGridCell { get; set; }

		public bool IsFalling { get; private set; }
		public float Velocity { get; private set; }

		[Space]
		[SerializeField] private float fallSpeed = 25;
		[SerializeField] private float fallAcceleration = .5f;

		public virtual void Setup(GridCell gridCell)
		{
			CurrentGridCell = gridCell;
		}

		public async UniTask Fall(Vector3 targetPosition)
		{
			await UniTask.WaitUntil(() => !IsFalling);

			IsFalling = true;
			var currentPos = transform.position;
			while (currentPos.z > targetPosition.z)
			{
				Velocity += fallAcceleration;
				Velocity = Velocity >= fallSpeed ? fallSpeed : Velocity;

				currentPos = transform.position;

				currentPos.z -= Velocity * Time.deltaTime;
				transform.position = currentPos;

				await UniTask.Yield();
			}

			currentPos.z = targetPosition.z;
			transform.position = currentPos;
			Velocity = 0;

			IsFalling = false;
		}

		public Transform GetTransform() => transform;

		public void PlaceToGrid(GridCell gridCell)
		{
			if (gridCell)
			{
				CurrentGridCell = gridCell;
				CurrentGridCell.CurrentObstacle = this;
				if (CurrentGridCell.CurrentNode)
					CurrentGridCell.CurrentNode = null;
			}
		}

		public void SwapCell(GridCell gridCell)
		{
			if (CurrentGridCell)
			{
				if (CurrentGridCell.CurrentNode)
					CurrentGridCell.CurrentNode = null;
				CurrentGridCell.CurrentObstacle = null;
			}

			PlaceToGrid(gridCell);
		}

		public override void OnBlastNear(Node node)
		{
			base.OnBlastNear(node);

			// particle

			DestroyObstacle();
		}

		public override void DestroyObstacle()
		{
			if (CurrentGridCell)
			{
				CurrentGridCell.CurrentObstacle = null;
			}

			base.DestroyObstacle();
		}
	}
}