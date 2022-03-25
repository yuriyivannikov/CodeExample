using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.MVVM;
using TurnBased.Controllers;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ActionBarWeaponSetVM : BaseDisposable, IViewModel, IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler
	{
		public readonly ReactiveProperty<bool> IsCurrentSet = new ReactiveProperty<bool>(false);
		
		public readonly int Index;
		public readonly ActionBarWeaponSetItemVM Primary;
		public readonly ActionBarWeaponSetItemVM Secondary;
		
		private readonly IReadOnlyReactiveProperty<UnitEntityData> m_SelectedUnit;
		private TurnController TurnController => Game.Instance.TurnBasedCombatController.CurrentTurn;
		public bool CheckIndex => m_SelectedUnit.Value.Body.CurrentHandEquipmentSetIndex != Index;

		public ActionBarWeaponSetVM(int index, IReadOnlyReactiveProperty<UnitEntityData> selectedUnit)
		{
			Index = index;
			m_SelectedUnit = selectedUnit;

			AddDisposable(Primary = new ActionBarWeaponSetItemVM(OnSetCurrentSet));
			AddDisposable(Secondary = new ActionBarWeaponSetItemVM(OnSetCurrentSet));
			
			AddDisposable(m_SelectedUnit.Subscribe(OnUnitChanged));
			AddDisposable(EventBus.Subscribe(this));
		}

		protected override void DisposeImplementation()
		{
			if (!IsCurrentSet.Value)
				return;
			TurnController?.CalculatePredictionForWeaponSetChange(false);
		}

		private void OnUnitChanged(UnitEntityData unit)
		{
			if (unit == null)
			{
				return;
			}
			
			IsCurrentSet.Value = unit.Body.CurrentHandEquipmentSetIndex == Index;
			
			Primary.SetItem(unit.Body.HandsEquipmentSets[Index]?.PrimaryHand?.MaybeItem);
			Secondary.SetItem(unit.Body.HandsEquipmentSets[Index]?.SecondaryHand?.MaybeItem);
			AddDisposable(IsCurrentSet.Skip(1).Subscribe(_=>TryUpdatePrediction()));
		}

		void IUnitActiveEquipmentSetHandler.HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
		{
			if (m_SelectedUnit.Value == null)
			{
				return;
			}
			
			if (unit != m_SelectedUnit.Value)
			{
				return;
			}

			OnUnitChanged(m_SelectedUnit.Value);
		}

		void IUnitEquipmentHandler.HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
		{
			if (m_SelectedUnit.Value == null)
			{
				return;
			}
			
			if (slot.Owner != m_SelectedUnit.Value)
			{
				return;
			}
			
			OnUnitChanged(m_SelectedUnit.Value);
		}

		public void OnSetCurrentSet()
		{
			var unit = m_SelectedUnit.Value;
			if (unit == null)
			{
				return;
			}

			unit.Body.CurrentHandEquipmentSetIndex = Index;
		}
		
		private void TryUpdatePrediction()
		{
			if (Game.Instance.CurrentMode == GameModeType.FullScreenUi)
				return;
			
			if (!IsCurrentSet.Value)
				return;
			
			TurnController?.CalculatePredictionForWeaponSetChange(CheckIndex);
		}
	}
}