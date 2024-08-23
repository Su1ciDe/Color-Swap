using Cysharp.Threading.Tasks;
using DeckSystem;
using Fiber.Managers;
using GamePlay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class DeckUI : MonoBehaviour
	{
		[SerializeField] private Button btnNext;
		[SerializeField] private TMP_Text txtNodesInDeckCount;

		private void Awake()
		{
			btnNext.onClick.AddListener(NextButtonClicked);
		}

		private void OnEnable()
		{
			LevelManager.OnLevelStart += OnLevelStarted;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelStart -= OnLevelStarted;
		}

		private async void OnLevelStarted()
		{
			await UniTask.Yield();
			SetDeckCountText(Deck.Instance.NodeCount + 1);
		}

		private async void NextButtonClicked()
		{
			btnNext.interactable = false;
			Player.Instance.Inputs.CanInput = false;

			SetDeckCountText(Deck.Instance.NodeCount);

			await Deck.Instance.NextNode();

			btnNext.interactable = true;
			Player.Instance.Inputs.CanInput = true;
		}

		private void SetDeckCountText(int count)
		{
			txtNodesInDeckCount.SetText(count.ToString());
		}
	}
}