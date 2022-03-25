using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Formation;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.Formation
{
	public class FormationCharacterVM : BaseDisposable, IViewModel
	{
		public readonly Sprite PortraitSprite;
		public readonly ReactiveCommand FormationChanged;
		public readonly ReactiveProperty<bool> IsInteractable = new ReactiveProperty<bool>();
		public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>();

		private readonly int m_Index;
		public readonly UnitEntityData Unit;
		
		public readonly Vector2 OffsetPosition = new Vector2(0, 6 * FormationCharacterDragComponent.OneStepSize);
		
		
		public FormationCharacterVM(int index, UnitEntityData unit, ReactiveCommand formationChanged)
		{
			m_Index = index;
			Unit = unit;
			FormationChanged = formationChanged;
			PortraitSprite = unit.Portrait.SmallPortrait;
			
			SetupCharacter();
			AddDisposable(formationChanged.Subscribe(_ => SetupCharacter()));
		}

		private void SetupCharacter()
		{
			var formationManager = Game.Instance.Player.FormationManager;
			IsInteractable.Value = formationManager.IsCustomFormation;
		}

		public Vector2 GetOffset()
		{
			return Game.Instance.Player.FormationManager.CurrentFormation.GetOffset(m_Index, Unit);
		}
		
		public Vector3 GetLocalPosition()
		{
			return GetOffset() * FormationCharacterDragComponent.Scaler + OffsetPosition;
		}

		protected override void DisposeImplementation()
		{
		}

		public void MoveCharacter(Vector2 vector)
		{
			Game.Instance.Player.FormationManager.CurrentFormation.SetOffset(m_Index, Unit, vector);
		}
	}
}