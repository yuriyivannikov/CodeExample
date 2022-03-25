using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Entities
{
	public class SettingsEntityBoolConsoleView : SettingsEntityWithValueConsoleView<SettingsEntityBoolVM>
	{
		[SerializeField]
		private OwlcatMultiButton m_SelectableMultiButton;
		
		[SerializeField]
		private OwlcatMultiSelectable m_MultiSelectable;

		protected override void BindViewImplementation()
		{
			base.BindViewImplementation();
			AddDisposable(ViewModel.TempValue.Subscribe(SetValueFromSettings));
		}

		private void SetValueFromSettings(bool value)
		{
			m_MultiSelectable.SetActiveLayer(value ? "On" : "Off");
		}

		public override void SetFocus(bool value)
		{
			base.SetFocus(value);
			
			m_SelectableMultiButton.SetFocus(value);
			m_SelectableMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
		}

		public override void OnModificationChanged(bool allowed)
		{
			m_SelectableMultiButton.Interactable = allowed;
		}

		public override bool HandleLeft()
		{
			if (ViewModel.ModificationAllowed.Value)
				ViewModel.ChangeValue();

			return true;
		}

		public override bool HandleRight()
		{
			if (ViewModel.ModificationAllowed.Value)
				ViewModel.ChangeValue();

			return true;
		}
	}
}