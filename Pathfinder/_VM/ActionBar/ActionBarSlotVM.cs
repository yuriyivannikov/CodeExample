using System.Collections.Generic;
using System.Linq;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TurnBased.Controllers;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ActionBarSlotVM : BaseDisposable, IViewModel, IGameModeHandler
	{
		public MechanicActionBarSlot MechanicActionBarSlot { get; private set; }
		public int Index { get; }
		public int SpellLevel { get; }

		public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();
		public readonly ReactiveProperty<Sprite> ForeIcon = new ReactiveProperty<Sprite>();
		public readonly ReactiveProperty<string> Name = new ReactiveProperty<string>();

		public readonly ReactiveProperty<Sprite> DecorationSprite = new ReactiveProperty<Sprite>();
		public readonly ReactiveProperty<Color> DecorationColor = new ReactiveProperty<Color>();

		public readonly ReactiveProperty<int> ResourceCount = new ReactiveProperty<int>();

		public readonly ReactiveProperty<bool> IsCasting = new ReactiveProperty<bool>();
		public readonly ReactiveProperty<bool> IsPossibleActive = new ReactiveProperty<bool>();
		public readonly ReactiveProperty<bool> IsActive = new ReactiveProperty<bool>();
		public readonly ReactiveProperty<bool> HasConvert = new ReactiveProperty<bool>();
		public readonly ReactiveProperty<bool> IsEmpty = new ReactiveProperty<bool>();
		public readonly ReactiveProperty<bool> IsRuntimeOnly = new ReactiveProperty<bool>();

		public readonly ReactiveCommand<bool> OnClickCommand = new ReactiveCommand<bool>();

		public readonly ReactiveProperty<ActionBarConvertedVM> ConvertedVm =
			new ReactiveProperty<ActionBarConvertedVM>(null);

		public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

		// private readonly Action<ActionBarSlotVM> m_SetSlotSelected;
		// private BoolReactiveProperty ActionBarIsActive { get; set; }
		
		// public IReadOnlyReactiveProperty<bool> Select => m_Select;
		// private readonly ReactiveProperty<bool> m_Select = new ReactiveProperty<bool>();
		
		
		private List<AbilityData> m_Conversion;
		private bool m_TempTooltipState;

		private TurnController TurnController
			=> Game.Instance.TurnBasedCombatController.CurrentTurn;

		public bool CanUseOnAllCharacters = false;

		public ActionBarSlotVM(int index = -1, int spellLevel = -1)
		{
			Index = index;
			SpellLevel = spellLevel;
			
			AddDisposable(EventBus.Subscribe(this));

			//Is is bugged in PC View. May be it need move to ConsoleSlotsView
			//AddDisposable(m_Select.Subscribe(_=>TryUpdatePrediction()));
			//AddDisposable(IsPossibleActive.Skip(1).Subscribe(_=>TryUpdatePrediction()));
		}

		public ActionBarSlotVM(MechanicActionBarSlot abs, int index = -1, int spellLevel = -1) : this(index, spellLevel)
		{
			SetMechanicSlot(abs);
		}
		
		// public ActionBarSlotVM(int index, Action<ActionBarSlotVM> setSlotSelected, BoolReactiveProperty actionBarIsActive) : this(index)
		// {
		// 	ActionBarIsActive = actionBarIsActive;
		// 	// m_SetSlotSelected = setSlotSelected;
		//
		// 	// AddDisposable(m_TooltipObject.Subscribe(_=>TryUpdatePrediction()));
		// }

		protected override void DisposeImplementation()
		{
			CloseConvert();
		}
		
		// public void SetSlotSelected()
		// {
		// 	m_SetSlotSelected?.Invoke(this);
		// }
		
		// public void SetSelect(bool value)
		// {
		// 	OnHover(value);
		// 	m_Select.Value = value;
		// }

		public void SetMechanicSlot(MechanicActionBarSlot abs)
		{
			if (abs.IsBad())
			{
				var replacement = UnitUISettings.GetBadSlotReplacement(abs, abs.Unit);
				if (replacement == null || replacement.IsBad())
				{
					PFLog.Default.Error("Bad slot set for actionbar UI: " + abs);
					SetMechanicSlot(new MechanicActionBarSlotEmpty());
					return;
				}
				else
				{
					abs = replacement;
					PFLog.Default.Warning("Bad slot set for actionbar UI: " + abs + " was replaced by" + replacement);
				}
			}

			IsEmpty.Value = abs is MechanicActionBarSlotEmpty;
			IsRuntimeOnly.Value = abs.IsRuntimeOnly();

			MechanicActionBarSlot = abs;
			m_Conversion = MechanicActionBarSlot.GetConvertedAbilityData();

			Icon.Value = MechanicActionBarSlot.GetIcon();
			ForeIcon.Value = MechanicActionBarSlot.GetForeIcon();
			Name.Value = MechanicActionBarSlot.GetTitle();
			HasConvert.Value = m_Conversion.Count > 0;
			DecorationSprite.Value = MechanicActionBarSlot.GetDecorationSprite();
			DecorationColor.Value = MechanicActionBarSlot.GetDecorationColor();
			ResourceCount.Value = MechanicActionBarSlot.GetResource();
			Tooltip.Value = MechanicActionBarSlot.GetTooltipTemplate();

			ConvertedVm.Value?.Dispose();
			ConvertedVm.Value = null;
		}

		public TooltipBaseTemplate GetTooltipTemplate()
		{
			return MechanicActionBarSlot?.GetTooltipTemplate();
		}

		public void UpdateResource()
		{
			using (ProfileScope.New("ActionBarSlotVM SetResource"))
			{
				if (MechanicActionBarSlot == null || MechanicActionBarSlot.IsBad())
				{
					return;
				}

				using (ProfileScope.New("ActionBarSlotVM UpdateResourceCount"))
				{
					MechanicActionBarSlot.UpdateResourceCount();
				}

				using (ProfileScope.New("ActionBarSlotVM GetResource"))
				{
					ResourceCount.Value = MechanicActionBarSlot.ResourceCount;
				}
				
				using (ProfileScope.New("ActionBarSlotVM IsCasting"))
				{
					IsCasting.Value = MechanicActionBarSlot.IsCasting();
				}
				
				using (ProfileScope.New("ActionBarSlotVM IsActive"))
				{
					IsActive.Value = MechanicActionBarSlot.IsActive();
				}
				
				using (ProfileScope.New("ActionBarSlotVM IsPossibleActive"))
				{
					IsPossibleActive.Value = (HasConvert.Value && ResourceCount.Value != 0 || !HasConvert.Value) && MechanicActionBarSlot.IsPossibleActive(MechanicActionBarSlot.ResourceCount);
				}
			}
		}

		public virtual void OnMainClick()
		{
			if (MechanicActionBarSlot == null)
			{
				return;
			}

			switch (MechanicActionBarSlot)
			{
				case MechanicActionBarSlotAbility slotAbility:
					if (slotAbility.Ability.Blueprint.HasVariants)
					{
						OnShowConvertRequest();
						return;
					}

					break;
				case MechanicActionBarSlotMemorizedSpell memorizedSpell:
					if (Enumerable.Any(m_Conversion) && !memorizedSpell.IsPossibleActive())
					{
						OnShowConvertRequest();
						return;
					}

					break;
				case MechanicActionBarSlotSpontaneousSpell spontaneousSpell:
					if (Enumerable.Any(m_Conversion) && spontaneousSpell.GetResource() > 0 && !spontaneousSpell.IsPossibleActive())
					{
						OnShowConvertRequest();
						return;
					}

					break;
			}

			if (MechanicActionBarSlot.IsPossibleActive())
			{
				var mechanicActionBarSlot = MechanicActionBarSlot as MechanicActionBarSlotActivableAbility;
				bool ignore = false;
				if (mechanicActionBarSlot != null)
				{
					var abilityBlueprint = mechanicActionBarSlot.ActivatableAbility.Blueprint;
					ignore =
						!(abilityBlueprint.ActivationType == AbilityActivationType.WithUnitCommand &&
						  UnitActivateAbility.GetCommandType(mechanicActionBarSlot.ActivatableAbility) ==
						  UnitCommand.CommandType.Standard);
				}

				OnClickCommand.Execute(ignore);
			}

			MechanicActionBarSlot.OnClick();

			if (CombatController.IsInTurnBasedCombat())
			{
				// If an ActivatableAbility, such as Arcane Strike, was toggled, we need to update predictions, as if we removed the cursor from the slot and moved it back again.
				// However, if the ability is turned ON, the real change may be delayed by one frame, so we need to delay the update.
				if (MechanicActionBarSlot.IsActive())
					DelayedInvoker.InvokeInFrames(() => OnHover(true), 1);
				else
					OnHover(true);	
			}
		}

		public void OnSupportClick()
		{
			MechanicActionBarSlot?.OnAutoUseToggle();
			OnClickCommand.Execute(true);
		}

		public void OnShowConvertRequest()
		{
			if (ConvertedVm.Value == null || ConvertedVm.Value.IsDisposed)
			{
				if (m_Conversion.Count == 0)
				{
					return;
				}

				ConvertedVm.Value =
					new ActionBarConvertedVM(
						m_Conversion.Select(abilityData => new MechanicActionBarSlotSpontaneusConvertedSpell
						{
							Spell = abilityData, Unit = MechanicActionBarSlot.Unit
						}).ToList(),
						CloseConvert);
			}
			else
			{
				CloseConvert();
			}
		}

		public void OnHover(bool state)
		{
			if (MechanicActionBarSlot == null)
				return;

			if (CombatController.IsInTurnBasedCombat())
			{
				bool unitCommandsEmpty = TurnController?.Rider?.Commands.Empty ?? true;
				bool unitMountCommandsEmpty = TurnController?.Mount?.Commands.Empty ?? true;

				if (!unitCommandsEmpty || !unitMountCommandsEmpty)
					return;
			}

			MechanicActionBarSlot.OnHover(state);

			if (TurnController == null)
				return;

			MechanicActionBarSlot.TrySetAbilityToPrediction(state, false);
		}

		public void CloseConvert()
		{
			ConvertedVm.Value?.Dispose();
			ConvertedVm.Value = null;
		}
		
		// public void SetMove(ActionBarSlotVM vm)
		// {
		// 	//IsSemiHide.Value = ims != null && ims.Index != Index;
		// }
		public void OnGameModeStart(GameModeType gameMode)
		{
			OnHover(false);
		}

		public void OnGameModeStop(GameModeType gameMode)
		{
		}
	}
}