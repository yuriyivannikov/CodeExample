using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.View;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ActionBarInspectVM : BaseDisposable, IViewModel, IUnitDirectHoverUIHandler
	{
		public readonly ReactiveProperty<UnitEntityData> Unit = new ReactiveProperty<UnitEntityData>(null);
		
		public ActionBarInspectVM()
		{
			AddDisposable(EventBus.Subscribe(this));
		}

		protected override void DisposeImplementation()
		{
		}

		public void HandleHoverChange(UnitEntityView unitEntityView, bool isHover)
		{
			Unit.Value = 
				isHover && !unitEntityView.EntityData.IsDirectlyControllable && unitEntityView.EntityData.IsPlayersEnemy ? 
					unitEntityView.EntityData : 
					null;
		}
	}
}