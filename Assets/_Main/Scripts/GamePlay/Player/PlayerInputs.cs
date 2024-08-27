using Fiber.Managers;
using Fiber.Utilities;
using GridSystem;
using Interfaces;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.Player
{
	public class PlayerInputs : MonoBehaviour
	{
		public bool CanInput { get; set; }

		[SerializeField] private LayerMask inputLayer;

		private GridCell selectedCell;

		public static event UnityAction<GridCell> OnDown;
		public static event UnityAction<GridCell> OnMove;
		public static event UnityAction<GridCell> OnUp;

		private void Awake()
		{
			Input.multiTouchEnabled = false;
		}

		private void OnEnable()
		{
			LevelManager.OnLevelStart += OnLevelStarted;
			LevelManager.OnLevelWin += OnLevelWon;
			LevelManager.OnLevelLose += OnLevelLost;

			LeanTouch.OnFingerDown += OnFingerDown;
			LeanTouch.OnFingerUpdate += OnFingerMove;
			LeanTouch.OnFingerUp += OnFingerUp;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelStart -= OnLevelStarted;
			LevelManager.OnLevelWin -= OnLevelWon;
			LevelManager.OnLevelLose -= OnLevelLost;

			LeanTouch.OnFingerDown -= OnFingerDown;
			LeanTouch.OnFingerUpdate -= OnFingerMove;
			LeanTouch.OnFingerUp -= OnFingerUp;
		}

		private void OnFingerDown(LeanFinger finger)
		{
			if (!CanInput) return;
			if (GridManager.Instance.IsBusy || GridManager.Instance.IsAnyNodeFalling()) return;
			if (finger.IsOverGui) return;

			var ray = finger.GetRay(Helper.MainCamera);
			if (Physics.Raycast(ray, out var hit, 100, inputLayer))
			{
				if (hit.rigidbody && hit.rigidbody.TryGetComponent(out GridCell gridCell))
				{
					selectedCell = gridCell;
					OnDown?.Invoke(selectedCell);
				}
			}
		}

		private void OnFingerMove(LeanFinger finger)
		{
			if (!CanInput) return;
			if (GridManager.Instance.IsBusy || GridManager.Instance.IsAnyNodeFalling()) return;
			if (finger.IsOverGui) return;

			var ray = finger.GetRay(Helper.MainCamera);
			if (Physics.Raycast(ray, out var hit, 100, inputLayer))
			{
				if (hit.rigidbody && hit.rigidbody.TryGetComponent(out GridCell gridCell))
				{
					if (selectedCell != gridCell)
					{
						selectedCell = gridCell;
						OnMove?.Invoke(selectedCell);
					}
				}
			}
		}

		private void OnFingerUp(LeanFinger finger)
		{
			if (!CanInput) return;
			if (GridManager.Instance.IsBusy || GridManager.Instance.IsAnyNodeFalling()) return;
			if (finger.IsOverGui) return;
			if (!selectedCell) return;

			var ray = finger.GetRay(Helper.MainCamera);
			if (Physics.Raycast(ray, out var hit, 100, inputLayer))
			{
				if (hit.rigidbody && hit.rigidbody.TryGetComponent(out GridCell gridCell))
				{
					if (selectedCell == gridCell)
					{
						OnUp?.Invoke(selectedCell);
					}
				}
			}
		}

		private void OnLevelStarted()
		{
			CanInput = true;
			selectedCell = null;
		}

		private void OnLevelWon()
		{
			CanInput = false;
			selectedCell = null;
		}

		private void OnLevelLost()
		{
			CanInput = false;
			selectedCell = null;
		}
	}
}