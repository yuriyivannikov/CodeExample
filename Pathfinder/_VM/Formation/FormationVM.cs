using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Formations;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.Formation
{
	public class FormationVM : VMBase, IFormationUIHandlers
	{
		public readonly SelectionGroupRadioVM<FormationSelectionItemVM> FormationSelector;
		private readonly List<FormationSelectionItemVM> m_FormationSelectionItemViewModels = new List<FormationSelectionItemVM>();
		
		public IReadOnlyReactiveProperty<int> SelectedFormationPresetIndex => m_SelectedFormationPresetIndex;
		private readonly IntReactiveProperty m_SelectedFormationPresetIndex = new IntReactiveProperty();
		
		public readonly List<FormationCharacterVM> Characters = new List<FormationCharacterVM>();
		
		public readonly BoolReactiveProperty IsPreserveFormation = new BoolReactiveProperty();
		public bool IsCustomFormation => FormationManager.IsCustomFormation;
		
		private readonly ReactiveCommand m_FormationChanged = new ReactiveCommand();
		
		private Action m_Close;
		
		private PartyFormationManager FormationManager => Game.Instance.Player.FormationManager;
		
		public FormationVM(Action close)
		{
			m_Close = close;
			
			AddDisposable(EventBus.Subscribe(this));
			
			for (int i = 0; i < BlueprintRoot.Instance.Formations.PredefinedFormations.Length; i++)
			{
				m_FormationSelectionItemViewModels.Add(new FormationSelectionItemVM(i));
			}
			
			var selectedItemVM = new ReactiveProperty<FormationSelectionItemVM>(m_FormationSelectionItemViewModels[FormationManager.CurrentFormationIndex]);
			AddDisposable(FormationSelector = new SelectionGroupRadioVM<FormationSelectionItemVM>(m_FormationSelectionItemViewModels, selectedItemVM));
			AddDisposable(selectedItemVM.Subscribe(OnFormationSelectedEntityChange));
			AddDisposable(m_SelectedFormationPresetIndex.Subscribe(_ => PartyFormationChanged()));
			
			var units = Game.Instance.Player.PartyAndPets.Where(u => u.IsDirectlyControllable).ToList();
			int index = 0;
			foreach (var unit in units)
			{
				Characters.Add(new FormationCharacterVM(index++, unit, m_FormationChanged));
			}

			PartyFormationChanged();
			
			EventBus.RaiseEvent<IUIEventHandler>(h => h.HandleUIEvent(UIEventType.FormationWindowOpen));
		}

		private void OnFormationSelectedEntityChange(FormationSelectionItemVM itemVM)
		{
			FormationManager.CurrentFormationIndex = itemVM.FormationIndex;
			m_SelectedFormationPresetIndex.Value = itemVM.FormationIndex;
			
			IsPreserveFormation.Value = FormationManager.GetPreserveFormation();

			m_FormationChanged?.Execute();
		}

		protected override void DisposeImplementation()
		{
			m_FormationSelectionItemViewModels.ForEach(s => s.Dispose());
			m_FormationSelectionItemViewModels.Clear();
			
			Characters.ForEach(c => c.Dispose());
			Characters.Clear();
			
			Close();
			m_Close = null;
			
			EventBus.RaiseEvent<IUIEventHandler>(h => h.HandleUIEvent(UIEventType.FormationWindowClose));
		}

		public void Close()
		{
			m_Close?.Invoke();
		}

		public void SwitchPreserveFormation()
		{
			FormationManager.SetPreserveFormation(!FormationManager.GetPreserveFormation());
			IsPreserveFormation.Value = FormationManager.GetPreserveFormation();
		}

		public void ResetCurrentFormation()
		{
			FormationManager.ResetCurrentCustomFormation();
			m_FormationChanged?.Execute();
		}

		public void CurrentFormationChanged(int currentFormationIndex)
		{
			
		}

		public void PartyFormationChanged()
		{
			foreach (var character in Characters)
			{
				var isVisible = IsCustomFormation || character.Unit.GetRider() == null;
				character.IsVisible.Value = isVisible;
			}
			
			m_FormationChanged?.Execute();
		}
	}
}