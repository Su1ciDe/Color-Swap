using DG.Tweening;
using Fiber.Managers;
using GoalSystem;
using GridSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class GoalItemUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text txtGoalCount;
		[SerializeField] private Image imgTileColor;
		[SerializeField] private Image imgTileAll;

		private Goal currentGoal;

		public void Setup(Goal goal)
		{
			currentGoal = goal;
			currentGoal.OnGoalUpdate += OnGoalUpdated;
			currentGoal.OnGoalComplete += OnGoalCompleted;

			if (goal.TileType == TileType._8All)
			{
				imgTileAll.gameObject.SetActive(true);
				imgTileColor.gameObject.SetActive(false);
			}
			else
			{
				imgTileAll.gameObject.SetActive(false);
				imgTileColor.gameObject.SetActive(true);
				imgTileColor.color = GameManager.Instance.ColorsSO.Colors[goal.TileType];
			}

			SetGoalCountText(goal.Count);
		}

		private void OnDestroy()
		{
			currentGoal.OnGoalUpdate -= OnGoalUpdated;
			currentGoal.OnGoalComplete -= OnGoalCompleted;
		}

		private void OnGoalUpdated(int currentCount, int totalCount)
		{
			var count = Mathf.Clamp(totalCount - currentCount, 0, int.MaxValue);
			SetGoalCountText(count);

			transform.DOComplete();
			transform.DOScale(1.5f, .1f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
		}

		private void OnGoalCompleted(Goal goal)
		{
			currentGoal.OnGoalUpdate -= OnGoalUpdated;
			currentGoal.OnGoalComplete -= OnGoalCompleted;

			transform.DOComplete();
			transform.DOScale(0, .5f).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
		}

		private void SetGoalCountText(int count)
		{
			txtGoalCount.SetText(count.ToString());
		}
	}
}