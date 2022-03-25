using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM._VM.Settings.Entities
{
	public abstract class SettingsEntityVM : VirtualListElementVMBase
	{
		public readonly string Title;
		public readonly string Description;

		public readonly bool IsConnector;
		public readonly bool IsSet;
		
		public SettingsEntityVM(IUISettingsEntityBase uiSettingsEntity)
		{
			Title = uiSettingsEntity.Description;
			Description = uiSettingsEntity.TooltipDescription;

			IsConnector = uiSettingsEntity.ShowVisualConnection && !uiSettingsEntity.IAmSetHandler;
			IsSet = uiSettingsEntity.ShowVisualConnection && uiSettingsEntity.IAmSetHandler;
		}
		
		protected override void DisposeImplementation()
		{
		}
	}
}