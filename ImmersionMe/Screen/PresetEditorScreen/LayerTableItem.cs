using Assets.UI.Colors;
using TheraBytes.BetterUi;
using UIKit;
using UnityEngine;
using UnityEngine.UI;

namespace GameClient 
{
    public class LayerTableItem : TableItem
    {
        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _disabledColor;
        
        [SerializeField] private TextStyleComponent _nameText;
        [SerializeField] private GameObject _layautAnimationGroup;
        [SerializeField] private BetterSlider _volumeSlider;
        [SerializeField] private BetterSlider _delayProgressBar;
        [SerializeField] private TextStyleComponent _delayProgressTimerText;
        [SerializeField] private BetterRangeSlider _delayRangeSlider;
        [SerializeField] private BetterToggle _playModeToggle;
        [SerializeField] private BetterToggle _enableToggle;
        [SerializeField] private GameObject _lockGameObject;
        [SerializeField] private BetterToggle _openToggle;
        
        [SerializeField] private SimpleTable _soundsTable;
        
        [SerializeField] private Image _volumeIconImage;
        [SerializeField] private Image _delayIconImage;
        [SerializeField] private Image _volumeSliderFillImage;
        [SerializeField] private Image _volumeSliderHandleImage;
        [SerializeField] private Image _delayRangeSliderHandleLowImage;
        [SerializeField] private Image _delayRangeSliderHandleHighImage;
        [SerializeField] private Image _delayRangeSliderFillImage;
        
        [Header("Hint")]
        [SerializeField] private TextStyleComponent _hintVolumeText;
        [SerializeField] private TextStyleComponent _hintDelayText;
        
        private LayerTableItemData _data;
        private LayerModel Model => _data.LayerModel;
        
        private Color _accentColor;
        private bool _isLocked;
        private bool _isEnabled;

        private void OnEnable()
        {
            _playModeToggle.ValueChanged += OnToggleValueChanged;
            _enableToggle.ValueChanged += OnEnableToggleValueChanged;
            _volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);
            _delayRangeSlider.OnValueChanged.AddListener(OnDelayRangeSliderValueChanged);
            _openToggle.ValueChanged += OnOpenToggleValueChanged;

            ColorLibrary.CallbackUpdateAllColor += OnCallbackUpdateAllColor;
        }

        private void OnDisable()
        {
            _soundsTable.Clear();
            
            _playModeToggle.ValueChanged -= OnToggleValueChanged;
            _enableToggle.ValueChanged -= OnEnableToggleValueChanged;
            _volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderValueChanged);
            _delayRangeSlider.OnValueChanged.RemoveListener(OnDelayRangeSliderValueChanged);
            _openToggle.ValueChanged -= OnOpenToggleValueChanged;
            
            ColorLibrary.CallbackUpdateAllColor -= OnCallbackUpdateAllColor;
        }

        public override void SetData(object data)
        {
            if (data == null)
                return;

            _data = (LayerTableItemData) data;
            
            _nameText.text = Model.LayerConfig.Name;

            _hintVolumeText.gameObject.SetActive(_data.IsFirstItem);
            _hintDelayText.gameObject.SetActive(_data.IsFirstItem);

            if (_data.LayerModel.PresetModel.PresetData.IsLocked)
            {
                _openToggle.isOn = true;
                FillSoundTable();
            }
            
            _soundsTable.gameObject.SetActive(_openToggle.isOn);

            UIUpdate();
            UpdateColor();
        }
        
        private void Update()
        {
            UIUpdate();
        }
        
        private void UIUpdate()
        {
            _enableToggle.SetValue(_data.LayerModel.Enabled, false);
            _playModeToggle.SetValue(_data.LayerModel.PlayState == PresetsScreen.PlayState.TargetPlay, false);
            _volumeSlider.SetValue(_data.LayerModel.LayerData.Volume, false);
            _delayRangeSlider.SetLowValue(Model.LayerData.DelayLow, false);
            _delayRangeSlider.SetHighValue(Model.LayerData.DelayHigh, false);

            var progressBarValue = _data.LayerModel.Delay > Mathf.Epsilon ? _data.LayerModel.EstimatedDelay / _data.LayerModel.Delay : 0;
            var estimatedIntDelay = (int) _data.LayerModel.EstimatedDelay;
            _delayProgressBar.SetValue(progressBarValue, false);
            _delayProgressTimerText.text = estimatedIntDelay.ToString();

            var isShowProgressDelay = progressBarValue > 0;
            _delayProgressBar.gameObject.SetActive(isShowProgressDelay);
            _delayProgressTimerText.gameObject.SetActive(isShowProgressDelay);
            
            var isLocked = Model.PresetModel.PresetData.IsLocked;
            if (_isLocked != isLocked)
            {
                _isLocked = isLocked;
                LockedEnabledUpdate();
            }
            
            var isEnabled = Model.Enabled;
            if (_isEnabled != isEnabled)
            {
                _isEnabled = isEnabled;
                LockedEnabledUpdate();
            }
            
            _layautAnimationGroup.SetActive(isEnabled && Model.IsPlay);
        }

        private void LockedEnabledUpdate()
        {
            _volumeSlider.interactable = !_isLocked && _isEnabled;
            _delayRangeSlider.interactable = !_isLocked && _isEnabled;
            _playModeToggle.gameObject.SetActive(!_isLocked);
            _lockGameObject.SetActive(_isLocked);
            _enableToggle.gameObject.SetActive(!_isLocked);
        }

        private void UpdateColor()
        {
            var isLocked = Model.PresetModel.PresetData.IsLocked;
            var isEnabled = Model.Enabled;

            _accentColor = ColorLibrary.GetColor(ColorLibrary.ColorMainAccent);
            
            var accentColor = isLocked || !isEnabled ? _disabledColor : _accentColor;
            var normalColor = isLocked || !isEnabled ? _disabledColor : _normalColor;
            
            //_nameText.Color = accentColor;
            _volumeSliderFillImage.color = accentColor;
            _volumeSliderHandleImage.color = accentColor;
            _delayRangeSliderFillImage.color = accentColor;
            
            _delayRangeSliderHandleLowImage.color = normalColor;
            _delayRangeSliderHandleHighImage.color = normalColor;
            _volumeIconImage.color = normalColor;
            _delayIconImage.color = normalColor;
        }
        
        private void OnCallbackUpdateAllColor()
        {
            UpdateColor();
        }

        private void OnToggleValueChanged(BetterToggle toggle)
        {
            if (!toggle.isOn && _data.LayerModel.PlayState == PresetsScreen.PlayState.Normal)
            {
                toggle.SetValue(true, false);
                _data.LayerModel.LayerTargetPlay();
            }
            else if (toggle.isOn)
                _data.LayerModel.LayerTargetPlay();
            else
                _data.LayerModel.LayerStop();
        }
        
        private void OnVolumeSliderValueChanged(float value)
        {
            _data.LayerModel.SetVolume(_volumeSlider.value);
            
            _data.Application.HasDataChanges = true;
        }
        
        private void OnEnableToggleValueChanged(BetterToggle toggle)
        {
            _data.LayerModel.Enabled = toggle.isOn;
            
            _data.Application.HasDataChanges = true;
        }
        
        private void OnDelayRangeSliderValueChanged(float arg0, float arg1)
        {
            _data.LayerModel.LayerData.DelayHigh = _delayRangeSlider.HighValue;
            _data.LayerModel.LayerData.DelayLow = _delayRangeSlider.LowValue;

            _data.Application.HasDataChanges = true;
        }
        
        private void OnOpenToggleValueChanged(BetterToggle toggle)
        {
            _soundsTable.gameObject.SetActive(toggle.isOn);

            if (toggle.isOn)
                FillSoundTable();
        }

        private void FillSoundTable()
        {
            if (_soundsTable.Count != 0)
                return;

            var isFirstItem = true;
            foreach (var soundModel in Model.SoundModels)
            {
                var soundItemData = new SoundTableItemData(_data.Application, soundModel, isFirstItem);
                _soundsTable.AddItem(soundItemData);
                isFirstItem = false;
            }
        }
    }
}