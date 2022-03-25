using System;
using Kingmaker.Items;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.MVVM;
using TurnBased.Controllers;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ActionBarWeaponSetEntityVM : BaseDisposable, IViewModel
	{
		public readonly Sprite PrimaryHand;
		public readonly Sprite SecondaryHand;
		public readonly int WeaponSetIndex;

		public bool IsCurrent => m_UnitBody.CurrentHandEquipmentSetIndex == WeaponSetIndex;
		
		private readonly UnitBody m_UnitBody;
		private Action<UnitBody> m_SetUnitBody;

		public BoolReactiveProperty m_Select = new BoolReactiveProperty();
		private TurnController TurnController => Game.Instance.TurnBasedCombatController.CurrentTurn;

		public ActionBarWeaponSetEntityVM(HandsEquipmentSet equipmentSet, UnitBody unitBody, Action<UnitBody> setUnitBody)
		{
			m_UnitBody = unitBody;
			m_SetUnitBody = setUnitBody;

			WeaponSetIndex = unitBody.HandsEquipmentSets.FindIndex(s => s == equipmentSet);
			PrimaryHand = equipmentSet.PrimaryHand.MaybeItem?.Icon;
			SecondaryHand = equipmentSet.SecondaryHand.MaybeItem?.Icon;
			AddDisposable(m_Select.Subscribe(_=>TryUpdatePrediction()));
		}

		public void SetSelected(bool value)
		{
			m_Select.Value  = value;
		}

		public void ConfirmChangeSet()
		{
			if (!m_Select.Value)
				return;
			
			m_UnitBody.CurrentHandEquipmentSetIndex = WeaponSetIndex;
			m_SetUnitBody?.Invoke(m_UnitBody);
		}
		
		private void TryUpdatePrediction()
		{
			if (!m_Select.Value)
				return;
			
			TurnController?.CalculatePredictionForWeaponSetChange(m_UnitBody.CurrentHandEquipmentSetIndex != WeaponSetIndex);
		}

		protected override void DisposeImplementation()
		{
			if (m_Select.Value)
				return;
			TurnController?.CalculatePredictionForWeaponSetChange(false);
		}
	}
}