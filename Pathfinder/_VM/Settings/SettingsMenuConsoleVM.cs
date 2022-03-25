using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM._VM.Settings.Menu;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM._VM.Settings
{
	public class SettingsMenuConsoleVM : BaseDisposable, IViewModel
	{
		public readonly List<SettingsMenuEntityVM> MenuEntitiesVMList = new List<SettingsMenuEntityVM>();
		
		private readonly Action<UISettingsManager.SettingsScreen?, bool> m_CloseAction;
		private readonly bool m_IsMainMenu;

		public SettingsMenuConsoleVM(Action<UISettingsManager.SettingsScreen?, bool> closeAction, bool isMainMenu = false)
		{
			m_CloseAction = closeAction;
			m_IsMainMenu = isMainMenu;

			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameGame, UISettingsManager.SettingsScreen.Game);
			
			if (!isMainMenu)
				CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameDifficulty, UISettingsManager.SettingsScreen.Difficulty);	
			
			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameControls, UISettingsManager.SettingsScreen.Controls);
			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameGraphics, UISettingsManager.SettingsScreen.Graphics);
			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameSound, UISettingsManager.SettingsScreen.Sound);
		}

		private void CreateMenuEntity(LocalizedString localizedString, UISettingsManager.SettingsScreen screenType)
		{
			var entityVM = new SettingsMenuEntityVM(localizedString, screenType, ShowSettings, true);
			AddDisposable(entityVM);
			MenuEntitiesVMList.Add(entityVM);
		}

		private void ShowSettings(UISettingsManager.SettingsScreen screenType)
		{
			m_CloseAction?.Invoke(screenType, m_IsMainMenu);
			Game.Instance.Player.UISettings.LastSettingsMenuType = screenType;
		}

		public void Close()
		{
			m_CloseAction?.Invoke(null, m_IsMainMenu);
			Game.Instance.Player.UISettings.LastSettingsMenuType = UISettingsManager.SettingsScreen.Game;
		}

		protected override void DisposeImplementation()
		{
			MenuEntitiesVMList.Clear();
		}
	}
}