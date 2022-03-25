using Kingmaker.UI.MVVM._PCView.ActionBar;
using Kingmaker.UI.MVVM._VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.ActionBar
{
	public class ActionBarBaseSlotConsoleView : ActionBarBaseSlotView, IConsoleEntityProxy
	{
		[Header("Console")]
		[SerializeField]
		private ActionBarSlotConsoleView m_SlotConsoleView;

		[SerializeField]
		private OwlcatMultiSelectable m_ConvertButtonState;

		[SerializeField]
		private OwlcatMultiSelectable m_CountButtonState;

		public IConsoleEntity ConsoleEntityProxy
			=> m_SlotConsoleView;

		private CompositeDisposable m_Disposable = new CompositeDisposable();

		protected override void BindViewImplementation()
		{
			base.BindViewImplementation();

			m_SlotConsoleView.Bind(ViewModel);

			m_ConvertButtonState.SetActiveLayer(ViewModel.HasConvert.Value ? "On" : "Off");
			m_MainButton.ClickSoundType = 0;
			
			AddDisposable(m_SlotConsoleView.SetTooltip(ViewModel.Tooltip, new TooltipConfig(tooltipPlace: m_SlotConsoleView.TooltipPlace)));
			AddDisposable(m_SlotConsoleView.SlotButton.OnLeftClickAsObservable().Subscribe(OnLeftClick));
			AddDisposable(m_SlotConsoleView.SlotButton.OnFocusAsObservable().Subscribe(value => ViewModel.OnHover(value)));
			AddDisposable(m_SlotConsoleView.SlotButton.OnHoverAsObservable().Subscribe(value => ViewModel.OnHover(value)));
		}

		private void OnLeftClick()
		{
			if (ViewModel.HasConvert.Value)
				ViewModel.OnShowConvertRequest();
			else
				ViewModel.OnMainClick();
		}

		protected override void SetResourceCount(int value)
		{
			bool show = value >= 0;

			m_CountButtonState.SetActiveLayer(show ? "On" : "Off");
			m_ResourceCount.text = value.ToString();
		}

		private void OnDestroy()
		{
			m_Disposable.Dispose();
			m_Disposable = null;
		}
	}
}