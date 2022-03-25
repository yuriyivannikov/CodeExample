using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ControlCharactersVM : BaseDisposable, IViewModel
	{
		public readonly ReactiveProperty<bool> IsStealthEnabled = new ReactiveProperty<bool>(false);
		public readonly ReactiveProperty<bool> IsAiEnabled = new ReactiveProperty<bool>(false);

		public ControlCharactersVM()
		{
			AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(_ =>
			{
				IsStealthEnabled.Value = IsInStealthState();
				IsAiEnabled.Value = IsInAiState();
			}));
		}

		protected override void DisposeImplementation()
		{
		}

		public void OnStealthClick()
		{
			var selectedUnits = SelectionManagerBase.Instance.SelectedUnits;
			bool inStealth = IsInStealthState();
			selectedUnits.ForEach(u => SetStealthEnabled(u, !inStealth));
		}

		private bool IsInStealthState()
		{
            foreach (var unit in SelectionManagerBase.Instance.SelectedUnits)
            {
                if (unit.Descriptor.State.IsInStealth)
                    return true;
            }

            return false;
		}

		private static void SetStealthEnabled(UnitEntityData unit, bool enabled)
		{
			unit.Stealth.WantEnterStealth = enabled;
			if (enabled)
			{
				unit.Stealth.ShouldExitStealth = true;
			}

			Game.Instance.StealthController.TickUnit(unit);

			if (enabled && !unit.Descriptor.State.IsInStealth)
			{
				unit.Stealth.WantEnterStealth = false;
			}
		}

		public void OnAiClick()
		{
			var selectedUnits = Game.Instance.UI.SelectionManager.SelectedUnits;
			bool isAIEnable = selectedUnits.All(u => u.IsAIEnabled);
			foreach (var unit in selectedUnits)
			{
				unit.IsAIEnabled = !isAIEnable;
			}
		}

		private bool IsInAiState()
		{
            foreach (var unit in SelectionManagerBase.Instance.SelectedUnits)
            {
                if (!unit.IsAIEnabled)
                    return false;
            }

            return true;
        }
	}
}