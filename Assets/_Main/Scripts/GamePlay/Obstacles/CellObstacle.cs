using Cysharp.Threading.Tasks;
using GridSystem;
using Interfaces;
using UnityEngine;

namespace GamePlay.Obstacles
{
	public abstract class CellObstacle : BaseObstacle, INode
	{
		public bool IsFalling { get; private set; }
		public GridCell CurrentGridCell { get; set; }

		public float Velocity { get; private set; }

		private const float FALL_SPEED = 20f;
		private const float ACCELERATION = .5f;

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
				Velocity += ACCELERATION;
				Velocity = Velocity >= FALL_SPEED ? FALL_SPEED : Velocity;

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
	}
}