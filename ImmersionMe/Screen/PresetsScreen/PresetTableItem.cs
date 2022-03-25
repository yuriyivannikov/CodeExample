using PolyAndCode.UI.effect;
using TheraBytes.BetterUi;
using UIKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameClient 
{
    public class PresetTableItem : TableItem
    {
        [SerializeField] private Image _presetNormalImage;
        [SerializeField] private Image _presetActiveImage;
        
        [SerializeField] private BetterToggle _playingNonInteractiveToggle;
        [SerializeField] private BetterButton _lockButton;
        [SerializeField] private BetterToggle _playToggle;
        [SerializeField] private BetterButton _playerButton;
        [SerializeField] private BetterButton _presetEditorButton;
        
        [SerializeField] private TextStyleComponent _availableNameText;
        [SerializeField] private TextStyleComponent _unAvailableNameText;
        [SerializeField] private TextStyleComponent _playingNameText;
        
        // [SerializeField] private Image _availableIconImage;
        // [SerializeField] private Image _unavailableIconImage;
        [SerializeField] private Image _playingBorderImage;

        [SerializeField] private GameObject _moneyGroup;
        [SerializeField] private TextStyleComponent _priceText;
        
        // [SerializeField] private UIGradient _gradientFirst;
        // [SerializeField] private UIGradient _gradientSecond;

        private PresetTableItemData _data;

        private bool _isPlaying;
        private bool _isLocked;

        private void OnEnable()
        {
            _lockButton.Click += OnLockButtonClick;
            _playToggle.ValueChanged += OnPlayToggleValueChanged;
            _playerButton.Click += OnPlayerButtonClick;
            _presetEditorButton.Click += OnPresetEditorButtonClick;
        }

        private void OnDisable()
        {
            _lockButton.Click -= OnLockButtonClick;
            _playToggle.ValueChanged -= OnPlayToggleValueChanged;
            _playerButton.Click -= OnPlayerButtonClick;
            _presetEditorButton.Click -= OnPresetEditorButtonClick;
        }

        public override void SetData(object data)
        {
            if (data == null || _data != null)
                return;
            
            _data = (PresetTableItemData) data;
            
            _availableNameText.text = _data.PresetModel.PresetConfig.Name;
            _unAvailableNameText.text = _data.PresetModel.PresetConfig.Name;
            _playingNameText.text = _data.PresetModel.PresetConfig.Name;
            _presetNormalImage.sprite = _data.PresetModel.PresetConfig.PreviewImage;
            _presetActiveImage.sprite = _data.PresetModel.PresetConfig.PreviewImage;
            _priceText.text =_data.PresetModel.PresetConfig.Price.ToString();

            PlayingObjectUpdate();
            LockedObjectUpdate();
            LockedPlayingObjectUpdate();
        }

        private void Update()
        {
            if (_isPlaying != _data.PresetModel.IsPlay)
            {
                _isPlaying = _data.PresetModel.IsPlay;

                PlayingObjectUpdate();
                LockedPlayingObjectUpdate();
            }

            if (_isLocked != _data.PresetModel.PresetData.IsLocked)
            {
                _isLocked = _data.PresetModel.PresetData.IsLocked;

                LockedObjectUpdate();
                LockedPlayingObjectUpdate();
            }
            
            /*
            if (_isPlaying)
            {
                const int coefficientSpeed = 60;
                _gradientFirst.angle += Time.deltaTime * coefficientSpeed;
                _gradientSecond.angle += Time.deltaTime * coefficientSpeed;
                
                _gradientFirst.UpdateMaterial(false);
                _gradientSecond.UpdateMaterial(false);
            }
            */
        }

        private void LockedObjectUpdate()
        {
            _playerButton.gameObject.SetActive(!_isLocked);
            _unAvailableNameText.gameObject.SetActive(_isLocked);
            _playToggle.gameObject.SetActive(!_isLocked);
            _lockButton.gameObject.SetActive(_isLocked);
            // _availableIconImage.gameObject.SetActive(!_isLocked);
            // _unavailableIconImage.gameObject.SetActive(_isLocked);
            _moneyGroup.SetActive(_isLocked);
            
        }
        
        private void PlayingObjectUpdate()
        {
            _playingNonInteractiveToggle.SetValue(_isPlaying, false);
            _playToggle.SetValue(_isPlaying, false);
        }

        private void LockedPlayingObjectUpdate()
        {
            _availableNameText.gameObject.SetActive(!_isLocked && !_isPlaying);
            _playingNameText.gameObject.SetActive(!_isLocked && _isPlaying);
            _playingBorderImage.gameObject.SetActive(!_isLocked && _isPlaying);
        }
        
        private void OnPresetEditorButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            var screenData = new PresetEditorScreenData(_data.Application, _data.PresetModel);
            _data.Application.ShowScreen(UIScreen.ScreenType.PresetEditorScreen, screenData);
        }
        
        private void OnPlayerButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            var screenData = new PlayerScreenData(_data.Application, _data.PresetModel);
            _data.Application.ShowScreen(UIScreen.ScreenType.PlayerScreen, screenData);
        }

        private void OnLockButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            if (_data.Application.ApplicationData.SoftCurrency >= _data.PresetModel.PresetConfig.Price)
                _data.Application.ShowModal(UIModal.ModalType.BuyPreset, new BuyPresetModalData(_data.Application, _data.PresetModel.PresetConfig.Price, ModalResult));
            else
                _data.Application.ShowModal(UIModal.ModalType.NoMoney, new NoMoneyModalData(_data.Application, _data.PresetModel.PresetConfig.Price));
        }
        
        private void ModalResult(bool value)
        {
            if (!value)
                return;
            
            _data.PresetModel.PresetData.IsLocked = false;
            _data.Application.ShopManager.SpendSoftCurrency(_data.PresetModel.PresetConfig.Price);
        }

        private void OnPlayToggleValueChanged(BetterToggle toggle)
        {
            if (toggle.isOn)
                _data.PresetModel.Play();
            else
                _data.PresetModel.Stop();
        }
    }
}