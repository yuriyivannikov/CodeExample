using Kingmaker.UI.MVVM._VM.Settings.Entities.Difficulty;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Entities.Difficulty
{
	public class SettingsEntityDropdownGameDifficultyItemConsoleView : ViewBase<SettingsEntityDropdownGameDifficultyItemVM>
	{
		[SerializeField]
		private Image m_Icon;

		[SerializeField]
		private TextMeshProUGUI m_Title;

		[SerializeField]
		private OwlcatMultiButton m_Button;
		
		protected override void BindViewImplementation()
		{
			m_Icon.sprite = ViewModel.Icon;
			m_Title.text = ViewModel.Title;
			
			AddDisposable(ViewModel.IsSelected.Subscribe(SetValueFromSettings));

			// if (ViewModel.IsCustom)
			// {
			// 	return;
			// }
			//AddDisposable(m_Button.OnConfirmClickAsObservable().Subscribe(ViewModel.SetSelected));
		}

		private void SetValueFromSettings(bool value)
		{
			m_Button.SetActiveLayer(value ? "On" : "Off");
		}

		protected override void DestroyViewImplementation()
		{
		}
	}
}