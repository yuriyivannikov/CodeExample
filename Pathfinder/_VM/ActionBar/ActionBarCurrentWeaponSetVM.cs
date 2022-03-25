using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Owlcat.Runtime.UI.MVVM;
using TurnBased.Controllers;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ActionBarCurrentWeaponSetVM : BaseDisposable, IViewModel
	{
		// private static IReadOnlyReactiveProperty<UnitEntityData> Unit => UnitSelectionManager.Instance.CurrentSelectUnit;
		
		public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>(null);
		public readonly ReactiveProperty<bool> IsPet = new ReactiveProperty<bool>(false);
		public readonly ReactiveProperty<ActionBarWeaponSetsVM> WeaponSets = new ReactiveProperty<ActionBarWeaponSetsVM>(null);
		
		// private readonly Action<ActionBarSlotVM> m_SetSelected;
		// public readonly BoolReactiveProperty TBMModeIsActive;
		public BoolReactiveProperty CanChangeWeaponSet => CombatController.CanChangeWeaponSet;
		public bool Enabled { get; set; }

		private readonly ReactiveProperty<UnitEntityData> m_SelectedUnit;
		
		public ActionBarCurrentWeaponSetVM(ReactiveProperty<UnitEntityData> selectedUnit/*, BoolReactiveProperty tbmModeIsActive*/)
		{
			m_SelectedUnit = selectedUnit;
			// TBMModeIsActive = tbmModeIsActive;
			Enabled = true;
			
			AddDisposable(m_SelectedUnit.Subscribe(unit =>
				{
					if (unit == null)
						return;
						
					// IsPet.Value = u.Descriptor.IsPet;
					SetupInfo(unit.Body);
				}));
		}
		
		private void SetupInfo(UnitBody playerBody)
		{
			var handsEquipmentSets = playerBody.HandsEquipmentSets.ToArray();
			var currentSet = handsEquipmentSets[playerBody.CurrentHandEquipmentSetIndex];

			var primaryHand = currentSet.PrimaryHand.MaybeItem;
			if (primaryHand?.Icon != null)
			{
				Icon.Value = primaryHand.Icon;
			}
			else
			{
				var secondaryHand = currentSet.SecondaryHand.MaybeItem;
				Icon.Value = secondaryHand?.Icon != null ? secondaryHand.Icon : null;
			}
		}

		public bool TryOpenWeaponSet()
		{
			if (CanChangeWeaponSet.Value)
			{
				WeaponSets.Value = new ActionBarWeaponSetsVM(CloseWeaponSet, m_SelectedUnit.Value.Body, SetupInfo/*, TBMModeIsActive*/);
				return true;
			}
			else
			{
				return false;
			}
		}

		private void CloseWeaponSet()
		{
			WeaponSets.Value?.Dispose();
			WeaponSets.Value = null;
		}

		// public void SetSelected()
		// {
		// 	m_SetSelected?.Invoke(null);
		// }
		
		protected override void DisposeImplementation()
		{
		}
	}
}