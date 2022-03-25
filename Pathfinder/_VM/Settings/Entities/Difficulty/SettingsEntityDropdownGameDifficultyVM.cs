using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Settings.Difficulty;
using Kingmaker.UI.SettingsUI;

namespace Kingmaker.UI.MVVM._VM.Settings.Entities.Difficulty
{
	public class SettingsEntityDropdownGameDifficultyVM : SettingsEntityDropdownVM
	{
		public List<SettingsEntityDropdownGameDifficultyItemVM> Items;
		
		public SettingsEntityDropdownGameDifficultyVM(UISettingsEntityGameDifficulty uiSettingsEntity) : base(uiSettingsEntity)
		{
			Items = new List<SettingsEntityDropdownGameDifficultyItemVM>();
			for (int i = 0; i < BlueprintRoot.Instance.DifficultyList.Difficulties.Count; i++)
			{
				DifficultyPresetAsset difficultyPresetAsset = BlueprintRoot.Instance.DifficultyList.Difficulties[i];
				SettingsEntityDropdownGameDifficultyItemVM item = new SettingsEntityDropdownGameDifficultyItemVM(difficultyPresetAsset, i, SetTempValue, TempIndexValue);
				AddDisposable(item);
				Items.Add(item);
			}
		}
	}
}