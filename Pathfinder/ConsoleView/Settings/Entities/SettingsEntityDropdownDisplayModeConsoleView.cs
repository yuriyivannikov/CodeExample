using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Entities
{
	public class SettingsEntityDropdownDisplayModeConsoleView : SettingsEntityDropdownConsoleView
	{
		protected override void SetValueFromUI(int value)
		{
			if (ViewModel.GetTempValue() == value)
				return;
			
			base.SetValueFromUI(value);
			
			bool exclusiveFullscreen = SettingsRoot.Graphics.FullScreenMode.GetTempValue() == FullScreenMode.ExclusiveFullScreen;
			if (exclusiveFullscreen)
				UIUtility.ShowMessageBox(UIStrings.Instance.CommonTexts.ExclusiveFullscreenWarning,
					MessageModalBase.ModalType.Message, null);
		}
	}
}