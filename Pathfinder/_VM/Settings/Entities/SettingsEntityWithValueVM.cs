using Kingmaker.UI.SettingsUI;
using Kingmaker.Utility.UniRxExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.Settings.Entities
{
	public abstract class SettingsEntityWithValueVM : SettingsEntityVM
	{
		public readonly ReadOnlyReactiveProperty<bool> IsChanged;
		private IUISettingsEntityWithValueBase m_UISettingsEntity;

		public readonly ReactiveProperty<bool> ModificationAllowed;

		public readonly bool IsOdd;
		
		public SettingsEntityWithValueVM(IUISettingsEntityWithValueBase uiSettingsEntity) : base(uiSettingsEntity)
		{
			m_UISettingsEntity = uiSettingsEntity;
			if (m_UISettingsEntity.SettingsEntity != null)
				AddDisposable(IsChanged = m_UISettingsEntity.SettingsEntity.ObserveTempValueIsConfirmed().Not().ToReadOnlyReactiveProperty());
			else
			{
				UberDebug.LogError($"Error: SettingsEntity in UISettingsEntity [{m_UISettingsEntity.Description}] not found ");
			}
			AddDisposable(ModificationAllowed = new ReactiveProperty<bool>(m_UISettingsEntity.ModificationAllowed));

			IsOdd = false;// todo PF-146487
		}
		
		public void ResetToDefault()
		{
			m_UISettingsEntity.SettingsEntity.ResetToDefault(false);
		}
	}
}