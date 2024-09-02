using Cysharp.Threading.Tasks;
using DeckSystem;
using DG.Tweening;
using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class DeckUI : Singleton<DeckUI>
	{
		[SerializeField] private Button btnNext;
		[SerializeField] private TMP_Text txtNodesInDeckCount;

		public Button BtnNext => btnNext;
		
		private void Awake()
		{
			btnNext.onClick.AddListener(NextButtonClicked);

			LevelManager.OnLevelStart += OnLevelStarted;
		}

		private void OnDestroy()
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
			transform.DOComplete();
			transform.DOScale(0.75f, .1f).SetEase(Ease.InOutExpo).SetLoops(2, LoopType.Yoyo);

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

			txtNodesInDeckCount.color = count <= 3 ? Color.red : Color.white;
		}

		public void SetEnableNextButton(bool enable)
		{
			btnNext.gameObject.SetActive(enable);
		}
	}
}