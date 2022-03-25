using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.TurnBasedMode;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.MVVM;
using TurnBased.Controllers;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ActionBarWeaponVM : BaseDisposable, 
			IViewModel, 
			IUnitActiveEquipmentSetHandler, 
			IUnitEquipmentHandler
	{
		public readonly IReadOnlyReactiveProperty<UnitEntityData> SelectedUnit;

		public readonly ReactiveProperty<Sprite> PrimaryItem = new ReactiveProperty<Sprite>(null);
		public readonly ReactiveProperty<Sprite> SecondaryItem = new ReactiveProperty<Sprite>(null);

		public readonly List<ActionBarWeaponSetVM> Sets = new List<ActionBarWeaponSetVM>();
		
		public readonly BoolReactiveProperty CanChange = new BoolReactiveProperty();

		public ActionBarWeaponVM(IReadOnlyReactiveProperty<UnitEntityData> selectedUnit)
		{
			SelectedUnit = selectedUnit;

			for (int i = 0; i < GameConsts.UnitHandsEquipmentSetsCount; i++)
			{
				Sets.Add(new ActionBarWeaponSetVM(i, selectedUnit));
			}
			
			AddDisposable(SelectedUnit.Subscribe(OnUnitChanged));
			AddDisposable(EventBus.Subscribe(this));
			AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(_ => UpdateCanChange()));
		}

		protected override void DisposeImplementation()
		{
			Sets.ForEach(s => s.Dispose());
			Sets.Clear();
		}

		private void OnUnitChanged(UnitEntityData unit)
		{
			if (unit == null)
			{
				return;
			}

			PrimaryItem.Value = unit.Body.PrimaryHand?.MaybeItem?.Blueprint.Icon;
			SecondaryItem.Value = unit.Body.SecondaryHand?.MaybeItem?.Blueprint.Icon;
		}

		void IUnitActiveEquipmentSetHandler.HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
		{
			if (SelectedUnit.Value != unit)
			{
				return;
			}

			OnUnitChanged(unit);
		}

		void IUnitEquipmentHandler.HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
		{
			if (slot.Owner != SelectedUnit.Value)
			{
				return;
			}

			OnUnitChanged(SelectedUnit.Value);
		}
		
		protected TurnController TurnController => Game.Instance.TurnBasedCombatController?.CurrentTurn;
		protected ActionsState TurnState => TurnController?.GetActionsStates(TurnController.SelectedUnit);
		
		private void UpdateCanChange()
		{
			CanChange.Value = !CombatController.IsInTurnBasedCombat() || (SelectedUnit.Value != null && TurnState != null &&
			                                                              TurnController.SelectedUnit == SelectedUnit.Value &&
			                                                              !SelectedUnit.Value.Descriptor.State.Prone.Active &&
			                                                              TurnController.CanEndTurnAndNoActing() && 
			                                                              (SelectedUnit.Value.HasMoveAction() || SelectedUnit.Value.HasStandardAction()));
		}
	}
}