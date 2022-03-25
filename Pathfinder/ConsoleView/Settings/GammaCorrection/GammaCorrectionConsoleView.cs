using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM._ConsoleView.Settings.Entities;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.UI.MVVM._VM.Settings.GammaCorrection;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using RewiredConsts;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.GammaCorrection
{
    public class GammaCorrectionConsoleView : ViewBase<GammaCorrectionVM>
    {
        [SerializeField]
        private TextMeshProUGUI m_HintText;

        [SerializeField]
        private SettingsEntitySliderConsoleView m_SettingsEntitySliderViewPrefab;

        private SettingsEntitySliderVM m_SliderVM;
        
        [Header("Hints")]
        [SerializeField, UsedImplicitly]
        private ConsoleHintsWidget m_ConsoleHintsWidget;

        private InputLayer m_InputLayer;
        
        private GridConsoleNavigationBehaviour m_ConsoleNavigation;
        public void Initialize()
        {
            gameObject.SetActive(false);
        }

        protected override void BindViewImplementation()
        {
            m_ConsoleNavigation = new GridConsoleNavigationBehaviour();
            m_InputLayer = m_ConsoleNavigation.GetInputLayer(new InputLayer {ContextName = "GammaCorrection"});
            m_HintText.text = UIStrings.Instance.SettingsUI.BrightnessHint;

            AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(_ => ViewModel.Close(), Action.Confirm),
                UIStrings.Instance.SettingsUI.Apply));

            AddDisposable(m_ConsoleHintsWidget.BindHint(
                m_InputLayer.AddButton(_ => ViewModel.Reset(), Action.Func01),
                UIStrings.Instance.SettingsUI.Default));
            
            AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
            
            m_SliderVM = new SettingsEntitySliderVM(ViewModel.GammaCorrection);
            m_SettingsEntitySliderViewPrefab.Bind(m_SliderVM);

            m_ConsoleNavigation.SetEntitiesHorizontal(m_SettingsEntitySliderViewPrefab);
            m_ConsoleNavigation.FocusOnFirstValidEntity();
            
            gameObject.SetActive(true);
        }

        protected override void DestroyViewImplementation()
        {
            gameObject.SetActive(false);
        }
    }
}