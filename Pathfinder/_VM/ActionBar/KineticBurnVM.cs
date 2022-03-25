using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Class.Kineticist;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class KineticBurnVM
		: BaseDisposable, IViewModel,
			IKineticistGlobalHandler,
			IUnitChangedAfterRespecHandler,
			ILevelUpCompleteUIHandler
	{
		private readonly IReadOnlyReactiveProperty<UnitEntityData> m_SelectedUnit;

		public readonly ReactiveProperty<bool> IsKinetic = new ReactiveProperty<bool>(false);
		
		public readonly ReactiveProperty<int> AcceptedBurn = new ReactiveProperty<int>(0);
		public readonly ReactiveProperty<int> MaxBurn = new ReactiveProperty<int>(0);

		private UnitPartKineticist m_UnitPartKineticist;

		public KineticBurnVM(IReadOnlyReactiveProperty<UnitEntityData> selectedUnit)
		{
			m_SelectedUnit = selectedUnit;

			AddDisposable(selectedUnit.Subscribe(OnUnitChanged));
			AddDisposable(EventBus.Subscribe(this));
		}

		protected override void DisposeImplementation()
		{
		}

		private void OnUnitChanged(UnitEntityData unit)
		{
			m_UnitPartKineticist = unit?.Get<UnitPartKineticist>();
			IsKinetic.Value = m_UnitPartKineticist != null;

			UpdateValues();
		}

		private void UpdateValues()
		{
			if (m_UnitPartKineticist == null)
			{
				return;
			}

			AcceptedBurn.Value = m_UnitPartKineticist.AcceptedBurn;
			MaxBurn.Value = m_UnitPartKineticist.MaxBurn;
		}

		public void HandleKineticistBurnValueChanged(
			UnitPartKineticist kineticist, int prevBurnValue, AbilityData ability)
		{
			UpdateValues();
		}

		public void HandleUnitChangedAfterRespec(UnitEntityData unit)
		{
			if (unit != m_SelectedUnit.Value)
			{
				return;
			}

			OnUnitChanged(m_SelectedUnit.Value);
		}

		public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
		{
			if (unit != m_SelectedUnit.Value)
			{
				return;
			}

			OnUnitChanged(m_SelectedUnit.Value);
		}
	}
}