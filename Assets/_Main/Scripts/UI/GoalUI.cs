using Cysharp.Threading.Tasks;
using Fiber.Managers;
using Fiber.Utilities.Extensions;
using GoalSystem;
using UnityEngine;

namespace UI
{
	public class GoalUI : MonoBehaviour
	{
		[SerializeField] private RectTransform goalContainer;
		[SerializeField] private GoalItemUI goalItemUIPrefab;

		private void OnEnable()
		{
			LevelManager.OnLevelLoad += OnLevelLoaded;
			LevelManager.OnLevelUnload += OnLevelUnloaded;
		}

		private void OnDestroy()
		{
			LevelManager.OnLevelLoad -= OnLevelLoaded;
			LevelManager.OnLevelUnload -= OnLevelUnloaded;
		}

		private void OnLevelUnloaded()
		{
			foreach (Transform t in goalContainer)
				Destroy(t.gameObject);
		}

		private async void OnLevelLoaded()
		{
			await UniTask.Yield();

			// Create new goal ui items
			foreach (var goal in GoalManager.Instance.Goals)
			{
				var goalItemUI = Instantiate(goalItemUIPrefab, goalContainer);
				goalItemUI.Setup(goal);
			}
		}
	}
}