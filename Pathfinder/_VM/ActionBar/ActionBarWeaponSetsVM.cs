using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Items;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ActionBarWeaponSetsVM : BaseDisposable, IViewModel
	{
		private readonly Action m_Close;
		
		public List<ActionBarWeaponSetEntityVM> EntityVms = new List<ActionBarWeaponSetEntityVM>();
		// public readonly BoolReactiveProperty TBMModeIsActive;
		public ActionBarWeaponSetsVM(Action close, UnitBody unitBody, Action<UnitBody> setUnitBody/*, BoolReactiveProperty tbmModeIsActive*/)
		{
			m_Close = close;
			// TBMModeIsActive = tbmModeIsActive;
			foreach (var equipmentSet in unitBody.HandsEquipmentSets)
			{
				EntityVms.Add(new ActionBarWeaponSetEntityVM(equipmentSet, unitBody, setUnitBody));
			}
		}

		public void Close()
		{
			m_Close?.Invoke();
		}

		protected override void DisposeImplementation()
		{
			EntityVms.ForEach(vm => vm.Dispose());
			EntityVms.Clear();
		}

		public void Confirm()
		{
			var selectedSet = EntityVms.FirstOrDefault(entity => entity.m_Select.Value);
			selectedSet?.ConfirmChangeSet();

			Close();
		}
	}
}