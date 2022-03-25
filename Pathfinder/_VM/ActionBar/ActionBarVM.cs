using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI._ConsoleUI.TurnBasedMode;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.ActionBar;
using Kingmaker.UI.MVVM._VM.ContextMenu;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using Owlcat.Runtime.UI.MVVM;
using TurnBased.Controllers;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class IndexedMechanicSlot
	{
		public int Index;
		public MechanicActionBarSlot Slot;
	}
	
	public enum ActionBarGroupType
	{
		None,
		Spell,
		Ability,
		Item
	}

	public class ActionBarVM
		: BaseDisposable, IViewModel,
			IUnitEquipmentHandler,
			ISpellBookUIHandler,
			IPlayerAbilitiesHandler,
			ISpellBookRest,
			ISpellBookCustomSpell,
			ITurnBasedModeHandler,
			ITurnBasedModeEnabledHandler,
			ILevelUpCompleteUIHandler,
			IFactCollectionUpdatedHandler,
			ISelectionHandler,
			IPartyCombatHandler,
			IUnitCommandActHandler,
			IUnitCommandEndHandler,
			IActionBarSelectorHolder,
			ISlotWasAddedHandler,
			IUnitDirectHoverUIHandler
	
	{
		public readonly KineticBurnVM KineticBurnVM;
		public readonly LocustSwarmVM LocustSwarmVM;
		public readonly AutoUseAbilityVM AutoUseAbilityVM;
		public readonly ControlCharactersVM ControlCharactersVM;
		public readonly ActionBarWeaponVM WeaponVM;

		public readonly List<ActionBarSlotVM> Slots = new List<ActionBarSlotVM>();

		public readonly List<ActionBarSlotVM> GroupSpells = new List<ActionBarSlotVM>();
		public readonly List<ActionBarSlotVM> GroupAbilities = new List<ActionBarSlotVM>();
		public readonly List<ActionBarSlotVM> GroupItems = new List<ActionBarSlotVM>();

		public readonly ReactiveProperty<UnitEntityData> SelectedUnit = new ReactiveProperty<UnitEntityData>();
		public readonly ReactiveCommand OnUnitUpdated = new ReactiveCommand();

		public ReactiveProperty<PredictionPanelVM> PredictionPanelVm = new ReactiveProperty<PredictionPanelVM>();
		public readonly ReactiveCommand OnTBMStateUpdated = new ReactiveCommand();
		public readonly ActionBarVisibilityStateVM ActionBarVisibilityStateVM;
		public readonly ReactiveProperty<Ability> DimensionDoor = new ReactiveProperty<Ability>(null);
		public readonly ReactiveProperty<ActionBarSlotVM> DimensionDoorSlotVM = new ReactiveProperty<ActionBarSlotVM>(null);

	#region Console

		public const int MaxPage = 10;

		private readonly ReactiveProperty<bool> m_ShowTooltip = new ReactiveProperty<bool>();
		
		public readonly BoolReactiveProperty IsActive = new BoolReactiveProperty();

		// public IReadOnlyReactiveProperty<bool> IsShown => m_IsShown;
		// private readonly ReactiveProperty<bool> m_IsShown = new ReactiveProperty<bool>(true);
		// public readonly BoolReactiveProperty TBMModeIsActive = new BoolReactiveProperty();
		
		public IReadOnlyReactiveProperty<int> CurrentPageIndex => m_CurrentPageIndex;
		private readonly ReactiveProperty<int> m_CurrentPageIndex = new ReactiveProperty<int>(1);
		
		private ActionBarSlotVM CurrentSlotValue => CurrentSlot.Value;
		public readonly ReactiveProperty<ActionBarSlotVM> CurrentSlot = new ReactiveProperty<ActionBarSlotVM>();
		
		public IReadOnlyReactiveProperty<ActionBarSelectorVM> SelectorVM => m_SelectorVM;
		private readonly ReactiveProperty<ActionBarSelectorVM> m_SelectorVM = new ReactiveProperty<ActionBarSelectorVM>();
		
		public readonly ReactiveProperty<bool> TooltipEnabled = new BoolReactiveProperty();
		public readonly ReactiveProperty<bool> IsVisibleContextMenuHint = new BoolReactiveProperty();
		
		public readonly ReactiveProperty<ActionBarSlotVM> MoveSlotProperty = new ReactiveProperty<ActionBarSlotVM>();
		
		public readonly ActionBarCurrentWeaponSetVM CurrentWeaponSetVM;
		
		private bool m_TempShowTooltip;

		public ReactiveProperty<UnitEntityView> HighlightedUnit = new ReactiveProperty<UnitEntityView>();

	#endregion

		private MechanicActionBarSlotEmpty EmptySlot
			=> m_EmptySlot ?? (m_EmptySlot = new MechanicActionBarSlotEmpty());

		private MechanicActionBarSlotEmpty m_EmptySlot;

		// TODO: move List<ActionBarSlotVM> and state to separate VM
		private readonly Dictionary<ActionBarGroupType, bool> m_GroupState = new Dictionary<ActionBarGroupType, bool>()
		{
			{ActionBarGroupType.Ability, false},
			{ActionBarGroupType.Item, false},
			{ActionBarGroupType.Spell, false},
		};

		private int m_CurrentSpellLevel;

		private bool m_NeedReset;
		private bool m_NeedUpdateSelection;

		private UnitEntityData SelectedUnitValue
			=> SelectedUnit.Value;

		private UnitEntityData MainCharacter
			=> Game.Instance.Player.MainCharacter.Value;

		public ActionBarVM()
		{
			CheckDimensionDoor();
			
			for (int i = 0; i < UIConsts.ActionSlotsCount; i++)
			{
				Slots.Add(new ActionBarSlotVM(i));
			}

			AddDisposable(KineticBurnVM = new KineticBurnVM(SelectedUnit));
			AddDisposable(LocustSwarmVM = new LocustSwarmVM(SelectedUnit));
			AddDisposable(AutoUseAbilityVM = new AutoUseAbilityVM(SelectedUnit));
			AddDisposable(ControlCharactersVM = new ControlCharactersVM());
			AddDisposable(WeaponVM = new ActionBarWeaponVM(SelectedUnit));

			AddDisposable(SelectedUnit.Subscribe(OnUnitChanged));
			AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(_ => OnUpdateHandler()));
			AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(_ => OnLateUpdateHandler()));
			
			AddDisposable(ActionBarVisibilityStateVM =
				new ActionBarVisibilityStateVM(SelectedUnit, OnUnitUpdated, PredictionPanelVm, OnTBMStateUpdated));
			AddDisposable(ActionBarVisibilityStateVM.ActionBarIsVisible.Skip(1).Subscribe(_ => CloseAllConverts()));
			AddDisposable(ActionBarVisibilityStateVM.AdditionalBarPosition.Skip(1).Subscribe(_ => CloseAllConverts()));
			
			AddDisposable(DimensionDoor.Subscribe(value =>
			{
				DimensionDoorSlotVM.Value?.Dispose();

				if (value == null)
				{
					DimensionDoorSlotVM.Value = null;
					return;
				}
				
				DimensionDoorSlotVM.Value = new ActionBarSlotVM(new MechanicActionBarSlotAbility {Ability = value.Data, Unit = MainCharacter});
				DimensionDoorSlotVM.Value.CanUseOnAllCharacters = true;
			}));

			AddDisposable(EventBus.Subscribe(this));
			
			// AddDisposable(AutoUseVm = new ActionBarAutoUseVm(SetSlotSelected, m_ShowTooltip, Active));
			AddDisposable(CurrentWeaponSetVM = new ActionBarCurrentWeaponSetVM(SelectedUnit/*, TBMModeIsActive*/));
			// AddDisposable(InspectVm = new ActionBarInspectVM());

			// for (int i = 0; i < UIConsts.ActionSlotsCountMainBar; i++)
			// {
			// 	var slotVm = Slots[i];
			// 	CurrentPage.Add(slotVm);
			// 	
			// 	AddDisposable(slotVm.OnClickCommand.Subscribe(
			// 		ignore =>
			// 		{
			// 			if (ignore)
			// 				return;
			//
			// 			IsActive.Value = false;
			// 		}));
			// }
			
			AddDisposable(CurrentPageIndex.Subscribe(_ => OnCurrentPageChange()));
			AddDisposable(CurrentSlot.Subscribe(_ => OnCurrentSlotChange()));
			AddDisposable(IsActive.Subscribe(SetActive));
			// AddDisposable(m_ShowTooltip.Subscribe(value => CurrentSlotValue?.SetTooltip(value)));
			
			AddDisposable(MoveSlotProperty.Subscribe(
				vm =>
				{
					// AutoUseAbilityVM.Enabled = vm == null;
					CurrentWeaponSetVM.Enabled = vm == null;
					
					// for (int i = 0; i < MaxPage; i++)
					// {
					// 	var slot = Slots[CurrentPageIndex.Value * MaxPage + i];
					// 	slot.SetMove(vm);
					// }
				}));
		}

		private void CheckDimensionDoor()
		{
			DimensionDoor.Value = MainCharacter.Abilities.GetAbility(UIRoot.Instance.DimensionDoorMassDLCAbility);
		}

		public void CloseAllConverts()
		{
			foreach (var slot in Slots)
			{
				slot.CloseConvert();
			}

			foreach (var slot in GroupSpells)
			{
				slot.CloseConvert();
			}

			foreach (var slot in GroupAbilities)
			{
				slot.CloseConvert();
			}

			foreach (var slot in GroupItems)
			{
				slot.CloseConvert();
			}
		}

		public void UpdateSpellLevel(int spellLevel)
		{
			m_CurrentSpellLevel = spellLevel;
		}

		public void UpdateGroupState(ActionBarGroupType type, bool visible)
		{
			m_GroupState[type] = visible;
			CloseAllConverts();
		}

		protected override void DisposeImplementation()
		{
			Slots.ForEach(s => s.Dispose());
			Slots.Clear();

			GroupClear();

			DisposePredictionPanel();
			
			DimensionDoorSlotVM.Value?.Dispose();
			
			IsActive.Value = false;
			
			// CurrentPage.ForEach(s => s.Dispose());
			// CurrentPage.Clear();
			//
			// m_Pages.Clear();

			HideSelectorVM();
		}

		private void CheckPredictionPanel(UnitEntityData unit)
		{
			if (UIUtility.GetGroup(false, true).Contains(unit))
			{
				if (!Game.Instance.Player.IsInCombat || !CombatController.IsInTurnBasedCombat())
				{
					return;
				}

				if (PredictionPanelVm.Value == null ||
				    PredictionPanelVm.Value != null && PredictionPanelVm.Value.Unit != unit)
				{
					PredictionPanelVm.Value?.Dispose();
					PredictionPanelVm.Value = new PredictionPanelVM(unit, new BoolReactiveProperty(true));
				}

				OnTBMStateUpdated.Execute();
			}
			else
			{
				DisposePredictionPanel();
			}
		}

		private void DisposePredictionPanel()
		{
			PredictionPanelVm.Value?.Dispose();
			PredictionPanelVm.Value = null;
		}

		private void OnUpdateHandler()
		{
			using (ProfileScope.New("ActionBarVM OnUpdateHandler"))
			{
				if (LoadingProcess.Instance.IsLoadingInProcess)
				{
					return;
				}

				if (m_NeedUpdateSelection)
				{
					UpdateSelection();
					m_NeedUpdateSelection = false;
				}

				bool isDirty = SelectedUnitValue != null && SelectedUnitValue.UISettings.Dirty;
				bool isUnitInGame = SelectedUnitValue?.IsInGame ?? false;
				bool isGameModeSuitable = Game.Instance.CurrentMode == GameModeType.Default ||
				                          Game.Instance.CurrentMode == GameModeType.Pause;

				if ((m_NeedReset || isDirty) && isGameModeSuitable && isUnitInGame)
				{
					OnUnitChanged(SelectedUnitValue);
					m_NeedReset = false;
				}
			}
		}

		private void OnLateUpdateHandler()
		{
			if (SelectedUnitValue == null)
				return;
			
			for (int i = 0; i < UIConsts.ActionSlotsCountMainBar * (1 + SelectedUnitValue.UISettings.Phase); i++)
			{
				Slots[i].UpdateResource();
			}

			if (m_GroupState[ActionBarGroupType.Spell])
			{
				foreach (var slotVM in GroupSpells)
				{
					if (slotVM.SpellLevel == m_CurrentSpellLevel)
						slotVM.UpdateResource();
				}
			}
			if (m_GroupState[ActionBarGroupType.Ability])
			{
				foreach (var slotVM in GroupAbilities)
				{
					slotVM.UpdateResource();
				}
			}
			if (m_GroupState[ActionBarGroupType.Item])
			{
				foreach (var slotVM in GroupItems)
				{
					slotVM.UpdateResource();
				}
			}
			
			AutoUseAbilityVM?.SlotVM.UpdateResource();
			DimensionDoorSlotVM.Value?.UpdateResource();
		}

		private void GroupClear(bool updateMechanicSlots = true)
		{
			if (updateMechanicSlots)
			{
				SetMechanicSlots(null);
			}

			GroupSpells.ForEach(g => g.Dispose());
			GroupSpells.Clear();

			GroupAbilities.ForEach(g => g.Dispose());
			GroupAbilities.Clear();

			GroupItems.ForEach(g => g.Dispose());
			GroupItems.Clear();
		}

		private void OnUnitChanged(UnitEntityData unit)
		{
			using (ProfileScope.New("ActionBarVM UpdateSelection"))
			{
				m_CurrentSpellLevel = 0;
				GroupClear(unit == null);

				if (unit != null)
				{
					using (ProfileScope.New("ActionBarVM TryToInitialize"))
					{
						unit.UISettings.TryToInitialize();
					}

					using (ProfileScope.New("ActionBarVM SetMechanicSlots"))
					{
						SetMechanicSlots(unit);
					}

					using (ProfileScope.New("ActionBarVM CollectSpells"))
					{
						CollectSpells(unit);
					}

					using (ProfileScope.New("ActionBarVM CollectAbilities"))
					{
						CollectAbilities(unit);
					}

					using (ProfileScope.New("ActionBarVM CollectItems"))
					{
						CollectItems(unit);
					}
				}
				
				OnUnitUpdated.Execute();
				
				if (unit == null)
					return;
				
				SetMoveSlot(null);
				HideSelectorVM();
			
				unit.UISettings.TryToInitialize(); // unit.UISettings.TryToInitialize(unit);

				// ReInitPages();

				// if (m_CurrentPageIndex.Value == unit.UISettings.ConsoleActionBarPageIndex)
				// {
				// 	OnCurrentPageChanges();
				// }
				// else
				// {
					
				// }
			
				SetCurrentSlot(Slots[unit.UISettings.ConsoleActionBarSlotIndex]);
				m_CurrentPageIndex.Value = unit.UISettings.ConsoleActionBarPageIndex;
				
				EventBus.RaiseEvent<IContextMenuHandler>(h => h.HandleContextMenuRequest(null));
			}
		}

		private void SetMechanicSlots(UnitEntityData unit)
		{
			for (int i = 0; i < Slots.Count; i++)
			{
				Slots[i].SetMechanicSlot(unit == null ? EmptySlot : unit.UISettings.GetSlot(i, unit));
			}
		}

		private void CollectSpells(UnitEntityData unit)
		{
			var alreadyContains = new List<AbilityData>();
			foreach (var spellBook in unit.Spellbooks)
			{
				for (int spellLevel = spellBook.MaxSpellLevel; spellLevel >= 0; spellLevel--)
				{
					if (spellBook.Blueprint.Spontaneous || spellLevel < 1)
					{
						var knownSpells = spellBook.GetSpecialSpells(spellLevel)
							.Concat(spellBook.GetKnownSpells(spellLevel)).Distinct().ToList();
						var customSpells = spellBook.GetCustomSpells(spellLevel).ToList();
						var spells = knownSpells;
						spells.AddRange(customSpells);

						foreach (var spell in spells)
						{
							if (alreadyContains.Contains(spell))
							{
								continue;
							}

							alreadyContains.Add(spell);
							GroupSpells.Add(new ActionBarSlotVM(
								new MechanicActionBarSlotSpontaneousSpell(spell) { Unit = unit },
								spellLevel: spellLevel));
						}
					}
					else
					{
						var spells = spellBook.GetMemorizedSpells(spellLevel);
						foreach (var spell in spells)
						{
							if (alreadyContains.Contains(spell.Spell))
							{
								continue;
							}

							alreadyContains.Add(spell.Spell);
							GroupSpells.Add(new ActionBarSlotVM(
								new MechanicActionBarSlotMemorizedSpell(spell) { Unit = unit },
								spellLevel: spellLevel));
						}
					}
				}
			}
		}

		private void CollectItems(UnitEntityData unit)
		{
			foreach (var quickSlot in unit.Body.QuickSlots)
			{
				if (!quickSlot.HasItem)
				{
					continue;
				}

				GroupItems.Add(
					new ActionBarSlotVM(new MechanicActionBarSlotItem { Item = quickSlot.Item, Unit = unit }));
			}
		}

		private void CollectAbilities(UnitEntityData unit)
		{
			foreach (var ability in unit.Abilities)
			{
				if (ability.Hidden || ability.Blueprint.IsCantrip)
				{
					continue;
				}

				GroupAbilities.Add(new ActionBarSlotVM(new MechanicActionBarSlotAbility
					{ Ability = ability.Data, Unit = unit }));
			}

			foreach (var ability in unit.ActivatableAbilities)
			{
				GroupAbilities.Add(new ActionBarSlotVM(new MechanicActionBarSlotActivableAbility
					{ ActivatableAbility = ability, Unit = unit }));
			}
		}

		void IUnitEquipmentHandler.HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
		{
		}

		void ISpellBookUIHandler.HandleMemorizedSpell(AbilityData data, UnitDescriptor owner)
		{
			m_NeedReset = m_NeedReset || SelectedUnitValue?.Descriptor == owner;
		}

		void ISpellBookUIHandler.HandleForgetSpell(AbilityData data, UnitDescriptor owner)
		{
			m_NeedReset = m_NeedReset || SelectedUnitValue?.Descriptor == owner;
		}

		void IPlayerAbilitiesHandler.HandleAbilityAdded(Ability ability)
		{
			m_NeedReset = true;
			if (ability.Blueprint == UIRoot.Instance.DimensionDoorMassDLCAbility)
			{
				CheckDimensionDoor();	
			}
		}

		void IPlayerAbilitiesHandler.HandleAbilityRemoved(Ability ability)
		{
			m_NeedReset = true;
			if (ability.Blueprint == UIRoot.Instance.DimensionDoorMassDLCAbility)
			{
				CheckDimensionDoor();	
			}
		}

		void ISpellBookRest.OnSpellBookRestHandler(UnitEntityData unit)
		{
			m_NeedReset = m_NeedReset || SelectedUnitValue == unit;
		}

		void ISpellBookCustomSpell.AddSpellHandler(AbilityData ability)
		{
			m_NeedReset = m_NeedReset || SelectedUnitValue?.Descriptor == ability.Caster;
		}

		void ISpellBookCustomSpell.RemoveSpellHandler(AbilityData ability)
		{
			m_NeedReset = m_NeedReset || SelectedUnitValue?.Descriptor == ability.Caster;
		}

		void ITurnBasedModeHandler.HandleSurpriseRoundStarted()
		{
			OnTBMStateUpdated.Execute();
		}

		void ITurnBasedModeHandler.HandleRoundStarted(int round)
		{
			OnTBMStateUpdated.Execute();
		}

		void ITurnBasedModeHandler.HandleTurnStarted(UnitEntityData unit)
		{
			CheckPredictionPanel(unit);
		}

		void ITurnBasedModeHandler.HandleUnitControlChanged(UnitEntityData unit)
		{
			CheckPredictionPanel(unit);
		}

		public void HandleUnitCommandDidAct(UnitCommand command)
		{
			if (Game.Instance.Player.IsTurnBasedModeOn() && command.Executor.IsCurrentUnit())
				CheckPredictionPanel(command.Executor);
		}

		public void HandleUnitCommandDidEnd(UnitCommand command)
		{
			if (Game.Instance.Player.IsTurnBasedModeOn() && command.Executor.IsCurrentUnit())
				CheckPredictionPanel(command.Executor);
		}

		void ITurnBasedModeHandler.HandleUnitNotSurprised(UnitEntityData unit, RuleSkillCheck perceptionCheck)
		{
		}

		void IPartyCombatHandler.HandlePartyCombatStateChanged(bool inCombat)
		{
			if (!inCombat || !Game.Instance.Player.IsTurnBasedModeOn())
			{
				DisposePredictionPanel();
				OnTBMStateUpdated.Execute();
			}
		}

		void ILevelUpCompleteUIHandler.HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
		{
			m_NeedReset = m_NeedReset || SelectedUnitValue?.Descriptor == unit;
		}

		void IFactCollectionUpdatedHandler.HandleFactCollectionUpdated(EntityFactsProcessor collection)
		{
			var unit = collection.Manager.Owner;
			if (!m_NeedReset && ReferenceEquals(SelectedUnitValue, unit) &&
			    (collection is AbilityCollection || collection is ActivatableAbilityCollection))
			{
				m_NeedReset = true;
			}
		}

		void ISelectionHandler.OnUnitSelectionAdd(UnitEntityData selected)
		{
			m_NeedUpdateSelection = true;
		}

		void ISelectionHandler.OnUnitSelectionRemove(UnitEntityData selected)
		{
			m_NeedUpdateSelection = true;
		}

		private void UpdateSelection()
		{
			using (ProfileScope.New("ActionBarVM UpdateSelection"))
			{
				SelectedUnit.Value = !Game.Instance.UI.SelectionManager.IsSingleSelected
					? null
					: UIUtility.GetCurrentCharacter();
			}
		}

		public void ClearSlot(ActionBarSlotVM viewModel)
		{
			if (viewModel.Index == -1)
			{
				return;
			}

			if (SelectedUnitValue == null)
			{
				return;
			}

			if (!viewModel.CanUseOnAllCharacters && viewModel.MechanicActionBarSlot.Unit != SelectedUnitValue)
			{
				return;
			}
			
			var slotEmpty = new MechanicActionBarSlotEmpty();
			SelectedUnitValue.UISettings.SetSlot(slotEmpty, viewModel.Index);
			viewModel.SetMechanicSlot(slotEmpty);
		}

		public void MoveSlot(ActionBarSlotVM sourceSlotVM, ActionBarSlotVM targetSlotVM)
		{
			if (targetSlotVM.Index == -1)
			{
				return;
			}

			SelectedUnitValue.UISettings.SetSlot(sourceSlotVM.MechanicActionBarSlot, targetSlotVM.Index);

			if (sourceSlotVM.Index != -1)
			{
				SelectedUnitValue.UISettings.SetSlot(targetSlotVM.MechanicActionBarSlot, sourceSlotVM.Index);
			}

			SetMechanicSlots(SelectedUnitValue);
		}

	#region Console

		// public void SetPredictionPanelShown(bool state)
		// {
		// 	TBMModeIsActive.Value = state;
		// }

		private void SetActive(bool value)
		{
			HideSelectorVM();
			
			if (value)
			{
				Game.Instance.TimeController.UiTimeScale = 
					!CombatController.IsInTurnBasedCombat() ? 0.5f : 1f;
			}
			else
			{
				Game.Instance.TimeController.UiTimeScale = 1f;
			}
					
			if (value)
			{
				m_ShowTooltip.Value = m_TempShowTooltip;
				OnCurrentSlotChange();
			}
			else
			{
				m_TempShowTooltip = m_ShowTooltip.Value;
				m_ShowTooltip.Value = false;
			}
		}
		
		private void HideSelectorVM()
		{
			m_SelectorVM.Value?.Dispose();
			m_SelectorVM.Value = null;
		}

		private void OnCurrentSlotChange()
		{
			if (CurrentSlotValue == null || SelectedUnitValue == null)
			{
				TooltipEnabled.Value = false;
				IsVisibleContextMenuHint.Value = false;
				return;
			}

			SelectedUnitValue.UISettings.ConsoleActionBarSlotIndex = CurrentSlotValue.Index;
			TooltipEnabled.Value = !CurrentSlotValue.IsEmpty.Value;
			IsVisibleContextMenuHint.Value = CurrentSlotValue != null;

			DoMove();
		}

		private void DoMove()
		{
			if (MoveSlotProperty.Value == null)
				return;

			var newIndex = CurrentSlotValue.Index;
			
			SelectedUnitValue.UISettings.SetSlot(MoveSlotProperty.Value.MechanicActionBarSlot, newIndex);
			SelectedUnitValue.UISettings.SetSlot(CurrentSlotValue.MechanicActionBarSlot, MoveSlotProperty.Value.Index);

			// ReInitPages();
			//
			// int indexPageSlot = 0;
			// foreach (var indexedSlot in m_Pages[CurrentPageIndex.Value - 1])
			// {
			// 	CurrentPage[indexPageSlot++].SetMechanicSlot(indexedSlot.Slot);
			// }
			
			SetMoveSlot(CurrentSlotValue);
		}

		private void OnCurrentPageChange()
		{
			if (SelectedUnitValue == null)
				return;
			
			SelectedUnitValue.UISettings.ConsoleActionBarPageIndex = m_CurrentPageIndex.Value;

			// int index = 0;
			// foreach (var indexedSlot in m_Pages[CurrentPageIndex.Value - 1])
			// {
			// 	CurrentPage[index++].SetMechanicSlot(indexedSlot.Slot);
			// }
			
			DoMove();
		}

		private void ShowSpellMenu()
		{
			CreateSelector(SelectorType.Spell);
		}

		private void ShowAbilityMenu()
		{
			CreateSelector(SelectorType.Ability);
		}

		private void ShowItemMenu()
		{
			CreateSelector(SelectorType.Item);
		}

		private void CreateSelector(SelectorType type)
		{
			m_TempShowTooltip = m_ShowTooltip.Value;
			m_ShowTooltip.Value = false;
			m_SelectorVM.Value = new ActionBarSelectorVM(type, this);
		}

		private void SetMoveSlot(ActionBarSlotVM slot)
		{
			if (slot == null)
			{
				MoveSlotProperty.Value = null;
				return;
			}

			MoveSlotProperty.Value = slot;
		}

		private void SetNewSlot(ActionBarSlotVM slot, MechanicActionBarSlot mabs)
		{
			SelectedUnitValue.UISettings.SetSlot(mabs, slot.Index);
			OnUnitChanged(mabs.Unit);
		}

		private void OnAutoUseToggle()
		{
			CurrentSlotValue?.MechanicActionBarSlot.OnAutoUseToggle();
			OnDeclineClick();
		}

		public void OnDeclineClick()
		{
			if (MoveSlotProperty.Value != null)
			{
				SetMoveSlot(null);
				return;
			}
			
			IsActive.Value = false;
		}

		public void OnFunc02Click()
		{
			SetMoveSlot(null);
			IsActive.Value = false;
		}

		public void OnFunc01Click(IContextMenuOwner owner)
		{
			if (CurrentSlotValue == null)
				return;

			SetMoveSlot(null);
			
			var contextMenuCollectionEntities = new List<ContextMenuCollectionEntity>();
			if (CurrentSlotValue.MechanicActionBarSlot.IsAutoUse)
			{
				contextMenuCollectionEntities.AddRange(new List<ContextMenuCollectionEntity>
				{
					new ContextMenuCollectionEntity(
						UIStrings.Instance.ActionBar.SetAutoUse, 
						() => OnAutoUseToggle()),
					new ContextMenuCollectionEntity(),
				});
			}
			
			contextMenuCollectionEntities.AddRange(new List<ContextMenuCollectionEntity>
			{
				new ContextMenuCollectionEntity(
					UIStrings.Instance.ActionBar.ActionBarSlotTypeSpell, 
					() => ShowSpellMenu(),
					() => IsEnable(SelectorType.Spell)),
				new ContextMenuCollectionEntity(
					UIStrings.Instance.ActionBar.ActionBarSlotTypeActivableAbility, 
					() => ShowAbilityMenu(),
					() => IsEnable(SelectorType.Ability)),
				new ContextMenuCollectionEntity(
					UIStrings.Instance.ActionBar.ActionBarSlotTypeItem, 
					() => ShowItemMenu(),
					() => IsEnable(SelectorType.Item)),
				new ContextMenuCollectionEntity(),
				new ContextMenuCollectionEntity(
					UIStrings.Instance.ActionBar.Move , 
					() => SetMoveSlot(CurrentSlotValue),
					() => !CurrentSlotValue.IsEmpty.Value),
				new ContextMenuCollectionEntity(
					UIStrings.Instance.ActionBar.ActionBarClearSlot , 
					() => ClearSlot(CurrentSlotValue),
					() => !CurrentSlotValue.IsEmpty.Value),
			});
			
			var collection = new ContextMenuCollection(contextMenuCollectionEntities, owner.GetOwnerRectTransform());
			EventBus.RaiseEvent<IContextMenuHandler>(h => h.HandleContextMenuRequest(collection));
		}

		public void OnShowConvertRequest()
		{
			SetMoveSlot(null);
			CurrentSlotValue?.OnShowConvertRequest();
		}
		
		private bool IsEnable(SelectorType type)
		{
			switch (type)
			{
				case SelectorType.Spell:
					var haveSpellInSpellBook = SelectedUnitValue.Descriptor.Spellbooks.Any(
						s =>
						{
							for (int i = 0; i <= s.MaxSpellLevel; i++)
							{
								if (s.Blueprint.Spontaneous)
								{
									if (s.GetKnownSpells(i).Any())
									{
										return true;
									}
								}
								else
								{
									var spells = s.GetMemorizedSpells(i);
									IEnumerable<SpellSlot> memorizedSpells = spells as IList<SpellSlot> ?? spells.ToList();
									if (!memorizedSpells.Empty())
									{
										return true;
									}
								}
							}

							return false;
						});

					if (haveSpellInSpellBook)
					{
						return true;
					}

					foreach (var ability in SelectedUnitValue.Abilities)
					{
						if (ability.Hidden || !ability.Blueprint.IsCantrip)
						{
							continue;
						}

						return true;
					}
					
					return false;
				case SelectorType.Item:
					return SelectedUnitValue.Body.QuickSlots.Any(s => s.HasItem);
				case SelectorType.Ability:
					return SelectedUnitValue.ActivatableAbilities.Enumerable.Any() || SelectedUnitValue.Abilities.Enumerable.Any(s => s.Data.IsAvailable && !s.Hidden);
			}

			return false;
		}

		public void OnRightStickButton()
		{
			m_ShowTooltip.Value = !m_ShowTooltip.Value;
		}

		public void OnRightStickY(float x)
		{
			if (Mathf.Abs(x) < 0.1f)
				return;

			//Game.Instance.UI.DescriptionController?.DescWindow?.OnScroll(x);
		}

		public void OnNextPage()
		{
			if (m_CurrentPageIndex.Value == MaxPage)
			{
				m_CurrentPageIndex.Value = 1;
				return;
			}

			m_CurrentPageIndex.Value++;
		}

		public void OnPrevPage()
		{
			if (m_CurrentPageIndex.Value == 1)
			{
				m_CurrentPageIndex.Value = MaxPage;
				return;
			}
			
			m_CurrentPageIndex.Value--;
		}
		
		// void IViewModelVisibleHandler.OnShow()
		// {
		// 	m_IsShown.Value = true;
		// }
		//
		// void IViewModelVisibleHandler.OnHide()
		// {
		// 	m_IsShown.Value = false;
		// }

		public void Result(MechanicActionBarSlot mechanicActionBarSlot, bool execute = false)
		{
			if (mechanicActionBarSlot != null && !execute)
			{
				SetNewSlot(CurrentSlotValue, mechanicActionBarSlot);	
			}
			
			HideSelectorVM();
			m_ShowTooltip.Value = m_TempShowTooltip;

			if (execute && mechanicActionBarSlot != null)
			{
				mechanicActionBarSlot.OnClick();
				IsActive.Value = false;	
			}
		}

		// public void SlotWasAdded(UnitEntityData unit)
		// {
		// 	OnUnitChangedHandler();
		// }

		public void OnBind()
		{
			if (!IsActive.Value)
            {
            	m_ShowTooltip.Value = false;
            	
            	// CurrentSlotValue?.SetTooltip(false);
            	CurrentSlotValue?.OnHover(false);
            }
		}

		public bool MoveConfirm()
		{
			if (MoveSlotProperty.Value == null)
			{
				return false;
			}

			MoveSlotProperty.Value = null;
			return true;
		}
		
		public void SetCurrentSlot(ActionBarSlotVM slot)
		{
			CurrentSlot.Value = slot;
		}

	#endregion

		public void SlotWasAdded(UnitEntityData unit)
		{
			OnUnitChanged(unit);
		}
		
		public void HandleTurnBasedModeStateChanged(bool enabled)
		{
			// if (Game.Instance.Player.IsTurnBasedStateMandatory)
			// 	return;

			// if (Game.Instance.Player.IsInCombat)
			// {
				if (!enabled)
					DisposePredictionPanel();
			// }
		}

		public void UpdateMandatoryTurnBasedModeState()
		{
			
		}

		public void HandleHoverChange(UnitEntityView unitEntityView, bool isHover)
		{
			HighlightedUnit.Value = isHover ? unitEntityView : null;
		}
	}
}