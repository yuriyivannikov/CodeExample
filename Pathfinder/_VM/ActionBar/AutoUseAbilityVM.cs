using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.UnitSettings;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class AutoUseAbilityVM : BaseDisposable, IViewModel, IUnitAutoUseAbilityHandler
	{
		private readonly IReadOnlyReactiveProperty<UnitEntityData> m_SelectedUnit;

		public readonly ReactiveProperty<bool> HasAbility = new ReactiveProperty<bool>(false);
		public readonly ActionBarSlotVM SlotVM;
		
		public AutoUseAbilityVM(IReadOnlyReactiveProperty<UnitEntityData> selectedUnit)
		{
			m_SelectedUnit = selectedUnit;
			
			AddDisposable(SlotVM = new ActionBarSlotVM());
			
			AddDisposable(m_SelectedUnit.Subscribe(OnUnitChanged));
			AddDisposable(EventBus.Subscribe(this));
		}

		protected override void DisposeImplementation()
		{
		}

		private void OnUnitChanged(UnitEntityData unit)
		{
			var ability = unit?.Brain.AutoUseAbility;

			HasAbility.Value = ability != null;

			if (ability != null)
			{
				if (ability.Fact != null)
				{
					if (ability.SourceItem is ItemEntityUsable usableItem && 
					    ability.SourceItem.Ability == ability.Fact)
					{
						SlotVM.SetMechanicSlot(new MechanicActionBarSlotItem { Item = usableItem, Unit = unit });
					}
					else
					{
						SlotVM.SetMechanicSlot(new MechanicActionBarSlotAbility { Ability = ability, Unit = unit });
					}
				}
				else if (ability.Spellbook == null)
				{
					SlotVM.SetMechanicSlot(new MechanicActionBarSlotEmpty());
				}
				else if (ability.IsSpontaneous || ability.SpellLevel < 1)
				{
					SlotVM.SetMechanicSlot(new MechanicActionBarSlotSpontaneousSpell(ability) { Unit = unit });
				}
				else if (ability.SpellSlot != null)
				{
					SlotVM.SetMechanicSlot(new MechanicActionBarSlotMemorizedSpell(ability.SpellSlot) { Unit = unit });	
				}
				else
				{
					SlotVM.SetMechanicSlot(new MechanicActionBarSlotEmpty());
				}
			}
			else
			{
				HasAbility.Value = false;
				SlotVM.SetMechanicSlot(new MechanicActionBarSlotEmpty());
			}
		}

		public void HandleUnitChangeAutoUseAbility(UnitEntityData unit)
		{
			if (unit != m_SelectedUnit.Value)
			{
				return;
			}
			
			OnUnitChanged(unit);
		}
	}
}