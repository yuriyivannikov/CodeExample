using System;
using Assets.UI.Colors;
using PolyAndCode.UI.effect;
using TheraBytes.BetterUi;
using UIKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameClient
{
    public class TimerScreen : UIScreen
    {
        public BetterButton BackButton;
        public BetterButton SwipeButton;
        public BetterButton CloseButton;

        public TextStyleComponent TimerValueText;
        public TextStyleComponent AttenuationTimerValueText;
    
        public UIKitKnob KnobSlider;
        public BetterSlider AttenuationSlider;
        
        public UIGradient Gradient;
        
        public BetterButton StartButton;
        public BetterButton ResetButton;

        public BetterButton UpHourButton;
        public BetterButton DownHourButton;
        public BetterButton UpMinuteButton;
        public BetterButton DownMinuteButton;

        [SerializeField] private UIGradient _circleGradient;
        [SerializeField] private UIGradient _secondsDotsGradient;
        [SerializeField] private UIGradient _volumeSliderGradient;
        
        private TimerScreenData _data;
        private TimerModel Model => _data.TimerModel;
        
        private bool _isDrag;

        private const int SecondsPerHour = 3600;
        private const int SecondsPerMinute = 60;
        
        private void OnEnable()
        {
            UpdateColor();
            
            BackButton.Click += OnBackButtonClick;
            SwipeButton.Click += OnBackButtonClick;
            CloseButton.Click += OnBackButtonClick;
            SwipeButton.StateChange += OnBackButtonStateChange;
            KnobSlider.OnValueChanged.AddListener(OnKnobSliderValueChanged);
            AttenuationSlider.onValueChanged.AddListener(OnAttenuationSliderValueChanged);
            
            StartButton.Click += StartButtonOnClick;
            ResetButton.Click += ResetButtonOnClick;
            
            UpHourButton.Click += OnUpHourButtonClick;
            DownHourButton.Click += OnDownHourButtonClick;
            UpMinuteButton.Click += OnUpMinuteButtonClick;
            DownMinuteButton.Click += OnDownMinuteButtonClick;
            
            ColorLibrary.CallbackUpdateAllColor += OnCallbackUpdateAllColor;
        }

        private void OnDisable()
        {
            BackButton.Click -= OnBackButtonClick;
            SwipeButton.Click -= OnBackButtonClick;
            CloseButton.Click -= OnBackButtonClick;
            SwipeButton.StateChange -= OnBackButtonStateChange;
            KnobSlider.OnValueChanged.RemoveListener(OnKnobSliderValueChanged);
            AttenuationSlider.onValueChanged.RemoveListener(OnAttenuationSliderValueChanged);
            
            StartButton.Click -= StartButtonOnClick;
            ResetButton.Click -= ResetButtonOnClick;
            
            UpHourButton.Click -= OnUpHourButtonClick;
            DownHourButton.Click -= OnDownHourButtonClick;
            UpMinuteButton.Click -= OnUpMinuteButtonClick;
            DownMinuteButton.Click -= OnDownMinuteButtonClick;
            
            ColorLibrary.CallbackUpdateAllColor -= OnCallbackUpdateAllColor;

            _data.Application.TrySaveData();
        }

        public override void SetData(object data)
        {
            if (data == null || _data == data)
                return;

            _data = (TimerScreenData) data;
            AttenuationSlider.SetValue(_data.Application.ApplicationData.AttenuationValue, false);

            UpdateTimer();
            UpdateAttenuationTimerText();
        }
        
        public override object GetData()
        {
            return _data;
        }

        private void Update()
        {
            if (_isDrag != KnobSlider.IsDrag)
            {
                _isDrag = KnobSlider.IsDrag;
                if (!_isDrag)
                {
                    OnKnobSliderValueChanged(KnobSlider.CurrentValue, true);
                }
            }
            
            if (_isDrag || !Model.IsTimerStarted)
                return;

            UpdateTimer();
        }

        private void UpdateColor()
        {
            var accentColor = ColorLibrary.GetColor(ColorLibrary.ColorMainAccent);

            SetColorGradient(_circleGradient, accentColor);
            SetColorGradient(_secondsDotsGradient, accentColor);
            SetColorGradient(_volumeSliderGradient, accentColor);
        }

        private static void SetColorGradient(UIGradient uiGradient, Color color)
        {
            var alphaKeys = uiGradient.gradient.alphaKeys;
            var colorKeys = new [] {new GradientColorKey(color, 0)};
            var gradient = new Gradient();
            gradient.SetKeys(colorKeys, alphaKeys);
            uiGradient.gradient = gradient;
        }

        private void OnCallbackUpdateAllColor()
        {
            UpdateColor();
        }
        
        private void OnKnobSliderValueChanged(float value, bool isManualChange)
        {
            Model.LeftTime = KnobSlider.CurrentValue * SecondsPerHour;
            UpdateTimerText();
        }

        private void OnAttenuationSliderValueChanged(float value)
        {
            UpdateAttenuationTimerText();
            
            var intValue = (int) value;
            if (_data.Application.ApplicationData.AttenuationValue == intValue)
                return;
            
            _data.Application.HasDataChanges = true;
            _data.Application.ApplicationData.AttenuationValue = intValue;
        }

        private void UpdateAttenuationTimerText()
        {
            AttenuationTimerValueText.SetLocalizationArgs(AttenuationSlider.value.ToString());
        }

        private void UpdateTimer()
        {
            KnobSlider.SetValue(Model.LeftTime / SecondsPerHour, false);
            
            var mod = Model.LeftTime % 60;
            const int angle = 6;
            const int gradientBeginAngle = 269;
            var gradientAngle = (mod * angle + gradientBeginAngle) % 360;
            
            Gradient.angle = gradientAngle;
            
            UpdateTimerText();
        }
        
        private void UpdateTimerText()
        {
            var seconds = (int) Model.LeftTime;
            var date = new DateTime().AddSeconds(seconds);
            var time = date.ToString("H mm");
            var colorTime = $"<mspace=0.55em><color=#13161B>0</color>{time}";
            TimerValueText.text = colorTime;
        }

        private void OnBackButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            _data.Application.HideScreen(ScreenType.TimerScreen);
        }
        
        private void OnBackButtonStateChange(BetterButton arg1, UIKitSelectable.SelectionState state)
        {
            if (state == UIKitSelectable.SelectionState.Pressed)
                _data.Application.HideScreen(ScreenType.TimerScreen);
        }
        
        private void ResetButtonOnClick(BetterButton arg1, BaseEventData arg2)
        {
            Model.TimerReset();
            
            UpdateTimer();
        }

        private void StartButtonOnClick(BetterButton arg1, BaseEventData arg2)
        {
            Model.TimerStart();
        }
        
        private void OnUpHourButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            Model.LeftTime += SecondsPerHour;
            
            UpdateTimer();
        }
        
        private void OnDownHourButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            if (Model.LeftTime >= SecondsPerHour)
                Model.LeftTime -= SecondsPerHour;
            else
                Model.TimerEnd();
            
            UpdateTimer();
        }
        
        private void OnUpMinuteButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            Model.LeftTime += SecondsPerMinute;
            
            UpdateTimer();
        }

        private void OnDownMinuteButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            if (Model.LeftTime >= SecondsPerMinute)
                Model.LeftTime -= SecondsPerMinute;
            else
                Model.TimerEnd();
            
            UpdateTimer();
        }
    }
}