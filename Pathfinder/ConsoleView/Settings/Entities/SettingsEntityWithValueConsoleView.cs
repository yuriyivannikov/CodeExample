using Kingmaker.PubSubSystem;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Action = RewiredConsts.Action;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.Entities
{
	public abstract class SettingsEntityWithValueConsoleView<TSettingsEntityVM> : SettingsEntityConsoleView<TSettingsEntityVM>, IConsoleNavigationEntity, INavigationHorizontalDirectionsHandler
		where TSettingsEntityVM : SettingsEntityWithValueVM
	{
		[SerializeField]
		private Image m_HighlightedImage;

		[SerializeField]
		private Color NormalColor = Color.clear;

		[SerializeField]
		private Color OddColor = new Color(0.77f, 0.75f, 0.69f, 0.29f);

		[SerializeField]
		private Color HighlightedColor = new Color(0.52f, 0.52f, 0.52f, 0.29f);
		
		[SerializeField]
		private Image m_PointImage;
		
		[SerializeField]
		private Image m_MarkImage;
		
		[SerializeField]
		protected ConsoleHint m_ConsoleHintPrev;

		[SerializeField]
		protected ConsoleHint m_ConsoleHintNext;
		
		[SerializeField]
		private VirtualListLayoutElementSettings m_LayoutSettings;
		public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

		private BoolReactiveProperty m_IsActiveConsoleHint = new BoolReactiveProperty();

		protected override void BindViewImplementation()
		{
			base.BindViewImplementation();
			
			AddDisposable(ViewModel.ModificationAllowed.Subscribe(OnModificationChanged));
			AddDisposable(ViewModel.IsChanged.Subscribe(UpdatePoints));

			SetupColor(false);
			
			var inputLayer = GamePad.Instance.CurrentInputLayer;
			AddDisposable(m_ConsoleHintPrev.BindCustomAction(Action.DPadLeft, inputLayer, m_IsActiveConsoleHint));
			AddDisposable(m_ConsoleHintNext.BindCustomAction(Action.DPadRight, inputLayer, m_IsActiveConsoleHint));
		}

		protected override void DestroyViewImplementation()
		{
			base.DestroyViewImplementation();

			m_IsActiveConsoleHint.Value = false;
		}

		private void UpdatePoints(bool isChanged)
		{
			if (m_PointImage != null)
				m_PointImage.gameObject.SetActive(!isChanged && !ViewModel.IsSet);
			
			if (m_MarkImage != null)
				m_MarkImage.gameObject.SetActive(isChanged);
		}

		private void SetupColor(bool isHighlighted)
		{
			var color = ViewModel.IsOdd ? OddColor : NormalColor;
			if (m_HighlightedImage != null)
				m_HighlightedImage.color = isHighlighted ? HighlightedColor : color;
		}

		public abstract void OnModificationChanged(bool allowed);

		public virtual void SetFocus(bool value)
		{
			SetupColor(value);
			
			if (value)
				EventBus.RaiseEvent<ISettingsDescriptionUIHandler>(h 
					=> h.HandleShowSettingsDescription(ViewModel.Title, ViewModel.Description));
			else
				EventBus.RaiseEvent<ISettingsDescriptionUIHandler>(h 
					=> h.HandleHideSettingsDescription());

			m_IsActiveConsoleHint.Value = value;
		}

		public bool IsValid()
		{
			return true;
		}

		public abstract bool HandleLeft();
		public abstract bool HandleRight();
	}
}