using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;

namespace Kingmaker.UI.MVVM._VM.Settings.KeyBindSetupDialog
{
	public interface IKeyBindingSetupDialogHandler : IGlobalSubscriber
	{
		void OpenKeyBindingSetupDialog(UISettingsEntityKeyBinding uiSettingsEntity, int bindingIndex);
	}
}