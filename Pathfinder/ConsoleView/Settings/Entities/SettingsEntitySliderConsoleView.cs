using System;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Entities
{
	public class SettingsEntitySliderConsoleView : SettingsEntityWithValueConsoleView<SettingsEntitySliderVM>
	{
		[SerializeField]
		private OwlcatMultiButton m_SelectableMultiButton;
		
		[SerializeField]
		private Slider Slider;
		
		[SerializeField]
		private TextMeshProUGUI LabelSliderValue;

		private DisposableBooleanFlag m_ChangingFromUI = new DisposableBooleanFlag();

		protected override void BindViewImplementation()
		{
			base.BindViewImplementation();
			
			SetupSlider();

			AddDisposable(Slider.onValueChanged.AsObservable().Subscribe(SetValueFromUI));
			AddDisposable(ViewModel.TempFloatValue.Subscribe(SetValueFromSettings));
		}

		private void SetupSlider()
		{
			Slider.gameObject.SetActive(true);

			Slider.wholeNumbers = true;

			float range = ViewModel.MaxValue - ViewModel.MinValue;
			int lastStep = ViewModel.IsInt ? Convert.ToInt32(Math.Round(range)) : Convert.ToInt32(Math.Round(range / ViewModel.Step));

			Slider.minValue = 0;
			Slider.maxValue = lastStep;
			
			if (ViewModel.ShowValueText)
			{
				LabelSliderValue.gameObject.SetActive(true);
				int decimalPlaces = ViewModel.IsInt ? 0 : ViewModel.DecimalPlaces;
				LabelSliderValue.text = ViewModel.GetTempValue().ToString($"F{decimalPlaces}").Replace(",",".");
			}
			else
			{
				LabelSliderValue.gameObject.SetActive(false);
			}
		}
		
		public override void SetFocus(bool value)
		{
			base.SetFocus(value);

			m_SelectableMultiButton.SetFocus(value);
			m_SelectableMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
		}

		private float GetSettingValueFromSlider(float sliderValue)
		{
			return ViewModel.IsInt ? ViewModel.MinValue + sliderValue : ViewModel.MinValue + sliderValue * ViewModel.Step;
		}

		private float GetSliderValueFromSetting(float settingValue)
		{
			float offsetValue = settingValue - ViewModel.MinValue;
			return ViewModel.IsInt
				? Convert.ToInt32(Math.Round(offsetValue))
				: Convert.ToInt32(Math.Round(offsetValue / ViewModel.Step));
		}

		private void SetValueFromUI(float sliderValue)
		{
			UISoundController.Instance.Play(UISoundType.SettingsSliderMove);

			float settingsValue = GetSettingValueFromSlider(sliderValue);
			using (m_ChangingFromUI.Retain())
			{
				ViewModel.SetTempValue(settingsValue);
			}
		}

		private void SetValueFromSettings(float settingValue)
		{
			if (ViewModel.ShowValueText)
			{
				int decimalPlaces = ViewModel.IsInt ? 0 : ViewModel.DecimalPlaces;
				LabelSliderValue.text = settingValue.ToString($"F{decimalPlaces}").Replace(",", ".");
			}
			
			// m_SelectableMultiButton.SetSelected(value);
			// m_SelectableMultiButton.SetActiveLayer(value ? "Selected" : "Normal");

			if (m_ChangingFromUI)
			{
				return;
			}

			float sliderValue = GetSliderValueFromSetting(settingValue);
			Slider.value = sliderValue;
		}

		public override void OnModificationChanged(bool allowed)
		{
			Slider.interactable = allowed;
			m_SelectableMultiButton.Interactable = allowed;
		}
		
		public override bool HandleLeft()
		{
			if (ViewModel.ModificationAllowed.Value)
				ViewModel.SetPrevValue();

			return true;
		}

		public override bool HandleRight()
		{
			if (ViewModel.ModificationAllowed.Value)
				ViewModel.SetNextValue();

			return true;
		}
	}
}