using System.Collections;
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
				StartCoroutine(WaitLoadingScreen());
			}
			else
			{
				LevelStart();
			}
		}

		private void Unsub()
		{
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

		private IEnumerator WaitLoadingScreen()
		{
			yield return new WaitUntilAction(ref LoadingPanelController.Instance.OnLoadingFinished);

			LevelStart();
		}

		private void LevelStart()
		{
			if (LevelManager.Instance.LevelNo.Equals(1))
			{
				StartCoroutine(Level1Tutorial());
			}

			if (LevelManager.Instance.LevelNo.Equals(2))
			{
				StartCoroutine(Level2Tutorial());
			}
		}

		#region Level1 Tutorial

		private IEnumerator Level1Tutorial()
		{
			yield return null;

			Player.Instance.Inputs.CanInput = false;
			DeckUI.Instance.SetEnableNextButton(false);

			var cell = GridManager.Instance.GridCells[1, 2];

			tutorialUI.ShowTap(cell.transform.position, Helper.MainCamera);
			tutorialUI.ShowText("Tap to Swap tiles!");

			tutorialUI.SetupFakeButton(() =>
			{
				Deck.Instance.Swap(cell.CurrentNode);
				StartCoroutine(Level1OnSwapped());
			}, cell.transform.position, Helper.MainCamera);
		}

		private IEnumerator Level1OnSwapped()
		{
			tutorialUI.HideFakeButton();
			tutorialUI.HideText();
			tutorialUI.HideHand();

			yield return new WaitForSeconds(2);

			var cell = GridManager.Instance.GridCells[1, 2];

			tutorialUI.ShowTap(cell.transform.position, Helper.MainCamera);
			tutorialUI.ShowText("Match 3+ colors!");
			tutorialUI.ShowFocus(Deck.Instance.transform.position, Helper.MainCamera);

			tutorialUI.SetupFakeButton(() =>
			{
				Deck.Instance.Swap(cell.CurrentNode);
				Level1OnSwapped2();
			}, cell.transform.position, Helper.MainCamera);
		}

		private IEnumerator Level1OnSwapped2()
		{
			tutorialUI.HideFakeButton();
			tutorialUI.HideText();
			tutorialUI.HideHand();

			yield return new WaitForSeconds(5);

			tutorialUI.ShowFocus(GoalUI.Instance.transform.position);
			tutorialUI.ShowText("Complete the goals!");

			yield return new WaitForSeconds(2);

			tutorialUI.HideText();
			tutorialUI.HideFocus();

			Player.Instance.Inputs.CanInput = true;
		}

		#endregion

		#region level2 tutorial

		private IEnumerator Level2Tutorial()
		{
			yield return null;

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

			StartCoroutine(Level2DeckUI());
		}

		private IEnumerator Level2DeckUI()
		{
			tutorialUI.ShowFocus(DeckUI.Instance.BtnNext.transform.position);
			tutorialUI.ShowText("If you empty your deck, You lose!");

			yield return new WaitForSeconds(2.5f);

			tutorialUI.HideFocus();
			tutorialUI.HideText();

			Player.Instance.Inputs.CanInput = true;
		}

		#endregion
	}
}