using System;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.UI.MVVM._VM.Settings.Menu
{
	public class SettingsMenuEntityVM : SelectionGroupEntityVM
	{
		public readonly string Title;
		public readonly bool IsSaber;
		public UISettingsManager.SettingsScreen SettingsScreenType { get; }
		private Action<UISettingsManager.SettingsScreen> m_ConfirmAction;

		public SettingsMenuEntityVM(string title, UISettingsManager.SettingsScreen screenType, Action<UISettingsManager.SettingsScreen> confirmAction, bool isSaber = false) : base(false)
		{
			Title = title;
			IsSaber = isSaber;
			SettingsScreenType = screenType;
			m_ConfirmAction = confirmAction;
		}

		public void Confirm()
		{
			DoSelectMe();
		}

		protected override void DoSelectMe()
		{
			m_ConfirmAction?.Invoke(SettingsScreenType);
		}

		protected override void DisposeImplementation()
		{
			m_ConfirmAction = null;
		}
	}
}