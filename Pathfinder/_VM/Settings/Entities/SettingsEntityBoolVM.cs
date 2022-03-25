using Kingmaker.UI.SettingsUI;
using Kingmaker.Utility.UniRxExtensions;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.Settings.Entities
{
	public class SettingsEntityBoolVM : SettingsEntityWithValueVM
	{
		private UISettingsEntityBool m_UISettingsEntity;
		public readonly ReadOnlyReactiveProperty<bool> TempValue;

		public SettingsEntityBoolVM(UISettingsEntityBool uiSettingsEntity) : base(uiSettingsEntity)
		{
			m_UISettingsEntity = uiSettingsEntity;
			AddDisposable(TempValue = m_UISettingsEntity.Setting.ObserveTempValue());
		}

		public bool GetTempValue()
		{
			return m_UISettingsEntity.Setting.GetTempValue();
		}

		public void SetTempValue(bool value)
		{
			if (!ModificationAllowed.Value)
			{
				return;
			}

			m_UISettingsEntity.Setting.SetTempValue(value);
		}

		public void ChangeValue()
		{
			SetTempValue(!GetTempValue());
		}
	}
}