using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeckSystem;
using Fiber.UI;
using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.Player;
using GridSystem;
using UI;
using UnityEngine;

namespace Managers
{
	public class TutorialManager : MonoBehaviour
	{
		private TutorialUI tutorialUI => TutorialUI.Instance;

		private CancellationTokenSource unsubCancellationToken = new CancellationTokenSource();

		private void OnEnable()
		{
			LevelManager.OnLevelStart += OnLevelStarted;
			LevelManager.OnLevelUnload += OnLevelUnloaded;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelStart -= OnLevelStarted;
			LevelManager.OnLevelUnload -= OnLevelUnloaded;
		}

		private void OnDestroy()
		{
			Unsub();
		}

		private void OnLevelUnloaded()
		{
			Unsub();
		}

		private void OnLevelStarted()
		{
			if (LoadingPanelController.Instance && LoadingPanelController.Instance.IsActive)
			{
				WaitLoadingScreen();
			}
			else
			{
				LevelStart();
			}
		}

		private void Unsub()
		{
			unsubCancellationToken.Cancel();
			StopAllCoroutines();

			if (TutorialUI.Instance)
			{
				tutorialUI.HideFocus();
				tutorialUI.HideHand();
				tutorialUI.HideText();
				tutorialUI.HideFakeButton();
			}

			Deck.OnNextNode -= Level2OnNextNode;
		}

		private async void WaitLoadingScreen()
		{
			try
			{
				await new WaitUntilAction(ref LoadingPanelController.Instance.OnLoadingFinished).ToUniTask(cancellationToken: unsubCancellationToken.Token);
			}
			catch (OperationCanceledException _)
			{
			}

			LevelStart();
		}

		private void LevelStart()
		{
			if (LevelManager.Instance.LevelNo.Equals(1))
			{
				Level1Tutorial();
			}

			if (LevelManager.Instance.LevelNo.Equals(2))
			{
				Level2Tutorial();
			}
		}

		#region Level1 Tutorial

		private async void Level1Tutorial()
		{
			await UniTask.Yield();

			Player.Instance.Inputs.CanInput = false;
			DeckUI.Instance.SetEnableNextButton(false);

			var cell = GridManager.Instance.GridCells[1, 2];

			tutorialUI.ShowTap(cell.transform.position, Camera.main);
			tutorialUI.ShowText("Tap to Swap tiles!");

			tutorialUI.SetupFakeButton(() =>
			{
				Deck.Instance.Swap(cell.CurrentNode);
				Level1OnSwapped();
			}, cell.transform.position, Camera.main);
		}

		private async void Level1OnSwapped()
		{
			tutorialUI.HideFakeButton();
			tutorialUI.HideText();
			tutorialUI.HideHand();
			try
			{
				await UniTask.WaitForSeconds(2, cancellationToken: unsubCancellationToken.Token);
			}
			catch (OperationCanceledException _)
			{
			}

			var cell = GridManager.Instance.GridCells[1, 2];

			tutorialUI.ShowTap(cell.transform.position, Camera.main);
			tutorialUI.ShowText("Match 3+ colors!");
			tutorialUI.ShowFocus(Deck.Instance.transform.position, Camera.main);

			tutorialUI.SetupFakeButton(() =>
			{
				Deck.Instance.Swap(cell.CurrentNode);
				Level1OnSwapped2();
			}, cell.transform.position, Camera.main);
		}

		private async void Level1OnSwapped2()
		{
			tutorialUI.HideFakeButton();
			tutorialUI.HideText();
			tutorialUI.HideHand();

			try
			{
				await UniTask.WaitForSeconds(5, cancellationToken: unsubCancellationToken.Token);
			}
			catch (OperationCanceledException _)
			{
			}

			tutorialUI.ShowFocus(GoalUI.Instance.transform.position);
			tutorialUI.ShowText("Complete the goals!");
			try
			{
				await UniTask.WaitForSeconds(2, cancellationToken: unsubCancellationToken.Token);
			}
			catch (OperationCanceledException _)
			{
			}

			tutorialUI.HideText();
			tutorialUI.HideFocus();

			Player.Instance.Inputs.CanInput = true;
		}

		#endregion

		#region level2 tutorial

		private async void Level2Tutorial()
		{
			await UniTask.Yield();

			Player.Instance.Inputs.CanInput = false;
			DeckUI.Instance.SetEnableNextButton(true);
			var btn = DeckUI.Instance.BtnNext;

			tutorialUI.ShowFocus(btn.transform.position);
			tutorialUI.ShowTap(btn.transform.position);
			tutorialUI.ShowText("Use Next button to discard current tile");

			Deck.OnNextNode += Level2OnNextNode;
		}

		private void Level2OnNextNode(Node node)
		{
			Deck.OnNextNode -= Level2OnNextNode;

			tutorialUI.HideFocus();
			tutorialUI.HideText();
			tutorialUI.HideHand();

			Level2DeckUI();
		}

		private async void Level2DeckUI()
		{
			tutorialUI.ShowFocus(DeckUI.Instance.BtnNext.transform.position);
			tutorialUI.ShowText("If you empty your deck, You lose!");

			try
			{
				await UniTask.WaitForSeconds(2.5f, cancellationToken: unsubCancellationToken.Token);
			}
			catch (OperationCanceledException _)
			{
			}

			tutorialUI.HideFocus();
			tutorialUI.HideText();

			Player.Instance.Inputs.CanInput = true;
		}

		#endregion
	}
}