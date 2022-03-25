using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM._VM.Settings.Entities.Decorative
{
	public class SettingsEntityHeaderVM : VirtualListElementVMBase
	{
		public readonly string Tittle;

		public SettingsEntityHeaderVM(string tittle)
		{
			Tittle = tittle;
		}

		protected override void DisposeImplementation()
		{ }
	}
}