using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.Settings;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.FullScreenUITypes;
using Kingmaker.UI.MVVM._ConsoleView.Settings.Entities;
using Kingmaker.UI.MVVM._ConsoleView.Settings.Entities.Difficulty;
using Kingmaker.UI.MVVM._ConsoleView.Settings.Menu;
using Kingmaker.UI.MVVM._VM.Settings;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Decorative;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Difficulty;
using Kingmaker.UI.MVVM._VM.Settings.Menu;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Action = RewiredConsts.Action;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings
{
	public class SettingsConsoleView : ViewBase<SettingsVM>
	{
		[SerializeField]
		private SettingsViews m_SettingsViews;
		
		[SerializeField]
		private VirtualListVertical m_VirtualList;
		
		[SerializeField]
		private SettingsDescriptionConsoleView m_DescriptionView;
		
		[SerializeField]
		private SettingsMenuSelectorConsoleView m_MenuSelector;
		
		[Header("Input")]
		[SerializeField] 
		protected ConsoleHintsWidget m_ConsoleHintsWidget;
		
		[Header("Animator")]
		[SerializeField]
		private FadeAnimator m_Animator;

		[SerializeField]
		private Image m_PenImage;
		
		[Header("Controls")]
		[SerializeField]
		private GameObject m_ControlsConsoleGroup;
		
		[SerializeField]
		private SettingsControlConsole m_PSConsoleGroup;
		
		[SerializeField]
		private SettingsControlConsole m_XBoxConsoleGroup;
		
		private bool m_IsShowed;

		private InputBindStruct m_ResetToDefaultStruct;
		private InputBindStruct m_ConfirmStruct;
		private InputBindStruct m_DeclineStruct;
		private InputBindStruct m_ChangeStruct;

		private readonly BoolReactiveProperty m_IsVisibleConfirm = new BoolReactiveProperty();

		public void Initialize()
		{
			gameObject.SetActive(false);
			m_Animator.Initialize();

			m_MenuSelector.Initialize();
			m_DescriptionView.Initialize();
			
			m_SettingsViews.InitializeVirtualList(m_VirtualList);
		}

		protected override void BindViewImplementation()
		{
			m_MenuSelector.Bind(ViewModel.SelectionGroup);
			
			AddDisposable(m_VirtualList.Subscribe(ViewModel.SettingEntities));
			AddDisposable(ViewModel.DescriptionVM.Skip(1).Subscribe(m_DescriptionView.Bind));

			AddDisposable(ViewModel.SelectedMenuEntity.Subscribe(OnSelectedMenuEntity));
			
			var navigationBehavior = m_VirtualList.GetNavigationBehaviour();
			AddDisposable(GamePad.Instance.PushLayer(GetInputLayer(navigationBehavior)));
			AddDisposable(m_VirtualList.AttachedFirstValidView.Subscribe(_ => navigationBehavior.FocusOnFirstValidEntity()));
			
			BindHints();
			Show();
		}

		public void OnSelectedMenuEntity(SettingsMenuEntityVM entity)
		{
			var isConsoleControls = Game.Instance.IsControllerGamepad &&
			                        entity.SettingsScreenType == UISettingsManager.SettingsScreen.Controls;
			                        
			m_DescriptionView.gameObject.SetActive(!isConsoleControls);
			m_VirtualList.gameObject.SetActive(!isConsoleControls);
			m_PenImage.gameObject.SetActive(!isConsoleControls);
			m_ControlsConsoleGroup.SetActive(isConsoleControls);
			
			if (isConsoleControls)
				SetupConsoleControls();
		}

		private void SetupConsoleControls()
		{
			var isXBox = GamePad.Instance.Type == ConsoleType.XBox;
			m_PSConsoleGroup.gameObject.SetActive(!isXBox);
			m_XBoxConsoleGroup.gameObject.SetActive(isXBox);

			var currentInputLayer = GamePad.Instance.CurrentInputLayer;
			
			var binds = isXBox ? m_XBoxConsoleGroup : m_PSConsoleGroup;
			
			AddDisposable(binds.LeftStickButtonHint.BindCustomAction(Action.LeftStickButton, currentInputLayer));
			binds.LeftStickButtonHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlLeftStickButtonHint);
			AddDisposable(binds.DPadRightHint.BindCustomAction(Action.DPadRight , currentInputLayer));
			binds.DPadRightHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadRightHint);
			AddDisposable(binds.DPadDownHint.BindCustomAction(Action.DPadDown , currentInputLayer));
			binds.DPadDownHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadDownHint);
			AddDisposable(binds.DPadLeftHint.BindCustomAction(Action.DPadLeft , currentInputLayer));
			binds.DPadLeftHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadLeftHint);
			AddDisposable(binds.DPadUpHint.BindCustomAction(Action.DPadUp , currentInputLayer));
			binds.DPadUpHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadUpHint);
			AddDisposable(binds.LeftUpHint.BindCustomAction(Action.LeftUp , currentInputLayer));
			binds.LeftUpHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlLeftUpHint);
			AddDisposable(binds.LeftBottomHint.BindCustomAction(Action.LeftBottom , currentInputLayer));
			binds.LeftBottomHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlLeftBottomHint);
			AddDisposable(binds.FuncAdditionalHint.BindCustomAction(Action.FuncAdditional , currentInputLayer));
			binds.FuncAdditionalHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlFuncAdditionalHint);
			AddDisposable(binds.RightBottomHint.BindCustomAction(Action.RightBottom , currentInputLayer));
			binds.RightBottomHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlRightBottomHint);
			AddDisposable(binds.RightUpHint.BindCustomAction(Action.RightUp , currentInputLayer));
			binds.RightUpHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlRightUpHint);
			AddDisposable(binds.OptionsHint.BindCustomAction(Action.Options , currentInputLayer));
			binds.OptionsHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlOptionsHint);
			AddDisposable(binds.Func02Hint.BindCustomAction(Action.Func02 , currentInputLayer));
			binds.Func02Hint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlFunc02Hint);
			AddDisposable(binds.DeclineHint.BindCustomAction(Action.Decline , currentInputLayer));
			binds.DeclineHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDeclineHint);
			AddDisposable(binds.ConfirmHint.BindCustomAction(Action.Confirm , currentInputLayer));
			binds.ConfirmHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlConfirmHint);
			AddDisposable(binds.Func01Hint.BindCustomAction(Action.Func01 , currentInputLayer));
			binds.Func01Hint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlFunc01Hint);
			AddDisposable(binds.RightStickButtonHint.BindCustomAction(Action.RightStickButton , currentInputLayer));
			binds.RightStickButtonHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlRightStickButtonHint);
		}

		private InputLayer GetInputLayer(ConsoleNavigationBehaviour navigationBehavior)
		{
			var inputLayer = navigationBehavior.GetInputLayer(new InputLayer {ContextName = "SettingsConsoleViewInput"});
			AddDisposable(inputLayer.AddButton(null, RewiredConsts.Action.RightStickY));
			
			AddDisposable(m_DeclineStruct = inputLayer.AddButton(_ => ViewModel.Close(), RewiredConsts.Action.Decline));
			AddDisposable(m_ConfirmStruct = inputLayer.AddButton(_ => ViewModel.OpenApplySettingsDialog(), RewiredConsts.Action.Confirm, m_IsVisibleConfirm));
			AddDisposable(m_ResetToDefaultStruct = inputLayer.AddLongPressButton(_ => ViewModel.OpenDefaultSettingsDialog(), RewiredConsts.Action.Func02, InputActionEventType.ButtonLongPressed));

			const int speedScroll = 12;//m_VirtualList.ScrollSettings.ConsoleNavigationScrollSpeed;
			AddDisposable(inputLayer.AddAxis((_, y) => { m_VirtualList.ScrollController.Scroll(- y * speedScroll); }, RewiredConsts.Action.RightStickY));
			
			//AddDisposable(m_ConsoleHintsWidget.CreateCustomHint(new List<int> { RewiredConsts.Action.DPadLeft, RewiredConsts.Action.DPadRight }, inputLayer, UIStrings.Instance.CommonTexts.Change, hintPosition:ConsoleHintsWidget.HintPosition.Left));
			
			AddDisposable(inputLayer.AddButton(_ => m_MenuSelector.OnNext(), RewiredConsts.Action.RightUp));
			AddDisposable(inputLayer.AddButton(_ => m_MenuSelector.OnPrev(), RewiredConsts.Action.LeftUp));
			
			return inputLayer;
		}

		private void Update()
		{
			// проверка массива внутри Update осуществляется только при открытом окне настроек и нагружает систему меньше чем реактивность
			var hasUnconfirmedSettings = SettingsController.HasUnconfirmedSettings();
			if (m_IsVisibleConfirm.Value == hasUnconfirmedSettings)
				return;

			m_IsVisibleConfirm.Value = hasUnconfirmedSettings;
		}

		protected virtual void BindHints()
		{
			AddDisposable(m_ConsoleHintsWidget.BindHint(m_ResetToDefaultStruct, UIStrings.Instance.SettingsUI.ResetToDefaultHold, ConsoleHintsWidget.HintPosition.Center));
			AddDisposable(m_ConsoleHintsWidget.BindHint(m_ConfirmStruct, UIStrings.Instance.CommonTexts.Accept, ConsoleHintsWidget.HintPosition.Right));
			AddDisposable(m_ConsoleHintsWidget.BindHint(m_DeclineStruct, UIStrings.Instance.CommonTexts.Close, ConsoleHintsWidget.HintPosition.Left));
		}

		protected override void DestroyViewImplementation()
		{
			Hide();
		}

		private void Show()
		{
			if (m_IsShowed)
				return;
			
			m_IsShowed = true;
			
			m_Animator.AppearAnimation();
			EventBus.RaiseEvent<IFullScreenUIHandler>(h => h.HandleFullScreenUiChanged(true, FullScreenUIType.Settings));
			UISoundController.Instance.Play(UISoundType.SettingsOpen);
		}

		public void Hide()
		{
			if (!m_IsShowed)
				return;

			m_Animator.DisappearAnimation(() =>
				{
					gameObject.SetActive(false);
					m_IsShowed = false;
				});
			
			EventBus.RaiseEvent<IFullScreenUIHandler>(h => h.HandleFullScreenUiChanged(false, FullScreenUIType.Settings));
			UISoundController.Instance.Play(UISoundType.SettingsClose);
		}

		[Serializable]
		public class SettingsViews
		{
			[SerializeField]
			private SettingsEntityHeaderConsoleView m_SettingsEntityHeaderViewPrefab;
			
			[SerializeField]
			private SettingsEntityBoolConsoleView m_SettingsEntityBoolViewPrefab;

			[SerializeField]
			private SettingsEntityDropdownConsoleView m_SettingsEntityDropdownViewPrefab;

			[SerializeField]
			private SettingsEntitySliderConsoleView m_SettingsEntitySliderViewPrefab;
			
			[SerializeField]
			private SettingsEntityDropdownDisplayModeConsoleView m_SettingsEntityDropdownDisplayModeViewPrefab;

			[SerializeField]
			private SettingsEntityGameDifficultyConsoleView SettingsEntityGameDifficultyViewPrefab;

			[SerializeField]
			private SettingsEntitySliderVisualPerceptionConsoleView m_SettingsEntitySliderVisualPerceptionViewPrefab;

			[SerializeField]
			private SettingsEntitySliderVisualPerceptionWithImagesConsoleView m_SettingsEntitySliderVisualPerceptionWithImagesViewPrefab;

			[SerializeField]
			private SettingsEntityStatisticsOptOutConsoleView m_SettingsEntityStatisticsOptOutViewPrefab;

			[SerializeField]
			private SettingEntityKeyBindingConsoleView m_SettingEntityKeyBindingViewPrefab;
			public void InitializeVirtualList(VirtualListComponent virtualListComponent)
			{
				virtualListComponent.Initialize(
					new VirtualListElementTemplate<SettingsEntityHeaderVM>(m_SettingsEntityHeaderViewPrefab),
					new VirtualListElementTemplate<SettingsEntityBoolVM>(m_SettingsEntityBoolViewPrefab),
					new VirtualListElementTemplate<SettingsEntityDropdownVM>(m_SettingsEntityDropdownViewPrefab, SettingsEntityDropdownVM.DefaultDropdownIndex),
					new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderViewPrefab, SettingsEntitySliderVM.DefaultSliderIndex),
					new VirtualListElementTemplate<SettingEntityKeyBindingVM>(m_SettingEntityKeyBindingViewPrefab),
					
					new VirtualListElementTemplate<SettingsEntityDropdownVM>(m_SettingsEntityDropdownDisplayModeViewPrefab, SettingsEntityDropdownVM.DisplayModeDropdownIndex),
					new VirtualListElementTemplate<SettingsEntityDropdownGameDifficultyVM>(SettingsEntityGameDifficultyViewPrefab, SettingsEntityDropdownVM.DefaultDropdownIndex),
					new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderVisualPerceptionViewPrefab, SettingsEntitySliderVM.VisualPerceptionSliderIndex),
					new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderVisualPerceptionWithImagesViewPrefab, SettingsEntitySliderVM.VisualPerceptionWithImagesSliderIndex),
					new VirtualListElementTemplate<SettingsEntityStatisticsOptOutVM>(m_SettingsEntityStatisticsOptOutViewPrefab));
			}
		}
	}
}