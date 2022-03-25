using TheraBytes.BetterUi;
using UIKit;
using UnityEngine;
using UnityEngine.UI;

namespace GameClient 
{
    public class SoundTableItem : TableItem
    {
        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _disabledColor;

        [SerializeField] private TextStyleComponent _nameText;
        [SerializeField] private BetterToggle _enableToggle;
        [SerializeField] private BetterRangeSlider _volumeRangeSlider;
        [SerializeField] private BetterSlider _progressBar;
        [SerializeField] private BetterToggle _playModeToggle;
        [SerializeField] private GameObject _lockGameObject;
        
        [SerializeField] private Image _randomVolumeIconImage;
        [SerializeField] private Image _sliderHandleLowImage;
        [SerializeField] private Image _sliderHandleHighImage;
        [SerializeField] private Image _sliderFillImage;
        [SerializeField] private Image _enabledToggleOnIconImage;
        [SerializeField] private Image _enabledToggleOffIconImage;
        
        [SerializeField] private TextStyleComponent _hintVolumeText;

        private SoundTableItemData _data;
        
        private bool _isLocked;
        private bool _isEnabled;
        private bool _isLayerEnabled;

        private void OnEnable()
        {
            _playModeToggle.ValueChanged += OnPlayToggleValueChanged;
            _enableToggle.ValueChanged += OnEnableToggleValueChanged;
            _volumeRangeSlider.OnValueChanged.AddListener(OnVolumeRangeSliderValueChanged);
        }

        private void OnDisable()
        {
            _playModeToggle.ValueChanged -= OnPlayToggleValueChanged;
            _enableToggle.ValueChanged -= OnEnableToggleValueChanged;
            _volumeRangeSlider.OnValueChanged.RemoveListener(OnVolumeRangeSliderValueChanged);
        }

        public override void SetData(object data)
        {
            if (data == null)
                return;
            
            _data = (SoundTableItemData) data;
            
            _nameText.text = _data.SoundModel.SoundConfig.Name;
            _hintVolumeText.gameObject.SetActive(_data.IsFirstItem);

            UIUpdate();
        }

        private void Update()
        {
            UIUpdate();
        }

        private void UIUpdate()
        {
            _enableToggle.SetValue(_data.SoundModel.Enabled, false);
            _playModeToggle.SetValue(_data.SoundModel.PlayState == PresetsScreen.PlayState.TargetPlay, false);
            _volumeRangeSlider.SetLowValue(_data.SoundModel.SoundData.VolumeLow, false);
            _volumeRangeSlider.SetHighValue(_data.SoundModel.SoundData.VolumeHigh, false);

            var durationInvertNormal = _data.SoundModel.DurationSound > Mathf.Epsilon ? 1f - _data.SoundModel.EstimatedDurationSound / _data.SoundModel.DurationSound : 0;
            var delayNormal = _data.SoundModel.EstimatedDelay / SoundModel.Delay;
            var progressBarValue = delayNormal > 0 ? delayNormal : durationInvertNormal;
            _progressBar.SetValue(progressBarValue, false);
            _progressBar.gameObject.SetActive(progressBarValue > 0);
            
            var isLocked = _data.SoundModel.SoundConfig.IsLocked && _data.SoundModel.PresetModel.PresetData.IsLocked;
            if (_isLocked != isLocked)
            {
                _isLocked = isLocked;
                LockedEnabledUpdate();
            }
            
            var isEnabled = _data.SoundModel.Enabled && _data.SoundModel.LayerModel.Enabled;
            if (_isEnabled != isEnabled)
            {
                _isEnabled = isEnabled;
                LockedEnabledUpdate();
            }

            var isLayerEnabled = _data.SoundModel.LayerModel.Enabled;
            if (_isLayerEnabled != isLayerEnabled)
            {
                _isLayerEnabled = isLayerEnabled;
                LockedEnabledUpdate();
            }
        }

        private void LockedEnabledUpdate()
        {
            _volumeRangeSlider.interactable = !_isLocked && _isEnabled;
            _playModeToggle.gameObject.SetActive(!_isLocked);
            _lockGameObject.SetActive(_isLocked);
            _enableToggle.interactable = !_isLocked && _isLayerEnabled;
            
            var color = _isLocked || !_isEnabled ? _disabledColor : _normalColor;
            var enableToggleColor = !_enableToggle.interactable ? _disabledColor : _normalColor;
            _nameText.Color = color;
            _randomVolumeIconImage.color = color;
            _sliderHandleLowImage.color = color;
            _sliderHandleHighImage.color = color;
            _sliderFillImage.color = color;
            _enabledToggleOnIconImage.color = enableToggleColor;
            _enabledToggleOffIconImage.color = enableToggleColor;
        }

        private void OnPlayToggleValueChanged(BetterToggle toggle)
        {
            if (!toggle.isOn && _data.SoundModel.PlayState == PresetsScreen.PlayState.Normal)
            {
                toggle.SetValue(true, false);   
                _data.SoundModel.SoundTargetPlay();
            }
            else if (toggle.isOn)
                _data.SoundModel.SoundTargetPlay();
            else
                _data.SoundModel.SoundStop();
        }

        private void OnVolumeRangeSliderValueChanged(float lowValue, float highValue)
        {
            _data.SoundModel.SoundData.VolumeHigh = highValue;
            _data.SoundModel.SoundData.VolumeLow = lowValue;

            _data.Application.HasDataChanges = true;
        }
        
        private void OnEnableToggleValueChanged(BetterToggle toggle)
        {
            _data.SoundModel.Enabled = toggle.isOn;
            
            _data.Application.HasDataChanges = true;
        }
    }
}