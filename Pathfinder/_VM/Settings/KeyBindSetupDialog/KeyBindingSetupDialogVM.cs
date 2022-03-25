using System;
using Kingmaker.Settings;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.Settings.KeyBindSetupDialog
{
	public class KeyBindingSetupDialogVM : BaseDisposable, IViewModel
	{
		private readonly UISettingsEntityKeyBinding m_UISettingsEntity;
		private readonly int m_BindingIndex;
		private readonly Action m_CloseAction;
		
		private KeyBindingData m_CurrentKeyBinding;
		public KeyBindingData CurrentKeyBinding
			=> m_CurrentKeyBinding;

		private bool m_CurrentBindingIsOccupied;
		public bool CurrentBindingIsOccupied
			=> m_CurrentBindingIsOccupied;
		
		public KeyBindingSetupDialogVM(UISettingsEntityKeyBinding uiSettingsEntity, int bindingIndex, Action closeAction)
		{
			m_UISettingsEntity = uiSettingsEntity;
			m_BindingIndex = bindingIndex;

			m_CloseAction = closeAction;
			
			m_CurrentKeyBinding = uiSettingsEntity.GetBinding(bindingIndex);
		}

		public void OnBindingChosen(KeyBindingData keyBindingData)
		{
			m_CurrentKeyBinding = keyBindingData;
			m_CurrentBindingIsOccupied = !m_UISettingsEntity.TrySetBinding(keyBindingData, m_BindingIndex);
			if (!m_CurrentBindingIsOccupied)
			{
				Close();
			}
		}
		
		public void Unbind()
		{
			m_UISettingsEntity.TrySetBinding(new KeyBindingData {Key = KeyCode.None}, m_BindingIndex);
			Close();
		}

		public void Close()
		{
			m_CloseAction?.Invoke();
		}

		protected override void DisposeImplementation()
		{
		}
	}
}