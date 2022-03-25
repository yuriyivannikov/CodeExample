using Kingmaker.UI.MVVM._PCView.ActionBar;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.ActionBar
{
	public class ActionBarWeaponSetConsoleView : ActionBarWeaponSetPCView, IConfirmClickHandler, IConsoleNavigationEntity
	{
		[SerializeField]
		private OwlcatMultiButton m_FocusMultiButton;
		
		public bool CanConfirmClick()
		{
			return true;
		}

		public string GetConfirmClickHint()
		{
			return string.Empty;
		}

		public void OnConfirmClick()
		{
			ViewModel.OnSetCurrentSet();
		}

		public void SetFocus(bool value)
		{
			m_FocusMultiButton.SetFocus(value);
			m_FocusMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
		}

		public bool IsValid()
		{
			return true;
		}
	}
}