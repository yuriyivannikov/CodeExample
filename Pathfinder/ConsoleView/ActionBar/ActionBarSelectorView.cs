using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.InfoWindow;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.MVVM._VM.Utility.CanvasSorting;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.VirtualListSystem;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.ActionBar
{
	public class ActionBarSelectorView : ViewBase<ActionBarSelectorVM>
	{
		[SerializeField]
		private TextMeshProUGUI m_Header;
		
		[SerializeField]
		private VirtualListVertical m_VirtualList;
		
		[SerializeField]
		private ActionBarSelectorItemView m_ActionBarSelectorItemView;

		[SerializeField]
		private ConsoleHintsWidget m_HintsWidget;
		
		// [SerializeField]
		// private TooltipSectionView m_TooltipSectionViewRight;
		//
		// [Header("Hints Block")]
		// [SerializeField]
		// private GenericHintView m_SelectHint;
		//
		// [SerializeField]
		// private GenericHintView m_CloseHint;
		//
		// [SerializeField]
		// private GenericHintView m_ActionHint;
		
		[SerializeField]
		private InfoSectionView m_InfoSectionView;

		[SerializeField]
		private CanvasSortingComponent m_CanvasSortingComponent;

		private GridConsoleNavigationBehaviour m_NavigationBehavior;
		private InputLayer m_InputLayer;

		public void Initialize()
		{
			gameObject.SetActive(false);
			
			m_InfoSectionView.Initialize();
			m_VirtualList.Initialize(new VirtualListElementTemplate<ActionBarSelectorItemVM>(m_ActionBarSelectorItemView));
		}

		protected override void BindViewImplementation()
		{
			gameObject.SetActive(true);
			AddDisposable(m_CanvasSortingComponent.PushView());
			
			// m_TooltipSectionViewRight.Bind(ViewModel.TooltipVm);
			
			AddDisposable(ViewModel.Title.Subscribe(title => m_Header.text = UIUtility.GetSaberBookFormat(title)));
			AddDisposable(m_VirtualList.Subscribe(ViewModel.Items));

			m_InfoSectionView.Bind(ViewModel.InfoSectionVM);
			
			m_NavigationBehavior = m_VirtualList.GetNavigationBehaviour();
			AddDisposable(m_VirtualList.AttachedFirstValidView.Subscribe(_ => m_NavigationBehavior.FocusOnFirstValidEntity()));
			AddDisposable(m_NavigationBehavior.DeepestFocusAsObservable.Subscribe(EntityFocused));
			
			GamePad.Instance.PushLayer(m_InputLayer = GetInputLayer());
		}

		private InputLayer GetInputLayer()
		{
			var inputLayer = m_NavigationBehavior.GetInputLayer(new InputLayer {ContextName = "ActionBarSelectorViewInput"});
			
			m_HintsWidget.BindHint(inputLayer.AddButton(_ => ViewModel.OnConfirm(), RewiredConsts.Action.Confirm), UIStrings.Instance.CommonTexts.Confirm);
			m_HintsWidget.BindHint(inputLayer.AddButton(_ => ViewModel.OnDecline(), RewiredConsts.Action.Decline), UIStrings.Instance.CommonTexts.Close);
			m_HintsWidget.BindHint(inputLayer.AddButton(_ => ViewModel.OnFunc01(), RewiredConsts.Action.Func01), UIStrings.Instance.ContextMenu.Use);
		
			inputLayer.AddAxis(Scroll, RewiredConsts.Action.RightStickY, true);
			
			return inputLayer;
		}

		protected override void DestroyViewImplementation()
		{
			if (m_InputLayer != null)
			{
				GamePad.Instance.PopLayer(m_InputLayer);
				m_InputLayer = null;
			}
			
			gameObject.SetActive(false);
		}

		private void Scroll(InputActionEventData obj, float x)
		{
			m_InfoSectionView.Scroll(x);
		}

		public void EntityFocused(IConsoleEntity entity)
		{
			ViewModel.InfoSectionVM.SetTemplate((entity as IHasTooltipTemplate)?.TooltipTemplate());
		}
	}
}