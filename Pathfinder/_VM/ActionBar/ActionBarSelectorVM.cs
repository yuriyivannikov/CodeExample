using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI._ConsoleUI.Models;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.InfoWindow;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public enum SelectorType
	{
		None,
		Spell,
		Ability,
		Item
	}

	public interface IActionBarSelectorHolder
	{
		void Result(MechanicActionBarSlot mechanicActionBarSlot, bool execute = false);
	}

	public class ActionBarSelectorVM : VMBase
	{
		private readonly ReactiveCollection<ActionBarSelectorItemVM> m_Items = new ReactiveCollection<ActionBarSelectorItemVM>();
		public IReadOnlyReactiveCollection<ActionBarSelectorItemVM> Items => m_Items;
		
		public IReadOnlyReactiveProperty<string> Title => m_Title;
		private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

		public readonly InfoSectionVM InfoSectionVM = new InfoSectionVM();

		private readonly IActionBarSelectorHolder m_Holder;

		private static UnitEntityData Unit => UnitSelectionManager.Instance.CurrentSelectUnit.Value;
		
		public readonly ReactiveProperty<bool> IsPossibleActive = new ReactiveProperty<bool>();

		public ActionBarSelectorVM(SelectorType type, IActionBarSelectorHolder holder)
		{
			UILog.VMCreated($"ActionBarSelector");

			m_Holder = holder;

			switch (type)
			{
				case SelectorType.None:
					break;
				case SelectorType.Spell:
					CollectSpells();
					break;
				case SelectorType.Ability:
					CollectAbilities();
					break;
				case SelectorType.Item:
					CollectItems();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
			
			AddDisposable(InfoSectionVM = new InfoSectionVM());

			m_Title.Value = ""; //UIStrings.Instance.ActionBar.GetSelectorHeader(type); !!!!!!!!!!!!!!!!!!!!!!!
		}

		private void CollectSpells()
		{
			List<AbilityData> alreadyContains = new List<AbilityData>();

			foreach (var spellBook in Unit.Descriptor.Spellbooks)
			{
				if (spellBook.Blueprint.Spontaneous)
				{
					for (int spellLevel = spellBook.MaxSpellLevel; spellLevel >= 1; spellLevel--)
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
							AddSlotToCollection(new MechanicActionBarSlotSpontaneousSpell(spell) {Unit = Unit});
						}
					}
				}
				else
				{
					for (int spellLevel = spellBook.MaxSpellLevel; spellLevel >= 0; spellLevel--)
					{
						var spells = spellBook.GetMemorizedSpells(spellLevel);
						foreach (var spell in spells)
						{
							if (alreadyContains.Contains(spell.Spell))
							{
								continue;
							}

							alreadyContains.Add(spell.Spell);
							AddSlotToCollection(new MechanicActionBarSlotMemorizedSpell(spell) {Unit = Unit});
						}
						
						var knownSpells = spellBook.GetKnownSpells(spellLevel);
						foreach (var spell in knownSpells)
						{
							if (alreadyContains.Contains(spell))
							{
								continue;
							}

							if (spell.Blueprint.IsCantrip || spell.IsSpontaneous)
							{

								alreadyContains.Add(spell);
								AddSlotToCollection(new MechanicActionBarSlotSpontaneousSpell(spell) {Unit = Unit});
							}
						}
					}
				}

				foreach (var ability in Unit.Abilities)
				{
					if (ability.Hidden || !ability.Blueprint.IsCantrip)
					{
						continue;
					}

					var spell = ability.Data;
					if (alreadyContains.Contains(spell))
					{
						continue;
					}

					alreadyContains.Add(spell);

					AddSlotToCollection(new MechanicActionBarSlotAbility {Ability = spell, Unit = Unit});
				}
			}
		}

		private void CollectItems()
		{
			foreach (var quickSlot in Unit.Body.QuickSlots)
			{
				if (!quickSlot.HasItem)
				{
					continue;
				}

				AddSlotToCollection(new MechanicActionBarSlotItem {Item = quickSlot.Item, Unit = Unit});
			}
		}

		private void CollectAbilities()
		{
			foreach (var ability in Unit.Abilities)
			{
				if (ability.Hidden || ability.Blueprint.IsCantrip)
				{
					continue;
				}

				AddSlotToCollection(new MechanicActionBarSlotAbility {Ability = ability.Data, Unit = Unit});
			}

			foreach (var ability in Unit.ActivatableAbilities)
			{
				AddSlotToCollection(
					new MechanicActionBarSlotActivableAbility {ActivatableAbility = ability, Unit = Unit});
			}
		}

		private void AddSlotToCollection(MechanicActionBarSlot mechanicActionBarSlot)
		{
			m_Items.Add(new ActionBarSelectorItemVM(mechanicActionBarSlot, IsPossibleActive));
		}

		public void OnConfirm()
		{
			var itemVm = m_Items.First(vm => vm.IsSelect.Value);
			m_Holder.Result(itemVm?.MechanicActionBarSlot);
		}

		public void OnDecline()
		{
			m_Holder.Result(null);
		}

		public void OnFunc01()
		{
			var itemVm = m_Items.First(vm => vm.IsSelect.Value);
			m_Holder.Result(itemVm?.MechanicActionBarSlot, true);
		}

		// public void SelectItem(IHasTooltip tooltipData)
		// {
			// InfoSectionVM.IsActive.Value = tooltipData != null;
			// InfoSectionVM.HandleTooltipObject(tooltipData);
		// }

		protected override void DisposeImplementation()
		{
			m_Items.Clear();
		}
	}
}