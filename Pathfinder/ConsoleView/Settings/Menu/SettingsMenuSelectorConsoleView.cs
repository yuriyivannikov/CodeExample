using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM._VM.Settings.Menu;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Menu
{
	public class SettingsMenuSelectorConsoleView : ViewBase<SelectionGroupRadioVM<SettingsMenuEntityVM>>
	{
		[SerializeField]
		public List<SettingsMenuEntityConsoleView> MenuEntities;

		public void Initialize()
		{
			if (MenuEntities.Empty())
			{
				MenuEntities = GetComponentsInChildren<SettingsMenuEntityConsoleView>().ToList();
			}
		}
		
		protected override void BindViewImplementation()
		{
			for (int i = 0; i < MenuEntities.Count; i++)
			{
				if (i >= ViewModel.EntitiesCollection.Count)
				{
					MenuEntities[i].gameObject.SetActive(false);
					continue;
				}
				
				MenuEntities[i].Bind(ViewModel.EntitiesCollection[i]);
			}
		}

		public void OnNext()
		{
			ViewModel.SelectNextValidEntity();
		}
		
		public void OnPrev()
		{
			ViewModel.SelectPrevValidEntity();
		}

		protected override void DestroyViewImplementation()
		{
		}
	}
}