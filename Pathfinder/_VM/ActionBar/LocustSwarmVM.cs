using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class LocustSwarmVM : BaseDisposable, IViewModel, ILocustSwarmHandler
	{
		private readonly IReadOnlyReactiveProperty<UnitEntityData> m_SelectedUnit;

		public readonly ReactiveProperty<bool> IsLocust = new ReactiveProperty<bool>();

		public readonly ReactiveProperty<int> CurrentScale = new ReactiveProperty<int>();
		public readonly ReactiveProperty<int> CurrentStrength = new ReactiveProperty<int>();

		private UnitPartLocustSwarm m_UnitPartLocustSwarm;

		public LocustSwarmVM(IReadOnlyReactiveProperty<UnitEntityData> selectedUnit)
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
			m_UnitPartLocustSwarm = unit?.Get<UnitPartLocustSwarm>();
			IsLocust.Value = m_UnitPartLocustSwarm != null;

			UpdateValues();
		}

		private void UpdateValues()
		{
			if (m_UnitPartLocustSwarm == null)
			{
				return;
			}

			CurrentScale.Value = m_UnitPartLocustSwarm.CurrentScale;
			CurrentStrength.Value = m_UnitPartLocustSwarm.CurrentStrength;
		}

		public void HandleLocustSwarmChanged(UnitEntityData unit)
		{
			if (unit != m_SelectedUnit.Value)
			{
				return;
			}

			OnUnitChanged(m_SelectedUnit.Value);
		}
	}
}