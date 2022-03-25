using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.MVVM._VM.ContextMenu;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.ActionBar
{
	public class ActionBarSlotConsoleView : ViewBase<ActionBarSlotVM>, IConsoleNavigationEntity, IHasTooltipTemplate, IConfirmClickHandler, IContextMenuOwner
	{
		[SerializeField]
		private OwlcatMultiButton m_Button;
		
		[SerializeField]
		private RectTransform m_TooltipPlace;

		public RectTransform TooltipPlace => m_TooltipPlace;

		public string SpellName => ViewModel.Name.Value;
		public bool IsEmpty => ViewModel.IsEmpty.Value;
		public bool HasConvert => ViewModel.HasConvert.Value;
		public OwlcatMultiButton SlotButton => m_Button;
		
		protected override void BindViewImplementation()
		{
		}

		protected override void DestroyViewImplementation()
		{
		}

		public void SetFocus(bool value)
		{
			m_Button.SetFocus(value);
		}

		public bool IsValid()
		{
			return m_Button.IsValid() && IsBinded/* && !ViewModel.IsEmpty.Value*/;
		}

		public TooltipBaseTemplate TooltipTemplate()
		{
			return ViewModel.Tooltip.Value;
		}

		public bool CanConfirmClick()
		{
			return IsValid();
		}

		public string GetConfirmClickHint()
		{
			return string.Empty;
		}

		public void OnConfirmClick()
		{
			ViewModel.OnMainClick();
		}

		public RectTransform GetOwnerRectTransform()
		{
			return transform as RectTransform;
		}
	}
}