using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.Settings.Menu;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Menu
{
	public class SettingsMenuEntityConsoleView : ViewBase<SettingsMenuEntityVM>, IConsoleNavigationEntity, IWidgetView
	{
		[SerializeField]
		private OwlcatMultiButton m_MultiButton;
		
		[SerializeField]
		private TextMeshProUGUI m_Title;
		
		public UISettingsManager.SettingsScreen SettingsScreenType => ViewModel.SettingsScreenType;
		protected override void BindViewImplementation()
		{
			m_Title.text = ViewModel.IsSaber ? UIUtility.GetSaberBookFormat(ViewModel.Title) : ViewModel.Title;
			AddDisposable(ViewModel.IsSelected.Subscribe(SetFocus));
		}

		protected override void DestroyViewImplementation()
		{
		}

		public void SetFocus(bool value)
		{
			m_MultiButton.SetFocus(value);
			m_MultiButton.SetActiveLayer(value ? "Selected" : "Normal");
		}

		public bool IsValid()
		{
			return true;
		}

		public void BindWidgetVM(IViewModel vm)
		{
			Bind((SettingsMenuEntityVM)vm);
		}

		public bool CheckType(IViewModel viewModel)
		{
			return viewModel is SettingsMenuEntityVM;
		}
		
		
		public bool CanConfirmClick()
		{
			return true;
		}

		public string GetConfirmClickHint()
		{
			return UIStrings.Instance.SettingsUI.MenuConfirm;
		}

		public void OnConfirmClick()
		{
			ViewModel.Confirm();
		}

		public MonoBehaviour MonoBehaviour => this;
	}
}