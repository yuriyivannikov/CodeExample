using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Settings;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.Settings.KeyBindSetupDialog
{
	public static class KeyBindingDataExtensions
	{
		public static string GetPrettyString(this KeyBindingData keyBindingData)
		{
			StringBuilder sb = new StringBuilder();
            
			if (keyBindingData.IsCtrlDown)
			{
				sb.Append("Ctrl+");
			}
			if (keyBindingData.IsAltDown)
			{
				sb.Append("Alt+");
			}
			if (keyBindingData.IsShiftDown)
			{
				sb.Append("Shift+");
			}
            
			if (keyBindingData.Key != KeyCode.None)
			{
				foreach (var entry in KeyboardAccess.AltCodes)
				{
					if (entry == keyBindingData.Key)
					{
						return "Alt";
					}
				}
				foreach (var entry in KeyboardAccess.CtrlCodes)
				{
					if (entry == keyBindingData.Key)
					{
						return "Ctrl";
					}
				}
				foreach (var entry in KeyboardAccess.ShiftCodes)
				{
					if (entry == keyBindingData.Key)
					{
						return "Shift";
					}
				}
				sb.Append(GetKeyCodeString(keyBindingData.Key));
			}

			return sb.ToString();
		}

		private static string GetKeyCodeString(KeyCode key)
		{
			foreach (var entry in KeyboardAccess.AltCodes)
			{
				if (entry == key)
				{
					return "Alt";
				}
			}
			foreach (var entry in KeyboardAccess.CtrlCodes)
			{
				if (entry == key)
				{
					return "Ctrl";
				}
			}
			foreach (var entry in KeyboardAccess.ShiftCodes)
			{
				if (entry == key)
				{
					return "Shift";
				}
			}
			return UIStrings.Instance.KeyboardTexts.GetStringByKeyCode(key);
		}
	}
}