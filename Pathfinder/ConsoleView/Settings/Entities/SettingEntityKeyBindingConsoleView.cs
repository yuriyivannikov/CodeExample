using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Entities
{
	public class SettingEntityKeyBindingConsoleView : SettingsEntityWithValueConsoleView<SettingEntityKeyBindingVM>
	{
		[SerializeField]
		private OwlcatMultiButton m_BindingButton1;
		[SerializeField]
		private OwlcatMultiButton m_BindingButton2;

		[SerializeField]
		private TextMeshProUGUI m_BindingText1;
		[SerializeField]
		private TextMeshProUGUI m_BindingText2;

		protected override void BindViewImplementation()
		{
			base.BindViewImplementation();
			AddDisposable(m_BindingButton1.OnConfirmClickAsObservable().Subscribe(_ => ViewModel.OpenBindingDialogVM(0)));
			AddDisposable(m_BindingButton2.OnConfirmClickAsObservable().Subscribe(_ => ViewModel.OpenBindingDialogVM(1)));
			
			// AddDisposable(ViewModel.TempBindingValue1.Subscribe(data => SetupBindingButton(m_BindingButton1, m_BindingText1, data.GetPrettyString())));
			// AddDisposable(ViewModel.TempBindingValue2.Subscribe(data => SetupBindingButton(m_BindingButton2, m_BindingText2, data.GetPrettyString())));
		}

		private void SetupBindingButton(OwlcatMultiButton button, TextMeshProUGUI buttonText, string text)
		{
			bool isEmpty = string.IsNullOrEmpty(text);
			button.SetActiveLayer(isEmpty ? "Off" : "On");
			buttonText.text = isEmpty ? "---" : text;
		}

		public override void OnModificationChanged(bool allowed)
		{
			m_BindingButton1.Interactable = allowed;
			m_BindingButton2.Interactable = allowed;
		}
		
		public override bool HandleLeft()
		{
			if (!ViewModel.ModificationAllowed.Value)
				return false;

			

			return false;
		}

		public override bool HandleRight()
		{
			if (!ViewModel.ModificationAllowed.Value)
				return false;

			

			return false;
		}
	}
}