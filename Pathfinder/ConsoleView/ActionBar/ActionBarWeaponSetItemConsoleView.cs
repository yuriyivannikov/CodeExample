using Kingmaker.UI.MVVM._ConsoleView.Slots;
using Kingmaker.UI.MVVM._PCView.ActionBar;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.ActionBar
{
	public class ActionBarWeaponSetItemConsoleView : ActionBarWeaponSetItemBaseView
	{
		[SerializeField]
		private ItemSlotConsoleView m_ItemSlotConsoleView;

		protected override void BindViewImplementation()
		{
			base.BindViewImplementation();
			
			m_ItemSlotConsoleView.Bind(ViewModel);
			AddDisposable(m_ItemSlotConsoleView.OnConfirmClickAsObservable.Subscribe(_ => OnClick()));
		}
	}
}