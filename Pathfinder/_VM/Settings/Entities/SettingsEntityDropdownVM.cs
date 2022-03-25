using System.Collections.Generic;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.Settings.Entities
{
	public class SettingsEntityDropdownVM : SettingsEntityWithValueVM, IVirtualListElementIdentifier
	{
		public enum DropdownType
		{
			Default = 0,
			DisplayMode = 1
		}

		public int VirtualListTypeId
			=> (int)m_DropdownType;
		
		private DropdownType m_DropdownType;
		
		public const int DefaultDropdownIndex = 0;
		public const int DisplayModeDropdownIndex = 1;
		
		private readonly IUISettingsEntityDropdown m_UISettingsEntity;
		public readonly ReadOnlyReactiveProperty<int> TempIndexValue;

		public List<string> LocalizedValues
			=> m_UISettingsEntity.LocalizedValues;

		public SettingsEntityDropdownVM(IUISettingsEntityDropdown uiSettingsEntity, DropdownType dropdownType = DropdownType.Default) : base(uiSettingsEntity)
		{
			m_UISettingsEntity = uiSettingsEntity;
			m_DropdownType = dropdownType;
			
			AddDisposable(TempIndexValue = Observable.FromEvent<int>(
				h => uiSettingsEntity.OnTempIndexValueChanged += h,
				h => uiSettingsEntity.OnTempIndexValueChanged -= h)
				.ToReadOnlyReactiveProperty(uiSettingsEntity.GetIndexTempValue()));
		}

		public int GetTempValue()
		{
			return m_UISettingsEntity.GetIndexTempValue();
		}

		public void SetTempValue(int value)
		{
			if (!ModificationAllowed.Value)
			{
				return;
			}

			m_UISettingsEntity.SetIndexTempValue(value);
		}

		public void SetNextValue()
		{
			SetTempValue((GetTempValue() + 1) % m_UISettingsEntity.ValuesCount());
		}
		
		public void SetPrevValue()
		{
			SetTempValue((GetTempValue() - 1 + m_UISettingsEntity.ValuesCount()) % m_UISettingsEntity.ValuesCount());
		}
	}
}