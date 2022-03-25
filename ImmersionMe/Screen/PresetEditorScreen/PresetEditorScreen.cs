using System.Collections;
using TheraBytes.BetterUi;
using UIKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameClient 
{
    public class PresetEditorScreen : UIScreen
    {
        [SerializeField] private AccountMoney _accountMoney;
        
        [SerializeField] private TextStyleComponent _nameText;
        [SerializeField] private BetterImage _presetImage;
        [SerializeField] private BetterScrollRect _scrollView;
        [SerializeField] private BetterButton _backButton;
        [SerializeField] private BetterToggle _playModeToggle;
        [SerializeField] private BetterButton _lockButton;
        [SerializeField] private BetterButton _resetButton;

        [Header("Animation")]
        [SerializeField] private float _delayAnimationTime;
        [SerializeField] private float _animationTime;
        [SerializeField] private AnimationCurve _animationCurve;
        [SerializeField] private Vector2 _startOptimalSize;
        [SerializeField] private Vector2 _finishOptimalSize;
        [SerializeField] private SizeDeltaSizer _presetSizer;
        [SerializeField] private SizeDeltaSizer _presetContentSizer;
        [SerializeField] private BetterContentSizeFitter _contentSizeFitter;
        
        [Header("Table")]
        public SimpleTable LayersTable;

        private PresetEditorScreenData _data;
        private float _currentAnimationTime;
        private Vector2 _deltaSize;

        private void Start()
        {
            _deltaSize = _finishOptimalSize - _startOptimalSize;
        }

        private void OnEnable()
        {
            _backButton.Click += OnBackButtonClick;
            _playModeToggle.ValueChanged += OnValueChanged;
            _lockButton.Click += OnLockButtonClick;
            _resetButton.Click += OnResetButtonClick;

            _scrollView.ResetToStartPosition();
            AsyncActions.Instance.StartCoroutine(OpenAnimation());
        }

        private void OnDisable()
        {
            _backButton.Click -= OnBackButtonClick;
            _playModeToggle.ValueChanged -= OnValueChanged;
            _lockButton.Click -= OnLockButtonClick;
            _resetButton.Click -= OnResetButtonClick;
            
            _data.Application.TrySaveData();
            
            AsyncActions.Instance.StopCoroutine(OpenAnimation());
        }

        private IEnumerator OpenAnimation()
        {
            _currentAnimationTime = 0;
            
            if (_presetSizer != null)
                _presetSizer.DeltaSizer.OptimizedSize = _startOptimalSize;
            else
            {
                Debug.LogError("Error: _presetSizer not found");
                yield break;
            }
            
            yield return new WaitForSeconds(_delayAnimationTime);

            while (_currentAnimationTime < _animationTime)
            {
                _currentAnimationTime += Time.deltaTime;
                
                var resultValue = _animationCurve.Evaluate(_currentAnimationTime);
                var newSize = _startOptimalSize + _deltaSize * resultValue;
                _presetSizer.DeltaSizer.OptimizedSize = newSize;
                //_presetContentSizer.DeltaSizer.OptimizedSize = newSize;
                _presetSizer.OnResolutionChanged();
                //_presetContentSizer.OnResolutionChanged();
                _contentSizeFitter.OnResolutionChanged();

                yield return null;
            }
        }

        public override void SetData(object data)
        {
            if (data == null)
                return;
            
            _data = (PresetEditorScreenData) data;
            
            _accountMoney.SetData(new AccountMoney.AccountMoneyData(_data.Application));
            
            _nameText.text = _data.PresetModel.PresetConfig.Name;
            _presetImage.sprite = _data.PresetModel.PresetConfig.PreviewImage;
            
            LayersTable.Clear();
            
            var isFirstItem = true;
            for (var i = 0; i < _data.PresetModel.PresetConfig.Layers.Length; i++)
            {
                var layerItemData = new LayerTableItemData(_data.Application, _data.PresetModel.LayerModels[i], isFirstItem);
                LayersTable.AddItem(layerItemData);
                isFirstItem = false;
            }
        }

        private void Update()
        {
            _playModeToggle.SetValue(_data.PresetModel.PlayState == PresetsScreen.PlayState.TargetPlay, false);
            _playModeToggle.gameObject.SetActive(!_data.PresetModel.PresetData.IsLocked);
            _lockButton.gameObject.SetActive(_data.PresetModel.PresetData.IsLocked);
        }
        
        public override object GetData()
        {
            return _data;
        }

        private void OnBackButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            var screenData = new PlayerScreenData(_data.Application, _data.PresetModel);
            _data.Application.ShowScreen(ScreenType.PlayerScreen, screenData);
        }
        
        private void OnValueChanged(BetterToggle toggle)
        {
            if (toggle.isOn)
                _data.PresetModel.Play();
            else
                _data.PresetModel.Stop();
        }
        
        private void OnResetButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            _data.Application.ShowModal(UIModal.ModalType.ResetPresetData, new TrueFalseModalData(_data.Application, ModalResult));
        }

        private void ModalResult(bool value)
        {
            if (!value)
                return;
            
            _data.PresetModel.ResetData();
        }
        
        private void OnLockButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            //_data.Application.ShowScreen(ScreenType.PresetsScreen);
        }
    }
}