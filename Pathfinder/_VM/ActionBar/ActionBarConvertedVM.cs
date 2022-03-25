using System;
using System.Collections.Generic;
using Kingmaker.UI.UnitSettings;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ActionBarConvertedVM : BaseDisposable, IViewModel
	{
		public readonly List<ActionBarSlotVM> Slots = new List<ActionBarSlotVM>();
		private readonly Action m_OnClose;

		public ActionBarConvertedVM(List<MechanicActionBarSlotSpontaneusConvertedSpell> list, Action onClose)
		{
			m_OnClose = onClose;
			list.ForEach(s => Slots.Add(new ActionBarSlotVM(s)));
			
			AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(_ => UpdateSlotResources()));
		}

		private void UpdateSlotResources()
		{
			foreach (var actionBarSlotVM in Slots)
			{
				actionBarSlotVM.UpdateResource();
			}
		}

		protected override void DisposeImplementation()
		{
			Slots.ForEach(s => s.Dispose());
			Slots.Clear();
		}

		public void Close()
		{
			m_OnClose?.Invoke();
		}
	}
}