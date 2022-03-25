using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM._ConsoleView.ActionBar
{
	public class ActionBarSelectorItemView : VirtualListElementViewBase<ActionBarSelectorItemVM>, IConsoleNavigationEntity, IHasTooltipTemplate
	{
		[SerializeField]
		private OwlcatMultiButton m_FocusMultiButton;
		
		[SerializeField]
		private Image m_Icon;

		[SerializeField]
		private TextMeshProUGUI m_Title;
		
		[SerializeField]
		private TextMeshProUGUI m_Description;

		// [SerializeField]
		// private TextMeshProUGUI m_Count;
		
		[SerializeField]
		private TextMeshProUGUI m_SchoolName;
		
		[SerializeField]
		private TextMeshProUGUI m_Level;
		
		[SerializeField]
		private GameObject m_LevelContainer;

		// [SerializeField]
		// private Image m_DisableLayer;
		//
		// [SerializeField]
		// private Image m_SelectLayer;
		//
		// [SerializeField]
		// private Image m_Decoration;
		
		public IConsoleEntity ConsoleEntityProxy
			=> m_FocusMultiButton;

		protected override void BindViewImplementation()
		{
			AddDisposable(ViewModel.Icon.Subscribe(icon => m_Icon.sprite = icon));
			AddDisposable(ViewModel.Title.Subscribe(title => m_Title.text = title));
			AddDisposable(ViewModel.Description.Subscribe(desc => m_Description.text = desc));
			
			// if (m_Count != null)
			// 	AddDisposable(ViewModel.Count.Subscribe(count =>
			// 		{
			// 			string result = string.Empty;
			// 			if (count != 0)
			// 			{
			// 				result = count.ToString();
			// 			}
			// 			m_Count.text = result;
			// 		}));
			
			
			// AddDisposable(ViewModel.IsEnabled.Subscribe(isEnabled => m_DisableLayer.gameObject.SetActive(!isEnabled)));
			// AddDisposable(ViewModel.IsSelect.Subscribe(select => m_SelectLayer.gameObject.SetActive(select)));
			
			AddDisposable(ViewModel.SpellSchool.Subscribe(school => 
							m_SchoolName.text = school == SpellSchool.None ? 
								string.Empty : 
								LocalizedTexts.Instance.SpellSchoolNames.GetText(school)));

			AddDisposable(ViewModel.Level.Subscribe(level =>
				{
					var hasLevel = level > 0;
					m_LevelContainer.gameObject.SetActive(hasLevel);
					if (!hasLevel)
						return;

					m_Level.text = UIUtility.ArabicToRoman(level);
				}));
			
			// AddDisposable(ViewModel.DecorationSprite.Subscribe(sprite =>
			// {
			// 	if (m_Decoration == null)
			// 		return;
			// 	m_Decoration.sprite = sprite;
			// 	m_Decoration.gameObject.SetActive(sprite != null);
			// }));
			// AddDisposable(ViewModel.DecorationColor.Subscribe(color =>
			// {
			// 	if (m_Decoration == null)
			// 		return;
			// 	m_Decoration.color = color;
			// }));
		}

		protected override void DestroyViewImplementation()
		{
			
		}

		public void SetFocus(bool value)
		{
			ViewModel?.SetSelect(value);
			
			m_FocusMultiButton.SetFocus(value);
			m_FocusMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
		}

		public bool IsValid()
		{
			return true;
		}

		public TooltipBaseTemplate TooltipTemplate()
		{
			return ViewModel.TooltipTemplate;
		}
	}
}