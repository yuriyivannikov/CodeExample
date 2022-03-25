using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM._PCView.ActionBar;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.ActionBar
{
	public class AutoUseAbilityConsoleView : AutoUseAbilityBaseView, IConsoleNavigationEntity, IConfirmClickHandler
	{
		[SerializeField]
		private ActionBarSlotConsoleView m_SlotConsoleView;
		
		[SerializeField]
		private OwlcatMultiButton m_FocusMultiButton;
		
		[SerializeField]
		private FadeAnimator m_FadeAnimator;
		
		public void SetFocus(bool value)
		{
			m_FocusMultiButton.SetFocus(value);
			m_FocusMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
		}

		public bool IsValid()
		{
			return ViewModel.HasAbility.Value;
		}
		
		public void Clear()
		{
			m_Slot.MechanicActionBarSlot.OnAutoUseToggle();
		}

		public bool CanConfirmClick()
		{
			return true;
		}

		public string GetConfirmClickHint()
		{
			return string.Empty;
		}

		public void OnConfirmClick()
		{
			m_SlotConsoleView.OnConfirmClick();
		}

		protected override void Show(bool value)
		{
			base.Show(value);
			
			if (value)
				m_FadeAnimator.AppearAnimation();
			else
				m_FadeAnimator.DisappearAnimation();
		}
	}
}