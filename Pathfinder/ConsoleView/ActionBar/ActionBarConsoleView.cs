using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.MVVM._PCView.ActionBar;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.MVVM._VM.ContextMenu;
using Kingmaker.UI.MVVM._VM.Tooltip.Utils;
using Kingmaker.Utility;
using Kingmaker.View;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Rewired;
using RewiredConsts;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.ActionBar
{
	public class ActionBarConsoleView : ActionBarBaseView
	{
		[SerializeField]
		private GameObject m_SpellNameGroup;

		[SerializeField]
		private TextMeshProUGUI m_SpellNameLabel;

		[SerializeField]
		private ConsoleHintsWidget m_HintsWidgetTop;

		[SerializeField]
		private ConsoleHintsWidget m_HintsWidgetBottom;

		[Header("Page Block")]
		[SerializeField]
		private GameObject m_PageGroup;

		[SerializeField]
		private TextMeshProUGUI m_PageLabel;

		[SerializeField]
		private ConsoleHint m_PrevPageHint;

		[SerializeField]
		private ConsoleHint m_NextPageHint;

		//
		// [SerializeField]
		// private GenericHintView m_ShowTooltipHint;
		//
		// [SerializeField]
		// private GenericHintView m_ContextMenuHint;
		//
		// [SerializeField]
		// private GenericHintView m_AbilityVarHint;
		//
		// [SerializeField]
		// private GenericHintView m_CloseHint;

		[SerializeField]
		private ActionBarSelectorView m_ActionBarSelectorView;

		[Header("Positions")]
		[SerializeField]
		private RectTransform m_ActionBarContainerRectTransform;

		[SerializeField]
		private float m_BasePosition;

		[SerializeField]
		private float m_ActivePosition;
		
		[SerializeField]
		private ConsoleHint m_InspectHint;

		private readonly BoolReactiveProperty m_IsConvert = new BoolReactiveProperty();

		private GridConsoleNavigationBehaviour m_NavigationBehaviour;
		private InputLayer m_InputLayer;

		private InputBindStruct m_DeactivateStruct;
		private InputBindStruct m_ContextMenuStruct;
		private InputBindStruct m_RightStickButtonStruct;
		private InputBindStruct m_ConvertStruct;

		public override void Initialize()
		{
			base.Initialize();

			foreach (var slot in m_Slots)
			{
				slot.Initialize();
			}

			m_ActionBarSelectorView.Initialize();
		}

		protected override void BindViewImplementation()
		{
			base.BindViewImplementation();

			AddDisposable(ViewModel.IsActive.Subscribe(SetActive));
			AddDisposable(ViewModel.CurrentPageIndex.Subscribe(SetPageLabel));
			// AddDisposable(ViewModel.IsShown.Subscribe(OnActionBarVisibleChanged));
			// AddDisposable(ViewModel.TBMModeIsActive.Subscribe(OnTurnPanelStateChanged));

			SetupNavigationBehaviour();

			AddDisposable(ViewModel.SelectorVM.Subscribe(vm =>
			{
				if (vm == null)
				{
					if (ViewModel.IsActive.Value)
						m_NavigationBehaviour.FocusOnCurrentEntity();

					return;
				}

				m_NavigationBehaviour.UnFocusCurrentEntity();
				m_ActionBarSelectorView.Bind(vm);
			}));

			AddDisposable(m_WeaponView.IsVisibleWeaponSet.Subscribe(value =>
			{
				if (!value && ViewModel.IsActive.Value)
					m_NavigationBehaviour.FocusOnCurrentEntity();
			}));

			// AddDisposable(ViewModel.AutoUseVm.IsValid.Subscribe(value =>
			// 		{
			// 			if (value)
			// 			{
			// 				return;
			// 			}
			//
			// 			var autoUse = m_NavigationBehaviour.CurrentEntity as ActionBarAutoUseView;
			// 			if (autoUse != null)
			// 			{
			// 				m_NavigationBehaviour.OnRight.Invoke(new InputActionEventData());
			// 			}
			// 		}));

			// AddDisposable(ViewModel.SelectedUnit.Subscribe(unit =>
			// 		{
			// 			var autoUse = m_NavigationBehaviour.CurrentEntity as ActionBarAutoUseView;
			// 			if (autoUse != null)
			// 			{
			// 				if (!autoUse.IsValid())
			// 					m_NavigationBehaviour.OnRight.Invoke(new InputActionEventData());
			//
			// 				return;
			// 			}
			//
			// 			var setView = m_NavigationBehaviour.CurrentEntity as ActionBarCurrentWeaponSetView;
			// 			if (setView == null)
			// 				return;
			//
			// 			if (!unit.Descriptor.IsPet)
			// 				return;
			//
			// 			m_NavigationBehaviour.OnLeft.Invoke(new InputActionEventData());
			// 		}));

			// AddDisposable(ViewModel.CurrentSlot.Subscribe(vm =>
			// 	{
			// 		if (vm == null || !ViewModel.IsActive.Value)
			// 			return;
			//
			// 		var slot = (IConsoleEntityProxy)m_Slots.FirstOrDefault(s => (ActionBarSlotVM)s.GetViewModel() == vm);
			// 		if (slot != null)
			// 			m_NavigationBehaviour.FocusOnEntityManual(slot.ConsoleEntityProxy);
			// 	}));

			AddDisposable(ViewModel.MoveSlotProperty.Subscribe(vm =>
			{
				if (vm == null)
					return;

				SetupSlots();
			}));

			AddDisposable(ViewModel.CurrentSlot.CombineLatest(
				ViewModel.IsActive,
				ViewModel.CurrentPageIndex,
				ViewModel.SelectedUnit,
				(vm, b, arg3, arg4) => true).Subscribe(
				_ =>
				{
					bool isActive = ViewModel.IsActive.Value && ViewModel.CurrentSlot?.Value != null && !ViewModel.CurrentSlot.Value.IsEmpty.Value;
					m_SpellNameGroup.SetActive(isActive);

					if (isActive)
						m_SpellNameLabel.text = ViewModel.CurrentSlot.Value.MechanicActionBarSlot.GetTitle();
				}));

			AddDisposable(ViewModel.SelectedUnit.Skip(1).Subscribe(_ => SetupSlots()));
			AddDisposable(ViewModel.CurrentPageIndex.Skip(1).Subscribe(_ => SetupSlots()));
			SetupSlots();

			AddDisposable(ViewModel.HighlightedUnit.Subscribe(OnHighlightedUnit));
		}

		private void SetupSlots()
		{
			var pageIndex = ViewModel.CurrentPageIndex.Value;
			for (int i = 0; i < m_Slots.Count; i++)
			{
				var slotIndex = (pageIndex - 1) * ActionBarVM.MaxPage + i;
				m_Slots[i].Bind(ViewModel.Slots[slotIndex]);
			}
		}

		private void SetupNavigationBehaviour()
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour(cyclical: new Vector2Int(1, 0));

			AddDisposable(m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity));

			if (m_AutoUseAbilityView is AutoUseAbilityConsoleView autoUseAbilityConsoleView)
				m_NavigationBehaviour.AddEntityHorizontal(autoUseAbilityConsoleView);

			m_Slots.ForEach(slot
				=> m_NavigationBehaviour.AddEntityHorizontal((slot as IConsoleEntityProxy)?.ConsoleEntityProxy));

			if (m_WeaponView is ActionBarWeaponConsoleView actionBarWeaponConsoleView)
				m_NavigationBehaviour.AddEntityHorizontal(actionBarWeaponConsoleView);

			m_InputLayer = GetInputLayer(m_NavigationBehaviour);
			BindHints();
		}

		private InputLayer GetInputLayer(ConsoleNavigationBehaviour navigationBehavior)
		{
			var inputLayer =
				navigationBehavior.GetInputLayer(new InputLayer { ContextName = "ActionBarConsoleViewInput" });

			inputLayer.AddAxis(OnRightStickY, Action.RightStickY, true);

			inputLayer.AddButton(_ => OnAcceptHandler(), Action.Confirm);

			inputLayer.AddButton(_ => ViewModel.OnDeclineClick(), Action.Decline);
			m_DeactivateStruct = inputLayer.AddButton(_ => ViewModel.OnFunc02Click(), Action.Func02);

			m_ConvertStruct = inputLayer.AddButton(_ => OnDPadUp(), Action.DPadUp, m_IsConvert);
			m_ContextMenuStruct = inputLayer.AddButton(_ => OnFunc01Handler(), Action.Func01, ViewModel.IsVisibleContextMenuHint);
			m_RightStickButtonStruct = inputLayer.AddButton(_ => ViewModel.OnRightStickButton(),
				Action.RightStickButton, InputActionEventType.ButtonJustReleased);

			inputLayer.AddButton(OnPrevPage, Action.LeftUp);
			inputLayer.AddButton(OnNextPage, Action.RightUp);
			m_PrevPageHint.BindCustomAction(Action.LeftUp, inputLayer);
			m_NextPageHint.BindCustomAction(Action.RightUp, inputLayer);

			return inputLayer;
		}

		private void BindHints()
		{
			AddDisposable(m_HintsWidgetTop.BindHint(m_ConvertStruct, UIStrings.Instance.ActionBar.Convert));
			AddDisposable(m_HintsWidgetTop.BindHint(m_ContextMenuStruct, UIStrings.Instance.CommonTexts.ContextMenu));
			AddDisposable(m_HintsWidgetBottom.BindHint(m_RightStickButtonStruct,
				UIStrings.Instance.CommonTexts.Information, ConsoleHintsWidget.HintPosition.Left));
			AddDisposable(m_HintsWidgetBottom.BindHint(m_DeactivateStruct, UIStrings.Instance.CommonTexts.Back,
				ConsoleHintsWidget.HintPosition.Right));
		}

		private void OnAcceptHandler()
		{
			if (ViewModel.MoveConfirm())
				return;

			if (!(m_NavigationBehaviour.CurrentEntity is ActionBarSlotConsoleView slot))
				return;

			if (slot.HasConvert)
			{
				TooltipHelper.HideTooltip();
				return;
			}

			if (slot.IsEmpty)
			{
				OnFunc01Handler();
				return;
			}

			ViewModel.IsActive.Value = false;
		}

		private void OnFunc01Handler()
		{
			var auto = m_NavigationBehaviour.CurrentEntity as AutoUseAbilityConsoleView; // ActionBarAutoUseView;
			if (auto != null)
			{
				auto.Clear();
				return;
			}

			if (!(m_NavigationBehaviour.CurrentEntity is IContextMenuOwner slot))
				return;

			ViewModel.OnFunc01Click(slot);
			TooltipHelper.HideTooltip();
		}

		private void OnDPadUp()
		{
			if (!(m_NavigationBehaviour.Focus.Value is ActionBarSlotConsoleView actionBarSlotConsoleView))
				return;

			if (actionBarSlotConsoleView.HasConvert)
			{
				actionBarSlotConsoleView.OnConfirmClick();
				TooltipHelper.HideTooltip();
			}
		}

		private void OnRightStickY(InputActionEventData data, float value)
		{
			ViewModel.OnRightStickY(value);
		}

		private void SetActive(bool value)
		{
			m_ActionBarContainerRectTransform.DOAnchorPosY(value ? m_ActivePosition : m_BasePosition, UIConsts.FadeTime)
				.SetUpdate(true);

			m_SpellNameGroup.SetActive(value);
			m_PageGroup.SetActive(value);

			// foreach (var slotView in m_Slots)
			// {
			// 	slotView.SetTooltipEnabled(!value);
			// }

			if (m_InputLayer == null)
				return;

			if (value)
			{
				GamePad.Instance.PushLayer(m_InputLayer);
				m_NavigationBehaviour.FocusOnCurrentEntity();

				// 	Game.Instance.UI.Common.UISound.Play(UISoundType.ActionBarFocusOn);
			}
			else
			{
				m_NavigationBehaviour.UnFocusCurrentEntity();
				GamePad.Instance.PopLayer(m_InputLayer);

				// Game.Instance.UI.Common.UISound.Play(UISoundType.ActionBarFocusOff);
			}
		}

		private void SetPageLabel(int value)
		{
			m_PageLabel.text = $"{ViewModel.CurrentPageIndex.Value}/{ActionBarVM.MaxPage}";
		}

		private void OnPrevPage(InputActionEventData data)
		{
			ViewModel.OnPrevPage();
			OnFocusEntity(m_NavigationBehaviour.Focus.Value);
		}

		private void OnNextPage(InputActionEventData data)
		{
			ViewModel.OnNextPage();
			OnFocusEntity(m_NavigationBehaviour.Focus.Value);
		}

		public void OnFocusEntity(IConsoleEntity entity)
		{
			if (entity == null)
			{
				TooltipHelper.HideTooltip();
				return;
			}

			// var isAuto = m_NavigationCollection.CurrentEntity as ActionBarAutoUseView;
			// m_ContextMenuHint.SetLabel(isAuto ? UIStrings.Instance.ActionBar.ActionBarClearSlot : UIStrings.Instance.CommonTexts.ContextMenu);

			var hasConvert = false;
			RectTransform tooltipPlace = null;
			switch (entity)
			{
				case ActionBarSlotConsoleView actionBarSlotConsoleView:
					ViewModel.SetCurrentSlot((ActionBarSlotVM)actionBarSlotConsoleView.GetViewModel());
					actionBarSlotConsoleView.SetFocus(true); //todo фокус не отображается при переходе с пустого на заполненный и наоборот!!!
					tooltipPlace = actionBarSlotConsoleView.TooltipPlace;
					hasConvert = actionBarSlotConsoleView.HasConvert;
					break;
				case AutoUseAbilityConsoleView autoUseAbilityConsoleView:
					break;
				case ActionBarWeaponConsoleView actionBarWeaponConsoleView:
					ViewModel.SetCurrentSlot(null);
					break;
			}

			m_IsConvert.Value = hasConvert;

			var view = entity as MonoBehaviour;
			var tooltip = (entity as IHasTooltipTemplate)?.TooltipTemplate();
			view.ShowConsoleTooltip(tooltip, m_NavigationBehaviour, new TooltipConfig(tooltipPlace: tooltipPlace));
		}

		protected override void DestroyViewImplementation()
		{
			base.DestroyViewImplementation();

			if (m_InputLayer == null)
				return;

			GamePad.Instance.PopLayer(m_InputLayer);
			m_InputLayer = null;

			// m_PredictionAppearTween?.Kill();
			// m_PredictionDisappearTween?.Kill();
			// m_PredictionAppearTween = null;
			// m_PredictionDisappearTween = null;
		}

		public void OnHighlightedUnit(UnitEntityView unit)
		{
			bool isActive = unit != null && !unit.Data.IsPlayerFaction && unit.Data.IsPlayersEnemy;
			m_SpellNameGroup.SetActive(isActive);
			m_InspectHint.SetActive(isActive);
			
			if (isActive)
				m_SpellNameLabel.text = unit.Data.CharacterName;
		}
	}
}