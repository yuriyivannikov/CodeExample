using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM._PCView.Formation;
using Kingmaker.UI.MVVM._VM.Formation;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using Rewired;
using RewiredConsts;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Formation
{
	public class FormationConsoleView : ViewBase<FormationVM>
	{
		[Header("Selector")]
		[SerializeField, UsedImplicitly]
		private FormationSelectorPCView m_FormationSelectorPCView;
		
		[Header("Character")]
		[SerializeField, UsedImplicitly]
		private RectTransform m_CharacterContainer;
		
		[SerializeField, UsedImplicitly]
		private FormationCharacterConsoleView m_CharacterView;
		
		[Header("Buttons")]
		[SerializeField, UsedImplicitly]
		private OwlcatButton m_CloseButton;

		private readonly List<FormationCharacterConsoleView> m_Characters = new List<FormationCharacterConsoleView>();
		
		private InputLayer m_InputLayer;
		public FloatConsoleNavigationBehaviour NavigationBehaviour { get; private set; }
		
		[Header("Animator")]
		[SerializeField]
		private FadeAnimator m_FadeAnimator;

		[Header("Console")]
		[SerializeField]
		private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;
		
		[Header("Hints")]
		[SerializeField, UsedImplicitly]
		private ConsoleHintsWidget m_ConsoleHintsWidget;
		
		[SerializeField]
		private ConsoleHint m_LeftHint;
		
		[SerializeField]
		private ConsoleHint m_RightHint;

		private bool m_MoveFreely = false;

		public void Initialize()
		{
			m_FadeAnimator.Initialize();
			m_FormationSelectorPCView.Initialize();
		}

		protected override void BindViewImplementation()
		{
			NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_Parameters);
			m_InputLayer = NavigationBehaviour.GetInputLayer(new InputLayer {ContextName = "Formation"});
			
			AddDisposable(m_LeftHint.Bind(m_InputLayer.AddButton(_ => ViewModel.FormationSelector.SelectPrevValidEntity(), Action.LeftUp)));
			AddDisposable(m_RightHint.Bind(m_InputLayer.AddButton(_ => OnSelectFormation(), Action.RightUp)));

			m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(_ => ViewModel.Close(), Action.Decline),
				UIStrings.Instance.CommonTexts.Close);
			
			m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(_ => ViewModel.SwitchPreserveFormation(),
					Action.RightStickButton, ViewModel.IsPreserveFormation),
				UIStrings.Instance.FormationTexts.HoldTheLine);
			
			m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(_ => ViewModel.SwitchPreserveFormation(),
					Action.RightStickButton, ViewModel.IsPreserveFormation.Not().ToReactiveProperty()),
				UIStrings.Instance.FormationTexts.FreeMovement);
			
			m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(_ => ViewModel.ResetCurrentFormation(), Action.Func01),
				UIStrings.Instance.FormationTexts.RestoreToDefault);
			
			m_ConsoleHintsWidget.CreateCustomHint(
				new List<int> { Action.RightStickX, Action.RightStickY }, m_InputLayer,
				UIStrings.Instance.FormationTexts.MoveCharacter);
			
			m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(_ => SetMoveFreely(true),
					Action.LeftBottom),
				UIStrings.Instance.FormationTexts.MoveCharacterFree);
			m_InputLayer.AddButton(_ => SetMoveFreely(false),
				Action.LeftBottom, InputActionEventType.ButtonJustReleased);
			m_InputLayer.AddButton(_ => SetMoveFreely(false),
				Action.LeftBottom, InputActionEventType.ButtonLongPressJustReleased);
			m_InputLayer.AddCursor(MoveCharacter, Action.RightStickX, Action.RightStickY);

			m_FadeAnimator.AppearAnimation();
			UISoundController.Instance.Play(UISoundType.FormationOpen);
			
			m_FormationSelectorPCView.Bind(ViewModel.FormationSelector);
			
			AddDisposable(ViewModel.SelectedFormationPresetIndex.Subscribe(OnFormationPresetIndexChanged));
			
			AddDisposable(Game.Instance.UI.EscManager.Subscribe(ViewModel.Close));
			
			foreach (var characterVm in ViewModel.Characters)
			{
				var character = WidgetFactory.GetWidget(m_CharacterView);
				character.transform.SetParent(m_CharacterContainer, false);
				character.Bind(characterVm);
				
				m_Characters.Add(character);
			}

			UpdateNavigation();
			
			AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		}

		private void SetMoveFreely(bool value)
		{
			m_MoveFreely = value;
		}

		private void UpdateNavigation()
		{
			NavigationBehaviour.Clear();
			if (ViewModel.IsCustomFormation)
			{
				NavigationBehaviour.AddEntities(m_Characters);
				NavigationBehaviour.FocusOnFirstValidEntity();
			}
		}

		private void OnSelectFormation()
		{
			ViewModel.FormationSelector.SelectNextValidEntity();
			ViewModel.FormationSelector.SelectedEntity.Value.SetSelectedFromView(true);
		}

		private void MoveCharacter(InputActionEventData data, Vector2 vec)
		{
			var character = NavigationBehaviour.Focus.Value as FormationCharacterConsoleView;
			if (character == null)
				return;
			
			character.MoveCharacter(vec, m_MoveFreely);
		}

		public void OnFormationPresetIndexChanged(int formationPresetIndex)
		{
			OnFormationPresetChanged(formationPresetIndex);
			UpdateNavigation();
		}

		public void OnFormationPresetChanged(int formationPresetIndex)
		{
			float minY = 0f;
			foreach (var characterVm in ViewModel.Characters)
			{
				var localPosition = characterVm.GetLocalPosition();
				
				if (localPosition.y < minY)
					minY = localPosition.y;
			}
			
			const float minPositionY = -185;
			if (minY < minPositionY)
			{
				var newScale = minPositionY / minY;
				m_CharacterContainer.localScale = new Vector3(newScale, newScale, m_CharacterContainer.localScale.z);
			}
			else
				m_CharacterContainer.localScale = Vector3.one;
		}

		protected override void DestroyViewImplementation()
		{
			m_FadeAnimator.DisappearAnimation();
			UISoundController.Instance.Play(UISoundType.FormationClose);
			
			m_FormationSelectorPCView.Unbind();
			
			m_Characters.ForEach(WidgetFactory.DisposeWidget);
			m_Characters.Clear();
		}
	}
}