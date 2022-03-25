using System.Linq;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Entities
{
	public class SettingsEntityDropdownConsoleView : SettingsEntityWithValueConsoleView<SettingsEntityDropdownVM>
	{
		[SerializeField]
		private OwlcatMultiButton m_SelectableMultiButton;
		
		[SerializeField]
		public TMP_Dropdown Dropdown;

		private DisposableBooleanFlag m_ChangingFromUI = new DisposableBooleanFlag();

		protected override void BindViewImplementation()
		{
			base.BindViewImplementation();
			
			SetupDropdown();
			
			AddDisposable(Dropdown.onValueChanged.AsObservable().Subscribe(SetValueFromUI));
			AddDisposable(ViewModel.TempIndexValue.Subscribe(SetValueFromSettings));
		}

		private void SetupDropdown()
		{
			Dropdown.gameObject.SetActive(true);
			Dropdown.ClearOptions();
			if (ViewModel.LocalizedValues != null && ViewModel.LocalizedValues.Count > 0)
			{
				Dropdown.AddOptions(ViewModel.LocalizedValues);
			}
			else
			{
				Dropdown.AddOptions(Enumerable.Range(0, 10).Select(i => $"Generic option {i}").ToList());
			}
		}

		public override void SetFocus(bool value)
		{
			base.SetFocus(value);

			m_SelectableMultiButton.SetFocus(value);
			m_SelectableMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
		}

		protected virtual void SetValueFromUI(int value)
		{
			using (m_ChangingFromUI.Retain())
			{
				ViewModel.SetTempValue(value);
			}
		}

		private void SetValueFromSettings(int value)
		{
			if (m_ChangingFromUI)
			{
				return;
			}
			Dropdown.value = value;
		}

		public override void OnModificationChanged(bool allowed)
		{
			Dropdown.interactable = allowed;
		}
		
		public override bool HandleLeft()
		{
			if (ViewModel.ModificationAllowed.Value)
			{
				Dropdown.value--;
			}

			return true;
		}

		public override bool HandleRight()
		{
			if (ViewModel.ModificationAllowed.Value)
			{
				Dropdown.value++;
			}

			return true;
		}
	}
}