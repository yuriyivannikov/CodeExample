using Kingmaker.PubSubSystem;
using Kingmaker.Settings;
using Kingmaker.UI.MVVM._VM.Settings.KeyBindSetupDialog;
using Kingmaker.UI.SettingsUI;
using Kingmaker.Utility.UniRxExtensions;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.Settings.Entities
{
	public class SettingEntityKeyBindingVM : SettingsEntityWithValueVM
	{
		private readonly UISettingsEntityKeyBinding m_UISettingsEntityKeybind;
		public readonly ReadOnlyReactiveProperty<KeyBindingData> TempBindingValue1;
		public readonly ReadOnlyReactiveProperty<KeyBindingData> TempBindingValue2;
		
		public SettingEntityKeyBindingVM(UISettingsEntityKeyBinding uiSettingsEntity) : base(uiSettingsEntity)
		{
			m_UISettingsEntityKeybind = uiSettingsEntity;

			SettingsEntityKeyBindingPair setting = uiSettingsEntity.SettingKeyBindingPair;

			AddDisposable(TempBindingValue1 = setting.ObserveTempValue()
				.Select(pair => pair.Binding1)
				.ToReadOnlyReactiveProperty(setting.GetTempValue().Binding1));
			AddDisposable(TempBindingValue2 = setting.ObserveTempValue()
				.Select(pair => pair.Binding2)
				.ToReadOnlyReactiveProperty(setting.GetTempValue().Binding2));
		}

		public void OpenBindingDialogVM(int bindingIndex)
		{
			EventBus.RaiseEvent<IKeyBindingSetupDialogHandler>(h
				=> h.OpenKeyBindingSetupDialog(m_UISettingsEntityKeybind, bindingIndex));
		}
	}
}