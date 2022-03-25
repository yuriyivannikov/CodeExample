using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Entities
{
	public abstract class SettingsEntityConsoleView<TSettingsEntityVM> : VirtualListElementViewBase<TSettingsEntityVM>
		where TSettingsEntityVM : SettingsEntityVM
	{
		[SerializeField]
		private GameObject m_SetConnector;
		
		[SerializeField]
		private GameObject m_SetConnectorIAmSet;

		[SerializeField]
		private TextMeshProUGUI m_Title;

		protected override void BindViewImplementation()
		{
			if (m_Title != null)
				m_Title.text = ViewModel.Title;
			
			if (m_SetConnector != null)
				m_SetConnector.SetActive(ViewModel.IsConnector || ViewModel.IsSet);
			
			if (m_SetConnectorIAmSet != null)
				m_SetConnectorIAmSet.SetActive(ViewModel.IsSet);
		}

		protected override void DestroyViewImplementation()
		{
		}
	}
}