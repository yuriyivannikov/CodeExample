using Kingmaker.UI.MVVM._PCView.Settings.Entities;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Entities
{
	public class SettingsEntityStatisticsOptOutConsoleView : SettingsEntityView<SettingsEntityStatisticsOptOutVM>
	{
		[SerializeField]
		private OwlcatButton m_GoToStatisticsButton;

		protected override void BindViewImplementation()
		{
			base.BindViewImplementation();
			AddDisposable(m_GoToStatisticsButton.OnConfirmClickAsObservable().Subscribe(ViewModel.OpenSettingsInBrowser));
		}
	}
}