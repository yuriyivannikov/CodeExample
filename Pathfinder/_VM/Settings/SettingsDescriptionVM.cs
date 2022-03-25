using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM._VM.Settings
{
	public class SettingsDescriptionVM : BaseDisposable, IViewModel
	{
		public string Title { get; }
		public string Description { get; }

		private IDisposable m_CurrentEntity;

		public SettingsDescriptionVM(string title, string description)
		{
			Title = title;
			Description = description;
		}

		protected override void DisposeImplementation()
		{
		}
	}
}