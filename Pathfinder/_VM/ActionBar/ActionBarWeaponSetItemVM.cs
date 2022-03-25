using System;
using Kingmaker.UI.MVVM._VM.Slots;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ActionBarWeaponSetItemVM : ItemSlotVM
	{
		private readonly Action m_OnSelectAction;

		public ActionBarWeaponSetItemVM(Action onSelect) : base()
		{
			m_OnSelectAction = onSelect;
		}

		public void OnSetCurrent()
		{
			m_OnSelectAction?.Invoke();
		}
	}
}