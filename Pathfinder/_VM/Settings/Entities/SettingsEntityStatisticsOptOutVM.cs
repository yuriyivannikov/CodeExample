using Kingmaker.UI.SettingsUI;

namespace Kingmaker.UI.MVVM._VM.Settings.Entities
{
	public class SettingsEntityStatisticsOptOutVM : SettingsEntityVM
	{
		private UISettingsEntityOptOut m_StatisticsEntity;
        
		public SettingsEntityStatisticsOptOutVM(UISettingsEntityOptOut uiSettingsEntity) : base(uiSettingsEntity)
		{
			m_StatisticsEntity = uiSettingsEntity;
		}

		public void OpenSettingsInBrowser()
		{
			m_StatisticsEntity.OpenSettingsInBrowser();
		}
	}
}