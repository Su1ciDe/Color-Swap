using Cysharp.Threading.Tasks;
using Fiber.CurrencySystem;
using Fiber.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fiber.UI
{
	public class WinPanel : PanelUI
	{
		[SerializeField] private TMP_Text txtLevelNo;
		[SerializeField] private Button btnContinue;
		[Space]
		[SerializeField] private TMP_Text txtMoneyAmount;

		private void Awake()
		{
			btnContinue.onClick.AddListener(Win);
		}

		private async void Win()
		{
			CurrencyManager.Money.AddCurrency(LevelManager.Instance.CurrentLevel.Money, txtMoneyAmount.rectTransform.position, false);

			txtMoneyAmount.transform.parent.gameObject.SetActive(false);
			btnContinue.gameObject.SetActive(false);

			await UniTask.Delay(2250);

			LevelManager.Instance.LoadNextLevel();
			Close();
		}

		private void SetLevelNo()
		{
			txtLevelNo.SetText("LEVEL " + LevelManager.Instance.LevelNo.ToString());
		}
		
		private void SetMoneyAmount()
		{
			txtMoneyAmount.SetText("+" + LevelManager.Instance.CurrentLevel.Money.ToString());
		}

		public override void Open()
		{
			btnContinue.gameObject.SetActive(true);
			txtMoneyAmount.transform.parent.gameObject.SetActive(true);
			SetLevelNo();
			SetMoneyAmount();

			base.Open();
		}
	}
}