using System.Linq;
using GameClient.Scripts;
using PolyAndCode.UI.effect;
using TheraBytes.BetterUi;
using UnityEngine;

namespace GameClient
{
    public class FooterPopup : UIPopup
    {
        [Header("Footer")]
        public BetterToggle PresetsToggle;
        public BetterToggle PlayerToggle;
        public BetterToggle TimerToggle;
        public BetterToggle SettingsToggle;
        
        // public BetterToggle CurrencyButton;

        // public TextMeshProUGUI SoftCurrencyText;

        public GameObject TimerStartedGroup;
        
        [Header("Gradients")]
        [SerializeField] private UIGradient _backgroundStrokeGradientPresets;
        [SerializeField] private UIGradient _backgroundStrokeGradientPlayer;
        [SerializeField] private UIGradient _backgroundStrokeGradientTimer;
        [SerializeField] private UIGradient _backgroundStrokeGradientSettings;
        
        private FooterPopupData _data;

        private Color _accentColor;

        private void OnEnable()
        {
            PresetsToggle.ValueChangedTrue += OnPresetsToggleValueChanged;
            PlayerToggle.ValueChangedTrue += OnPlayerToggleValueChanged;
            TimerToggle.ValueChangedTrue += OnTimerToggleValueChanged;
            SettingsToggle.ValueChangedTrue += OnSettingsToggleValueChanged;
            // CurrencyButton.Click += OnCurrencyButtonClick;
        }

        private void OnDisable()
        {
            PresetsToggle.ValueChangedTrue -= OnPresetsToggleValueChanged;
            PlayerToggle.ValueChangedTrue -= OnPlayerToggleValueChanged;
            TimerToggle.ValueChangedTrue -= OnTimerToggleValueChanged;
            SettingsToggle.ValueChangedTrue -= OnSettingsToggleValueChanged;
            // CurrencyButton.Click -= OnCurrencyButtonClick;
        }
        
        public override void SetData(object data)
        {
            _data = (FooterPopupData) data;

            UpdateData();
        }

        private void Update()
        {
            if (_data == null)
                return;
            
            // CurrencyButton.gameObject.SetActive(!_data.Application.ApplicationData.IsSubscription);

            var isOpened = _data.Application.Screens.TimerScreen.IsOpened;
            if (TimerToggle.isOn != isOpened)
                TimerToggle.SetValue(isOpened, false);
            
            isOpened = _data.Application.Screens.SettingsScreen.IsOpened;
            if (SettingsToggle.isOn != isOpened)
                SettingsToggle.SetValue(isOpened, false);
            
            UpdateData();
            UpdateColor();
        }
        
        private void UpdateData()
        {
            // SoftCurrencyText.text = _softCurrency.ToString();
            
            PresetsToggle.SetValue(_data.Application.CurrentScreenType == UIScreen.ScreenType.PresetsScreen, false);
            PlayerToggle.SetValue(_data.Application.CurrentScreenType == UIScreen.ScreenType.PlayerScreen || _data.Application.CurrentScreenType == UIScreen.ScreenType.PresetEditorScreen, false);
            TimerToggle.SetValue(_data.Application.CurrentScreenType == UIScreen.ScreenType.TimerScreen, false);
            SettingsToggle.SetValue(_data.Application.CurrentScreenType == UIScreen.ScreenType.SettingsScreen, false);
            
            TimerStartedGroup.SetActive(_data.Application.TimerModel.IsTimerStarted);
        }

        private void UpdateColor()
        {
            var newColor = _data.Application.ApplicationData.Color;
            if (_accentColor == newColor)
                return;

            _accentColor = newColor;

            var alphaKeys = _backgroundStrokeGradientPresets.gradient.alphaKeys;
            var colorKeys = new [] {new GradientColorKey(_accentColor, 0)};
            var gradient = new Gradient();
            gradient.SetKeys(colorKeys, alphaKeys);
            
            _backgroundStrokeGradientPresets.gradient = gradient;
            _backgroundStrokeGradientPlayer.gradient = gradient;
            _backgroundStrokeGradientTimer.gradient = gradient;
            _backgroundStrokeGradientSettings.gradient = gradient;
        }

        private void OnPresetsToggleValueChanged(BetterToggle toggle)
        {
            _data.Application.ShowScreen(UIScreen.ScreenType.PresetsScreen, new PresetsScreenData(_data.Application, _data.Application.PresetModels.ToArray()));
        }
        
        private void OnPlayerToggleValueChanged(BetterToggle toggle)
        {
            _data.Application.ShowScreen(UIScreen.ScreenType.PlayerScreen, new PlayerScreenData(_data.Application, _data.Application.CurrentPresetModel));
        }

        private void OnTimerToggleValueChanged(BetterToggle toggle)
        {
            _data.Application.ShowScreen(UIScreen.ScreenType.TimerScreen, new TimerScreenData(_data.Application, _data.Application.TimerModel));
        }
        
        private void OnSettingsToggleValueChanged(BetterToggle toggle)
        {
            _data.Application.ShowScreen(UIScreen.ScreenType.SettingsScreen, new SettingsScreenData(_data.Application));
        }
        
        // private void OnCurrencyButtonClick(BetterButton arg1, BaseEventData arg2)
        // {
        //     _data.Application.ShowScreen(UIScreen.ScreenType.ShopScreen, new ShopScreenData(_data.Application));
        // }
    }
}