using System.Collections;
using TheraBytes.BetterUi;
using UIKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameClient 
{
    public class PlayerScreen : UIScreen
    {
        [SerializeField] private AccountMoney _accountMoney;
        [SerializeField] private TextStyleComponent _nameText;
        [SerializeField] private BetterButton _backButton;
        [SerializeField] private BetterImage _backButtonImage;
        [SerializeField] private BetterToggle _playModeToggle;
        [SerializeField] private BetterButton _settingsButton;
        [SerializeField] private BetterScrollRect _scrollRect;
        [SerializeField] private BetterImage _image;
        
        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _animationTime;
        [SerializeField] private float _alphaDelayTime;
        [SerializeField] private float _alphaCloseTime;
        [SerializeField] private Vector2 _finishOptimalSize;
        [SerializeField] private SizeDeltaSizer _maskSizer;
        [SerializeField] private AnchorOverride _anchorOverride;
        [SerializeField] private AnimationCurve _animationCurve;

        private float _currentAnimationTime;
        private Vector2 _startSize;
        private Vector2 _startOptimalSize;
        private Vector2 _deltaOptimalSize;
        
        private bool _isScreenLandscapeOrientation;
        private RectTransform _contentRectTransform;
        private bool _isFirstEntry = true;
        
        private PlayerScreenData _data;

        private void Awake()
        {
            _contentRectTransform = _scrollRect.content.GetComponent<RectTransform>();
            
            var diameter = _maskSizer.transform.position.magnitude * 2;
            _startSize = new Vector2(diameter, diameter);
        }

        private void OnEnable()
        {
            _canvasGroup.alpha = 1f;
            
            _backButton.Click += OnBackButtonClick;
            _playModeToggle.ValueChanged += OnPlayModeToggleValueChanged;
            _settingsButton.Click += OnSettingsButtonClick;

            _maskSizer.DeltaSizer.SetSize(_maskSizer, _startSize);
            _startOptimalSize = _maskSizer.DeltaSizer.OptimizedSize;
            _deltaOptimalSize = _finishOptimalSize - _startOptimalSize;
        }

        private void OnDisable()
        {
            _backButton.Click -= OnBackButtonClick;
            _playModeToggle.ValueChanged -= OnPlayModeToggleValueChanged;
            _settingsButton.Click -= OnSettingsButtonClick;

            _isFirstEntry = true;
            
            AsyncActions.Instance.StopCoroutine(CloseRadiusAnimation());
            AsyncActions.Instance.StopCoroutine(CloseAlphaAnimation());
        }

        public override void SetData(object data)
        {
            if (data == null)
                return;
            
            _data = (PlayerScreenData) data;
            
            _accountMoney.SetData(new AccountMoney.AccountMoneyData(_data.Application));

            _nameText.text = _data.PresetModel.PresetConfig.Name;
            _image.sprite = _data.PresetModel.PresetConfig.Image;

            UpdateScreenOrientation();

            _scrollRect.horizontalNormalizedPosition = _data.PresetModel.PresetConfig.HorizontalNormalizedPosition;
            _scrollRect.verticalNormalizedPosition = _data.PresetModel.PresetConfig.VerticalNormalizedPosition;
            
            _playModeToggle.SetValue(_data.PresetModel.IsPlay, false);
        }

        private void Update()
        {
            UpdateScreenOrientation();
            
            //_data.Application.HidePopup(UIPopup.PopupType.FooterPopup, TweenComponent.MotionDirection.RightScreenWidth);
        }

        private void UpdateScreenOrientation()
        {
            var isScreenLandscapeOrientation = Screen.width > Screen.height;
            if (_isScreenLandscapeOrientation == isScreenLandscapeOrientation && !_isFirstEntry)
                return;

            _isFirstEntry = false;
            _isScreenLandscapeOrientation = isScreenLandscapeOrientation;
            
            var sprite = _image.sprite;
            var isImageLandscapeOrientation = sprite.textureRect.width > sprite.textureRect.height;
            
            var coefficient = 1f;
            if (isImageLandscapeOrientation || _isScreenLandscapeOrientation)
            {
                var ratioHeight = sprite.textureRect.height / Screen.height;
                var width = sprite.textureRect.width / ratioHeight;
                if (width < Screen.width)
                    coefficient = width / Screen.width;
                
                _contentRectTransform.sizeDelta = new Vector2(width / coefficient, Screen.height / coefficient);
            }
            else
            {
                var ratioWidth = sprite.textureRect.width / Screen.width;
                var height = sprite.textureRect.height / ratioWidth;
                if (height < Screen.height)
                    coefficient = height / Screen.height;
                    
                _contentRectTransform.sizeDelta = new Vector2(Screen.width / coefficient, height / coefficient);
            }
        }
        
        public override object GetData()
        {
            return _data;
        }

        private void OnBackButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            // _data.Application.CloseScreens();
            _data.Application.ShowScreen(ScreenType.PresetsScreen);
            //_data.Application.ShowPopup(UIPopup.PopupType.FooterPopup, new FooterPopupData(_data.Application), TweenComponent.MotionDirection.RightScreenWidth);
        }

        private void OnSettingsButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            _data.Application.ShowScreen(ScreenType.PresetEditorScreen, new PresetEditorScreenData(_data.Application, _data.PresetModel));

            AsyncActions.Instance.StartCoroutine(CloseRadiusAnimation());
            AsyncActions.Instance.StartCoroutine(CloseAlphaAnimation());
        }

        private IEnumerator CloseRadiusAnimation()
        {
            _currentAnimationTime = 0;
            
            while (_currentAnimationTime < _animationTime)
            {
                _currentAnimationTime += Time.deltaTime;
                
                var resultValue = _animationCurve.Evaluate(_currentAnimationTime);
                var newSize = _startOptimalSize + _deltaOptimalSize * resultValue;
                _maskSizer.DeltaSizer.OptimizedSize = newSize;
                _maskSizer.OnResolutionChanged();
                _anchorOverride.OnResolutionChanged();
                
                yield return null;
            }
        }

        private IEnumerator CloseAlphaAnimation()
        {
            yield return new WaitForSeconds(_alphaDelayTime);

            while (_canvasGroup.alpha > 0)
            {
                _canvasGroup.alpha -= Time.deltaTime / _alphaCloseTime;

                yield return null;
            }
        }
        
        private void OnPlayModeToggleValueChanged(BetterToggle toggle)
        {
            if (toggle.isOn)
                _data.PresetModel.Play();
            else
                _data.PresetModel.Stop();
        }
    }
}