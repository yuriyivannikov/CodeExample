using System;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.Settings.Entities.Difficulty
{
	public class SettingsEntityDropdownGameDifficultyItemVM : BaseDisposable, IViewModel
	{
		public readonly Sprite Icon;
		public readonly string Title;
		public readonly string Description;
		
		public bool IsCustom;

		private bool m_IsVeryHard;
		private bool NeedWarning =>
			m_IsVeryHard && 
			(SettingsRoot.Difficulty.GameDifficulty != GameDifficultyOption.Hard || 
			 SettingsRoot.Difficulty.GameDifficulty != GameDifficultyOption.Unfair);

		private readonly int m_Index;
		private readonly Action<int> m_SetSelected;

		public readonly ReadOnlyReactiveProperty<bool> IsSelected;

		public SettingsEntityDropdownGameDifficultyItemVM(
			DifficultyPresetAsset difficulty, int index, Action<int> setSelected, IObservable<int> selectedIndex)
		{
			m_Index = index;
			m_SetSelected = setSelected;
			AddDisposable(IsSelected = selectedIndex.Select(i => i == m_Index).ToReadOnlyReactiveProperty());
			
			Title = difficulty.LocalizedName;
			Description = difficulty.LocalizedDescription;
			Icon = difficulty.Icon;

			IsCustom = difficulty.IsCustomMode;

			m_IsVeryHard = difficulty.Preset.GameDifficulty == GameDifficultyOption.Hard ||
			               difficulty.Preset.GameDifficulty == GameDifficultyOption.Unfair;
		}

		public void SetSelected()
		{
			m_SetSelected?.Invoke(m_Index);
		}

		protected override void DisposeImplementation()
		{
		}
	}
}