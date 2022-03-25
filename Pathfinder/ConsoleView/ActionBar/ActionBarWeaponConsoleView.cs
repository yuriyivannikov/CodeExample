using DG.Tweening;
using Kingmaker.UI.MVVM._PCView.ActionBar;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using RewiredConsts;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.ActionBar
{
	public class ActionBarWeaponConsoleView : ActionBarWeaponBaseView, IConsoleNavigationEntity, IConfirmClickHandler
	{
		[SerializeField]
		private OwlcatMultiButton m_FocusMultiButton;

		[SerializeField]
		private CanvasGroup m_WeaponSetCanvasGroup;

		private CompositeDisposable m_Disposable = new CompositeDisposable();

		private GridConsoleNavigationBehaviour m_NavigationBehaviour;
		private InputLayer m_InputLayer;

		private void SetupNavigationBehaviour()
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour();

			m_SetsViews.ForEach(view => m_NavigationBehaviour.AddEntityHorizontal(view as IConsoleEntity));
			m_NavigationBehaviour.FocusOnFirstValidEntity();

			m_InputLayer = GetInputLayer(m_NavigationBehaviour);
			GamePad.Instance.PushLayer(m_InputLayer);
		}

		private InputLayer GetInputLayer(ConsoleNavigationBehaviour navigationBehavior)
		{
			var inputLayer = navigationBehavior.GetInputLayer(new InputLayer
				{ ContextName = "ActionBarWeaponConsoleViewInput" });

			inputLayer.AddButton(_ => OnWeaponSetVisibleChange(false), Action.Decline);
			inputLayer.AddButton(_ => OnWeaponSetVisibleChange(false), Action.DPadDown);

			return inputLayer;
		}

		public void SetFocus(bool value)
		{
			m_FocusMultiButton.SetFocus(value);
			m_FocusMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
		}

		public bool IsValid()
		{
			return true;
		}

		public bool CanConfirmClick()
		{
			return true;
		}

		public string GetConfirmClickHint()
		{
			return string.Empty;
		}

		public void OnConfirmClick()
		{
			OnWeaponSetVisibleChange(true);
		}

		protected override void Show(bool value)
		{
			if (value)
			{
				SetupNavigationBehaviour();

				m_WeaponSetContainer.gameObject.SetActive(true);
				m_WeaponSetCanvasGroup.DOFade(1, UIConsts.FadeTime).SetUpdate(true);
				//transform.DOLocalMoveY(ViewModel.TBMModeIsActive.Value ? m_ShowPosTBMY : m_ShowPosY, UIConsts.FadeTime).SetUpdate(true);
			}
			else
			{
				m_NavigationBehaviour.UnFocusCurrentEntity();
				GamePad.Instance.PopLayer(m_InputLayer);

				m_WeaponSetCanvasGroup.DOFade(0, UIConsts.FadeTime).SetUpdate(true).OnComplete(() =>
				{
					m_WeaponSetContainer.gameObject.SetActive(false);
				});
				//transform.DOLocalMoveY(m_HidePosY, UIConsts.FadeTime).SetUpdate(true);
			}
		}

		private void OnDestroy()
		{
			m_Disposable.Dispose();
			m_Disposable = null;
		}
	}
}