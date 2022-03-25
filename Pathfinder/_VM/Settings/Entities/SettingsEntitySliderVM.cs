using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.Settings.Entities
{
	public class SettingsEntitySliderVM : SettingsEntityWithValueVM, IVirtualListElementIdentifier
	{
		public enum SliderType
		{
			Default = 0,
			VisualPerception = 1,
			VisualPerceptionWithImages = 2
		}

		public const int DefaultSliderIndex = 0;
		public const int VisualPerceptionSliderIndex = 1;
		public const int VisualPerceptionWithImagesSliderIndex = 2;

		public int VirtualListTypeId
			=> (int)m_SliderType;

		private readonly IUISettingsEntitySlider m_UISettingsEntity;
		public readonly ReadOnlyReactiveProperty<float> TempFloatValue;

		public float MinValue;
		public float MaxValue;
		
		public bool IsInt;
		public float Step;
		
		public bool ShowValueText;
		public int DecimalPlaces;

		private SliderType m_SliderType;
		
		public SettingsEntitySliderVM(IUISettingsEntitySlider uiSettingsEntity, SliderType sliderType = SliderType.Default) : base(uiSettingsEntity)
		{
			m_UISettingsEntity = uiSettingsEntity;
			m_SliderType = sliderType;

			MinValue = m_UISettingsEntity.MinValue;
			MaxValue = m_UISettingsEntity.MaxValue;
			IsInt = m_UISettingsEntity.IsInt;

			Step = m_UISettingsEntity.Step;
			ShowValueText = m_UISettingsEntity.ShowValueText;
			DecimalPlaces = m_UISettingsEntity.DecimalPlaces;

			AddDisposable(TempFloatValue = Observable.FromEvent<float>(
					h => m_UISettingsEntity.OnTempFloatValueChanged += h,
					h => m_UISettingsEntity.OnTempFloatValueChanged -= h)
				.ToReadOnlyReactiveProperty(m_UISettingsEntity.GetFloatTempValue()));
		}

		public float GetTempValue()
		{
			return m_UISettingsEntity.GetFloatTempValue();
		}

		public void SetTempValue(float value)
		{
			if (!ModificationAllowed.Value)
			{
				return;
			}

			m_UISettingsEntity.SetFloatTempValue(value);
		}

		public void SetNextValue(int steps = 1)
		{
			float newValue = GetTempValue() + (IsInt ? 1f : m_UISettingsEntity.Step) * steps;
			newValue = Mathf.Min(newValue, MaxValue);
			SetTempValue(newValue);
		}

		public void SetPrevValue(int steps = 1)
		{
			float newValue = GetTempValue() - (IsInt ? 1f : m_UISettingsEntity.Step) * steps;
			newValue = Mathf.Max(newValue, MinValue);
			SetTempValue(newValue);
		}
	}
}