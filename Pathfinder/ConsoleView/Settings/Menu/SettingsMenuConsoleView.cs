using System.Collections.Generic;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.FullScreenUITypes;
using Kingmaker.UI.MVVM._VM.Settings;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Menu
{
	public class SettingsMenuConsoleView : ViewBase<SettingsMenuConsoleVM>
	{
		[SerializeField]
		private WidgetListMVVM m_WidgetList;

		[SerializeField]
		private SettingsMenuEntityConsoleView m_Prefab;

		[Header("Animator")]
		[SerializeField]
		private FadeAnimator m_Animator;

		private GridConsoleNavigationBehaviour m_NavigationBehavior;
		
		private List<SettingsMenuEntityConsoleView> m_Entities;

		private bool m_IsShowed;

		public void Initialize()
		{
			gameObject.SetActive(false);
			m_Animator.Initialize();
		}

		protected override void BindViewImplementation()
		{
			DrawEntities();
			
			m_NavigationBehavior = new GridConsoleNavigationBehaviour();

			m_Entities = new List<SettingsMenuEntityConsoleView>();
			SettingsMenuEntityConsoleView selectedItem = null;
			foreach (var widgetView in m_WidgetList.Entries)
			{
				var item = (SettingsMenuEntityConsoleView)widgetView;
				m_Entities.Add(item);
				if (item.SettingsScreenType == Game.Instance.Player.UISettings.LastSettingsMenuType)
					selectedItem = item;
			}

			m_NavigationBehavior.SetEntitiesVertical(m_Entities);
			AddDisposable(GamePad.Instance.PushLayer(GetInputLayer()));
			m_NavigationBehavior.FocusOnEntityManual(selectedItem);

			Show();
		}
		
		private void DrawEntities()
		{
			m_WidgetList.DrawEntries(ViewModel.MenuEntitiesVMList, m_Prefab);
		}

		private InputLayer GetInputLayer()
		{
			var inputLayer = m_NavigationBehavior.GetInputLayer(new InputLayer { ContextName = "SettingsMenuConsoleViewInput"});
			inputLayer.AddButton(_ => ViewModel.Close(), RewiredConsts.Action.Decline);
			
			return inputLayer;
		}

		protected override void DestroyViewImplementation()
		{
			Hide();
			m_Entities.Clear();
			m_Entities = null;
		}

		private void Show()
		{
			if (m_IsShowed)
				return;
			
			m_IsShowed = true;
			
			m_Animator.AppearAnimation();
			EventBus.RaiseEvent<IFullScreenUIHandler>(h => h.HandleFullScreenUiChanged(true, FullScreenUIType.Settings));
			UISoundController.Instance.Play(UISoundType.SettingsOpen);
		}

		public void Hide()
		{
			if (!m_IsShowed)
				return;

			m_Animator.DisappearAnimation(() =>
				{
					gameObject.SetActive(false);
					m_IsShowed = false;
				});
			
			EventBus.RaiseEvent<IFullScreenUIHandler>(h => h.HandleFullScreenUiChanged(false, FullScreenUIType.Settings));
			UISoundController.Instance.Play(UISoundType.SettingsClose);
		}
	}
}