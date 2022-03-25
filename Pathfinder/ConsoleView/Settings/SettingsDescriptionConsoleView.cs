using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM._VM.Settings;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings
{
	public class SettingsDescriptionConsoleView : ViewBase<SettingsDescriptionVM>
	{
		[SerializeField]
		private TextMeshProUGUI m_TitleText;
		
		[SerializeField]
		private TextMeshProUGUI m_DescriptionText;

		[SerializeField] 
		private FadeAnimator m_ContentAnimator;
		
		private bool m_IsShowed;
		private bool m_ContentRefreshing;

		public void Initialize()
		{
			m_ContentAnimator.gameObject.SetActive(false);
			m_ContentAnimator.Initialize();
		}

		protected override void BindViewImplementation()
		{
			if (m_IsShowed)
				ContentUpdate();
			else
				Show();
		}

		private void ContentUpdate()
		{
			if (m_ContentRefreshing)
				return;

			m_ContentRefreshing = true;

			m_ContentAnimator.DisappearAnimation(() =>
			{
				m_ContentAnimator.AppearAnimation();
				UpdateView();
				m_ContentRefreshing = false;
			});
		}

		private void Show()
		{
			if (m_ContentRefreshing)
				return;

			m_ContentRefreshing = true;
            
			m_ContentAnimator.AppearAnimation();
            
			UpdateView();
			m_IsShowed = true;
			m_ContentRefreshing = false;
		}

		private void Hide()
		{
			m_ContentAnimator.DisappearAnimation();
			m_IsShowed = false;
		}

		private void UpdateView()
		{
			m_TitleText.text = UIUtility.GetSaberBookFormat(ViewModel.Title);
			m_DescriptionText.text = ViewModel.Description;
		}

		protected override void DestroyViewImplementation()
		{
			Hide();
		}
	}
}