using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Decorative;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Entities
{
	public class SettingsEntityHeaderConsoleView : VirtualListElementViewBase<SettingsEntityHeaderVM>, IConsoleNavigationEntity
	{
		[SerializeField]
		private TextMeshProUGUI m_Tittle;
		
		[SerializeField]
		private VirtualListLayoutElementSettings m_LayoutElementSettings;
		
		public override VirtualListLayoutElementSettings LayoutSettings
			=> m_LayoutElementSettings;
		
		protected override void BindViewImplementation()
		{
			m_Tittle.text = UIUtility.GetSaberBookFormat(ViewModel.Tittle);
		}

		protected override void DestroyViewImplementation()
		{ 
		}

		public void SetFocus(bool value)
		{
		}

		public bool IsValid()
		{
			return false;
		}
	}
}