using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.Settings;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Decorative;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Difficulty;
using Kingmaker.UI.MVVM._VM.Settings.KeyBindSetupDialog;
using Kingmaker.UI.MVVM._VM.Settings.Menu;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UI.Tooltip;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.Settings
{
	public class SettingsVM : BaseDisposable, IViewModel, IKeyBindingSetupDialogHandler, ISettingsDescriptionUIHandler, IOptionsWindowUIHandler
	{
		private readonly Action m_CloseAction;
		
		public readonly SelectionGroupRadioVM<SettingsMenuEntityVM> SelectionGroup;
		
		private readonly ReactiveProperty<SettingsMenuEntityVM> m_SelectedMenuEntity = new ReactiveProperty<SettingsMenuEntityVM>();
		public ReactiveProperty<SettingsMenuEntityVM> SelectedMenuEntity => m_SelectedMenuEntity;
		
		private readonly List<SettingsMenuEntityVM> m_MenuEntitiesList = new List<SettingsMenuEntityVM>();
		
		public readonly ReactiveProperty<SettingsDescriptionVM> DescriptionVM = new ReactiveProperty<SettingsDescriptionVM>();

		private readonly ReactiveCollection<VirtualListElementVMBase> m_SettingEntities = new ReactiveCollection<VirtualListElementVMBase>();
		public IReadOnlyReactiveCollection<VirtualListElementVMBase> SettingEntities => m_SettingEntities;
		
		private readonly ReactiveProperty<KeyBindingSetupDialogVM> m_CurrentKeyBindDialog = new ReactiveProperty<KeyBindingSetupDialogVM>();
		public IReadOnlyReactiveProperty<KeyBindingSetupDialogVM> CurrentKeyBindDialog => m_CurrentKeyBindDialog;

		public readonly BoolReactiveProperty IsDefaultButtonInteractable = new BoolReactiveProperty();
		public readonly BoolReactiveProperty IsApplyButtonInteractable = new BoolReactiveProperty();
		public readonly BoolReactiveProperty IsCancelButtonInteractable = new BoolReactiveProperty();

		private SettingsMenuEntityVM m_PreviousSelectedMenuEntity;

		public SettingsVM(Action closeAction, bool isMainMenu = false)
		{
			m_CloseAction = closeAction;

			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameGame, UISettingsManager.SettingsScreen.Game);
			
			if (!isMainMenu)
				CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameDifficulty, UISettingsManager.SettingsScreen.Difficulty);	
			
			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameControls, UISettingsManager.SettingsScreen.Controls);
			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameGraphics, UISettingsManager.SettingsScreen.Graphics);
			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameSound, UISettingsManager.SettingsScreen.Sound);
			
			AddDisposable(SelectionGroup = new SelectionGroupRadioVM<SettingsMenuEntityVM>(m_MenuEntitiesList, m_SelectedMenuEntity));
			m_SelectedMenuEntity.Value = m_MenuEntitiesList.FirstOrDefault();
			SetSettingsList(m_SelectedMenuEntity.Value.SettingsScreenType);
			
			AddDisposable(EventBus.Subscribe(this));
		}

		private void CreateMenuEntity(LocalizedString localizedString, UISettingsManager.SettingsScreen screenType)
		{
			var entityVM = new SettingsMenuEntityVM(localizedString, screenType, SetSettingsList);
			AddDisposable(entityVM);
			m_MenuEntitiesList.Add(entityVM);
		}

		private void SetSettingsList(UISettingsManager.SettingsScreen settingsScreen)
		{
			if (m_PreviousSelectedMenuEntity == m_SelectedMenuEntity.Value)
				return;

			if (SettingsController.HasUnconfirmedSettings())
			{
				OnChangeSettingsList(button =>
				{
					OnApplyDialogAnswer(button);
					SwitchSettingsScreen(settingsScreen);
				});
			}
			else
			{
				SwitchSettingsScreen(settingsScreen);
			}

			IsDefaultButtonInteractable.Value = settingsScreen == UISettingsManager.SettingsScreen.Game ||
			                                    settingsScreen == UISettingsManager.SettingsScreen.Controls ||
			                                    settingsScreen == UISettingsManager.SettingsScreen.Sound;
			
			m_PreviousSelectedMenuEntity = m_SelectedMenuEntity.Value;
		}

		private void SwitchSettingsScreen(UISettingsManager.SettingsScreen settingsScreen)
		{
			m_SettingEntities.Clear();
			
			if (Game.Instance.IsControllerGamepad && settingsScreen == UISettingsManager.SettingsScreen.Controls)
				return;

			foreach (UISettingsGroup uiSettingsGroup in Game.Instance.UISettingsManager.GetSettingsList(settingsScreen))
			{
				m_SettingEntities.Add(new SettingsEntityHeaderVM(uiSettingsGroup.Title));
				foreach (UISettingsEntityBase uiSettingsEntityBase in uiSettingsGroup.SettingsList)
				{
					m_SettingEntities.Add(GetVMForSettingsItem(uiSettingsEntityBase));
				}
			}
		}

		public static VirtualListElementVMBase GetVMForSettingsItem(UISettingsEntityBase uiSettingsEntity)
		{
			switch (uiSettingsEntity)
			{
				case UISettingsEntityGameDifficulty uiSettingsEntityGameDifficulty:
					return new SettingsEntityDropdownGameDifficultyVM(uiSettingsEntityGameDifficulty);
				case UISettingsEntityDropdownFullScreenMode uiSettingsEntityDropdownFullScreenMode:
					return new SettingsEntityDropdownVM(uiSettingsEntityDropdownFullScreenMode, SettingsEntityDropdownVM.DropdownType.DisplayMode);
				case UISettingsEntityVisualPerception uiSettingsEntityVisualPerception:
					return new SettingsEntitySliderVM(uiSettingsEntityVisualPerception, SettingsEntitySliderVM.SliderType.VisualPerception);
				case UISettingsEntityVisualPerceptionWithImages uiSettingsEntityVisualPerceptionWithImages:
					return new SettingsEntitySliderVM(uiSettingsEntityVisualPerceptionWithImages, SettingsEntitySliderVM.SliderType.VisualPerceptionWithImages);
				case UISettingsEntityOptOut uiSettingsEntityStatisticsOptOut:
					return new SettingsEntityStatisticsOptOutVM(uiSettingsEntityStatisticsOptOut);
				
				case UISettingsEntityBool uiSettingsEntityBool:
					return new SettingsEntityBoolVM(uiSettingsEntityBool);
				case IUISettingsEntityDropdown uiSettingsEntityDropdown:
					return new SettingsEntityDropdownVM(uiSettingsEntityDropdown);
				case IUISettingsEntitySlider uiSettingsEntitySlider:
					return new SettingsEntitySliderVM(uiSettingsEntitySlider);
				case UISettingsEntityKeyBinding uiSettingsEntityKeybind:
					return new SettingEntityKeyBindingVM(uiSettingsEntityKeybind);
				default: 
					UberDebug.LogError($"Error: SettingsVM: GetVMForSettingsItem: uiSettingsEntity {uiSettingsEntity} is undefined");
					break;
			}

			return null;
		}

		public void OpenKeyBindingSetupDialog(UISettingsEntityKeyBinding uiSettingsEntity, int bindingIndex)
		{
			m_CurrentKeyBindDialog.Value = new KeyBindingSetupDialogVM(uiSettingsEntity, bindingIndex, CloseKeyBindSetupDialog);
			AddDisposable(m_CurrentKeyBindDialog.Value);
		}

		private void CloseKeyBindSetupDialog()
		{
			RemoveDisposable(m_CurrentKeyBindDialog.Value);
			m_CurrentKeyBindDialog.Value.Dispose();
			m_CurrentKeyBindDialog.Value = null;
		}

		protected override void DisposeImplementation()
		{
			HandleHideSettingsDescription();
			SettingsController.SaveAll();
		}

		public void ApplyAndClose()
		{
			if (SettingsController.HasUnconfirmedSettings())
				ApplySettings();

			m_CloseAction?.Invoke();
		}
		
		public void Close()
		{
			if (SettingsController.HasUnconfirmedSettings())
			{
				OnChangeSettingsList(OnCloseDialogAnswer);
				return;
			}

			m_CloseAction?.Invoke();
		}
		
		private void OnChangeSettingsList(Action<MessageModalBase.ButtonType> OnApplyDialogAction)
		{
			var questionText = string.Format(Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SettingsUI.SaveChangesMessage, m_PreviousSelectedMenuEntity.Title);
			var yesText = UIStrings.Instance.SettingsUI.DialogSave;
			var noText = UIStrings.Instance.SettingsUI.DialogRevert;

			EventBus.RaiseEvent<IMessageModalUIHandler>(
				w => w.HandleOpen(
					questionText,
					MessageModalBase.ModalType.Dialog,
					OnApplyDialogAction,
					null,
					yesText,
					noText));
		}
		
		public void OpenApplySettingsDialog()
		{
			if (!SettingsController.HasUnconfirmedSettings())
				return;

			OnChangeSettingsList(OnApplyDialogAnswer);
		}

		private void OnApplyDialogAnswer(MessageModalBase.ButtonType buttonType)
		{
			if (buttonType == MessageModalBase.ButtonType.Yes)
				ApplySettings();
			else
				RevertSettings();
		}

		private void OnCloseDialogAnswer(MessageModalBase.ButtonType buttonType)
		{
			if (buttonType == MessageModalBase.ButtonType.Yes)
				ApplySettings();
			else
				RevertSettings();

			m_CloseAction?.Invoke();
		}

		private void ApplySettings()
		{
			SettingsController.ConfirmAllTempValues();
			SettingsController.SaveAll();
			// SettingsListScreen.RefreshUI();
			HandleItemChanged();
			Game.Instance.UISettingsManager.OnSettingsApplied();
		}

		private void RevertSettings()
		{
			SettingsController.RevertAllTempValues();
			HandleItemChanged();
		}

		public void OpenDefaultSettingsDialog()
		{
			var text = string.Format(Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SettingsUI.RestoreAllDefaultsMessage, m_SelectedMenuEntity.Value.Title);
			EventBus.RaiseEvent<IMessageModalUIHandler>(w => w.HandleOpen(text, MessageModalBase.ModalType.Dialog, OnDefaultDialogAnswer));
		}
		
		private void OnDefaultDialogAnswer(MessageModalBase.ButtonType buttonType)
		{
			if (buttonType != MessageModalBase.ButtonType.Yes)
				return; 
		
			SettingsController.ResetToDefault(m_SelectedMenuEntity.Value.SettingsScreenType);
			HandleItemChanged();
		}
		
		public void OpenCancelSettingsDialog()
		{
			var text = string.Format(Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SettingsUI.CancelChangesMessage, m_SelectedMenuEntity.Value.Title);
			EventBus.RaiseEvent<IMessageModalUIHandler>(w => w.HandleOpen(text, MessageModalBase.ModalType.Dialog, OnCancelDialogAnswer));
		}

		private void OnCancelDialogAnswer(MessageModalBase.ButtonType buttonType)
		{
			if (buttonType != MessageModalBase.ButtonType.Yes)
				return;
			
			RevertSettings();
		}

		public void HandleDescriptionChanged(TooltipData data)
		{
			// ?
		}

		public void HandleItemChanged()
		{
			IsApplyButtonInteractable.Value = SettingsController.HasUnconfirmedSettings();
			IsCancelButtonInteractable.Value = SettingsController.HasUnconfirmedSettings();
		}

		public void HandleShowSettingsDescription(string title, string description)
		{
			DescriptionVM.Value?.Dispose();
			DescriptionVM.Value = new SettingsDescriptionVM(title, description);
		}

		public void HandleHideSettingsDescription()
		{
			DescriptionVM.Value?.Dispose();
			DescriptionVM.Value = null;
		}
	}
}