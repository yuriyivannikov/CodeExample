using System.Collections.Generic;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Difficulty;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Entities.Difficulty
{
	public class SettingsEntityGameDifficultyConsoleView : SettingsEntityWithValueConsoleView<SettingsEntityDropdownGameDifficultyVM>
	{
		[SerializeField]
		private VirtualListLayoutElementSettings m_LayoutElementSettings;

		[SerializeField]
		private List<SettingsEntityDropdownGameDifficultyItemConsoleView> m_ItemViews;
		
		[SerializeField]
		private OwlcatMultiButton m_FocusMultiButton;
		
		public override VirtualListLayoutElementSettings LayoutSettings
			=> m_LayoutElementSettings;

		protected override void BindViewImplementation()
		{
			base.BindViewImplementation();
			for (int i = 0; i < ViewModel.Items.Count; i++)
			{
				m_ItemViews[i].Bind(ViewModel.Items[i]);
			}
		}
		
		public override void SetFocus(bool value)
		{
			base.SetFocus(value);
			
			m_FocusMultiButton.SetFocus(value);
			m_FocusMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
		}

		public override void OnModificationChanged(bool allowed)
		{
			
		}

		public override bool HandleLeft()
		{
			if (ViewModel.ModificationAllowed.Value)
				ViewModel.SetPrevValue();
			
			return true;
		}

		public override bool HandleRight()
		{
			if (ViewModel.ModificationAllowed.Value)
				ViewModel.SetNextValue();
			
			return true;
		}
	}
}