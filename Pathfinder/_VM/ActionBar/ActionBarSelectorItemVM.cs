using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UI.UnitSettings;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.ActionBar
{
	public class ActionBarSelectorItemVM : VirtualListElementVMBase
	{
		public MechanicActionBarSlot MechanicActionBarSlot { get; }

		public IReadOnlyReactiveProperty<Sprite> Icon => m_Icon;
		private readonly ReactiveProperty<Sprite> m_Icon = new ReactiveProperty<Sprite>();

		public IReadOnlyReactiveProperty<string> Title => m_Title;
		private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

		public IReadOnlyReactiveProperty<string> Description => m_Description;
		private readonly ReactiveProperty<string> m_Description = new ReactiveProperty<string>();

		public ReactiveProperty<bool> IsEnabled => m_IsEnabled;
		private readonly ReactiveProperty<bool> m_IsEnabled = new ReactiveProperty<bool>();
		
		public IReadOnlyReactiveProperty<bool> IsSelect => m_IsSelect;
		private readonly ReactiveProperty<bool> m_IsSelect = new ReactiveProperty<bool>();
		
		public IReadOnlyReactiveProperty<int> Count => m_Count;
		private readonly ReactiveProperty<int> m_Count = new ReactiveProperty<int>();
		
		public IReadOnlyReactiveProperty<int> Level => m_Level;
		private readonly ReactiveProperty<int> m_Level = new ReactiveProperty<int>();
		
		public IReadOnlyReactiveProperty<SpellSchool> SpellSchool => m_SpellSchool;
		private readonly ReactiveProperty<SpellSchool> m_SpellSchool = new ReactiveProperty<SpellSchool>();

		public IReadOnlyReactiveProperty<Sprite> DecorationSprite => m_DecorationSprite;
		private readonly ReactiveProperty<Sprite> m_DecorationSprite = new ReactiveProperty<Sprite>();

		public IReadOnlyReactiveProperty<Color> DecorationColor => m_DecorationColor;
		private readonly ReactiveProperty<Color> m_DecorationColor = new ReactiveProperty<Color>();
		private readonly ReactiveProperty<bool> m_IsPossibleActive;

		public TooltipBaseTemplate TooltipTemplate => MechanicActionBarSlot.GetTooltipTemplate();

		public ActionBarSelectorItemVM(MechanicActionBarSlot mechanicActionBarSlot, ReactiveProperty<bool> isPossibleActive)
		{
			m_IsPossibleActive    = isPossibleActive;
			MechanicActionBarSlot = mechanicActionBarSlot;

			m_Icon.Value        = mechanicActionBarSlot.GetIcon();
			m_Description.Value = mechanicActionBarSlot.GetDescription();
			m_Title.Value       = mechanicActionBarSlot.GetTitle();
			m_Level.Value       = mechanicActionBarSlot.GetLevel();
			m_SpellSchool.Value = mechanicActionBarSlot.GetSpellSchool();
			m_IsEnabled.Value   = mechanicActionBarSlot.IsPossibleActive();
			
			m_Count.Value = 0;//mechanicActionBarSlot.ResourceCount;
			
			m_DecorationSprite.Value = MechanicActionBarSlot.GetDecorationSprite();
			m_DecorationColor.Value  = MechanicActionBarSlot.GetDecorationColor();
		}

		public void SetSelect(bool value)
		{
			m_IsSelect.Value = value;
			if (value)
			{
				m_IsPossibleActive.Value = MechanicActionBarSlot.IsPossibleActive();
			}
		}

		protected override void DisposeImplementation()
		{
			
		}
	}
}