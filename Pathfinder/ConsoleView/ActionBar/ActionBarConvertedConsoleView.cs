using Kingmaker.UI.MVVM._PCView.ActionBar;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using RewiredConsts;

namespace Kingmaker.UI.MVVM._ConsoleView.ActionBar
{
	public class ActionBarConvertedConsoleView : ActionBarConvertedView
	{
		private GridConsoleNavigationBehaviour m_NavigationBehaviour;
		private InputLayer m_InputLayer;

		protected override void BindViewImplementation()
		{
			base.BindViewImplementation();

			SetupNavigationBehaviour();
		}

		private void SetupNavigationBehaviour()
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour();

			m_Slots.ForEach(view => m_NavigationBehaviour.AddEntityGrid(view as IConsoleEntity));
			m_NavigationBehaviour.FocusOnFirstValidEntity();

			m_InputLayer = GetInputLayer(m_NavigationBehaviour);
			AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		}

		private InputLayer GetInputLayer(ConsoleNavigationBehaviour navigationBehavior)
		{
			var inputLayer = navigationBehavior.GetInputLayer(new InputLayer
				{ ContextName = "ActionBarConvertedConsoleViewInput" });

			inputLayer.AddButton(_ => ViewModel.Close(), Action.Decline);
			inputLayer.AddButton(_ => ViewModel.Close(), Action.Confirm);

			return inputLayer;
		}
	}
}