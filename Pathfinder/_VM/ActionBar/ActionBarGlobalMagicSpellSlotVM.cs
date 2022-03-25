using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Crusade.GlobalMagic.SpellsManager;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.UnitSettings;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ActionBarGlobalMagicSpellSlotVM : ActionBarSlotVM
	{
		public readonly SpellState SpellState;
		
		public ActionBarGlobalMagicSpellSlotVM(MechanicActionBarSlot abs, int index = -1, int spellLevel = -1) : base(abs, index, spellLevel)
		{
			SpellState = (MechanicActionBarSlot as MechanicActionBarSlotGlobalMagicSpell)?.SpellState;
		}

		public override void OnMainClick()
		{
			if (SpellState != null)
			{
				if (SpellState.Cooldown > 0)
				{
					EventBus.RaiseEvent<IWarningNotificationUIHandler>(h 
						=> h.HandleWarning(UIStrings.Instance.GlobalMap.GlobalMagicSpellOnCooldownWarning));
					return;
				}
			}

			base.OnMainClick();
		}
	}
}