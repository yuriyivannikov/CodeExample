using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.UI.Formation;
using Kingmaker.UI.MVVM._VM.Formation;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM._ConsoleView.Formation
{
	public class FormationCharacterConsoleView : ViewBase<FormationCharacterVM>, IFloatConsoleNavigationEntity
	{
		[SerializeField, UsedImplicitly]
		private OwlcatButton m_Button;
		
		[SerializeField, UsedImplicitly]
		private OwlcatMultiButton m_FocusMultiButton;
		
		[SerializeField, UsedImplicitly]
		private Image m_Portrait;
		
		[SerializeField, UsedImplicitly]
		private Material m_GreyScaleMaterial;
		
		[SerializeField, UsedImplicitly]
		private FormationCharacterDragComponent m_FormationCharacterDragComponent;

		protected override void BindViewImplementation()
		{
			m_FormationCharacterDragComponent.Initialize(transform.parent as RectTransform);
			
			m_Portrait.sprite = ViewModel.PortraitSprite;
			SetupPosition();
			AddDisposable(ViewModel.FormationChanged.Subscribe(_ => SetupPosition()));
			AddDisposable(ViewModel.IsInteractable.Subscribe(SetInteractable));
			AddDisposable(ViewModel.IsVisible.Subscribe(SetVisible));
			AddDisposable(m_FormationCharacterDragComponent.DragCommand.Subscribe(_ => 
				ViewModel.MoveCharacter((transform.localPosition - new Vector3(ViewModel.OffsetPosition.x, ViewModel.OffsetPosition.y)) / FormationCharacterDragComponent.Scaler)));
		}

		public void MoveCharacter(Vector3 vec, bool moveFreely)
		{
			var newVec = transform.localPosition;
			
			if (!moveFreely)
			{
				float oneStepSize = FormationCharacterDragComponent.OneStepSize;
				vec *= (oneStepSize * 1.25f);
				vec.x -= vec.x % oneStepSize;
				vec.y -= vec.y % oneStepSize;
				newVec.x -= newVec.x % oneStepSize;
				newVec.y -= newVec.y % oneStepSize;
			} else
				vec *= 7.5f;

			newVec += vec;
			ViewModel.MoveCharacter(new Vector2(newVec.x, newVec.y - ViewModel.OffsetPosition.y) / FormationCharacterDragComponent.Scaler);
			transform.localPosition = newVec;
			m_FormationCharacterDragComponent.ClampToWindow();
		}

		private void SetupPosition()
		{
			transform.localPosition = ViewModel.GetLocalPosition();
		}

		private void SetInteractable(bool value) 
		{
			m_Button.Interactable = value;
			m_FormationCharacterDragComponent.IsInteractable = value;
			m_Portrait.material = value ? null : m_GreyScaleMaterial;
		}
		
		private void SetVisible(bool value)
		{
			m_Button.gameObject.SetActive(value);
			m_FormationCharacterDragComponent.gameObject.SetActive(value);
		}
		
		protected override void DestroyViewImplementation()
		{
			
		}

		public void SetFocus(bool value)
		{
			m_Button.SetFocus(value);
			m_FocusMultiButton.SetFocus(value);
			m_FocusMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
		}

		public bool IsValid()
		{
			return true;
		}

		public Vector2 GetPosition()
		{
			return ViewModel.GetLocalPosition();
		}

		public List<IFloatConsoleNavigationEntity> GetNeighbours()
		{
			return null;
		}
	}
}