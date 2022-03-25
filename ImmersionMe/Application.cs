using System;
using System.Collections;
using System.Collections.Generic;
using Assets.UI.Colors;
using GameClient.Scripts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TheraBytes.BetterUi;
using UIKit;
using UnityEngine;
using UnityEngine.UI;

namespace GameClient
{
    public class ApplicationData
    {
        [JsonProperty(PropertyName = "soft_currency")]
        public int SoftCurrency;
        
        [JsonProperty(PropertyName = "is_subscription")]
        public bool IsSubscription;//todo сделать датой
        
        [JsonProperty(PropertyName = "language")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SystemLanguage Language;
        
        [JsonProperty(PropertyName = "current_preset_model_name")]
        public string CurrentPresetModelName;

        [JsonProperty(PropertyName = "layers")]
        public Dictionary<string, PresetModelData> Presets;
        
        [JsonProperty(PropertyName = "attenuation_value")]
        public int AttenuationValue;
        
        [JsonProperty(PropertyName = "color_r")]
        public float ColorR;
        
        [JsonProperty(PropertyName = "color_g")]
        public float ColorG;
            
        [JsonProperty(PropertyName = "color_b")]
        public float ColorB;
         
        [JsonProperty(PropertyName = "color_a")]
        public float ColorA;

        [JsonIgnore]
        private Color _accentColor = Color.clear;
        [JsonIgnore]
        public Color Color
        {
            get
            {
                if (_accentColor == Color.clear ||
                    !Mathf.Approximately(_accentColor.r, ColorR) || 
                    !Mathf.Approximately(_accentColor.g, ColorG) || 
                    !Mathf.Approximately(_accentColor.b, ColorB) || 
                    !Mathf.Approximately(_accentColor.a, ColorA))
                    _accentColor = new Color(ColorR, ColorG, ColorB, ColorA);
                
                return _accentColor;
            }
        }
    }
    
    public class Application : MonoBehaviour
    {
        public Camera Camera;
        
        [SerializeField] private AK.Wwise.Bank _commonSoundBank;
        
        public Screens Screens;
        public Popups Popups;
        public Modals Modals;

        public GameObject SoundGameObject;

        [SerializeField] private CanvasScaler _canvasScaler;
        [SerializeField] private AnimationCurve _screenMatchCurve;
        
        public Dictionary<UIScreen.ScreenType, UIScreen> DictionaryScreens;
        public Dictionary<UIPopup.PopupType, UIPopup> DictionaryPopups;
        public Dictionary<UIModal.ModalType, UIModal> DictionaryModals;
        
        public ApplicationData ApplicationData { get; private set; }
        public List<PresetModel> PresetModels { get; private set; }
        public TimerModel TimerModel { get; private set; }
        public ShopManager ShopManager { get; private set; }
        public bool HasDataChanges { get; set; }
        
        public UIScreen.ScreenType CurrentScreenType { get; private set; }
        private UIScreen.ScreenType _previousScreenType;
        
        private PresetModel _currentPresetModel;
        public PresetModel CurrentPresetModel
        {
            get => _currentPresetModel;
            private set
            {
                _currentPresetModel = value;
                ApplicationData.CurrentPresetModelName = _currentPresetModel.PresetConfig.Name;
                HasDataChanges = true;
            } 
        }

        private TweenRunner<FloatTween> _tweenRunner;
        
        private const string ApplicationDataKey = "ApplicationData";
        private const SystemLanguage DefaultLanguage = SystemLanguage.English;

        private float _screenWidth;
        private float _screenHeight;

        private void Awake()
        {
            var config = GameConfig.Instance.Data;
            
            ApplicationData = LoadData();
            if (ApplicationData == null)
            {
                ApplicationData = new ApplicationData
                {
                    SoftCurrency = 0,
                    IsSubscription = false,
                    Language = DefaultLanguage,
                    Presets = new Dictionary<string, PresetModelData>(),
                    AttenuationValue = 45,
                    ColorR = 0f,
                    ColorG = 0.96f,
                    ColorB = 1f,
                    ColorA = 1f
                };
            }

            SetAccentColor(ApplicationData.Color);

            LocalizationManager.CurrentLanguage = ApplicationData.Language;
            ShopManager = new ShopManager(this);
            
            _commonSoundBank.Load();
            
            DictionaryScreens = new Dictionary<UIScreen.ScreenType, UIScreen>
            {
                {UIScreen.ScreenType.PresetsScreen, Screens.PresetsScreen},
                {UIScreen.ScreenType.PresetEditorScreen, Screens.PresetEditorScreen},
                {UIScreen.ScreenType.PlayerScreen, Screens.PlayerScreen},
                {UIScreen.ScreenType.TimerScreen, Screens.TimerScreen},
                {UIScreen.ScreenType.SettingsScreen, Screens.SettingsScreen},
                {UIScreen.ScreenType.ShopScreen, Screens.ShopScreen}
            };
            
            DictionaryPopups = new Dictionary<UIPopup.PopupType, UIPopup>
            {
                {UIPopup.PopupType.FooterPopup, Popups.FooterPopup}
            };

            DictionaryModals = new Dictionary<UIModal.ModalType, UIModal>
            {
                {UIModal.ModalType.BuyPreset, Modals.BuyPresetModal},
                {UIModal.ModalType.NoMoney, Modals.NoMoneyModal},
                {UIModal.ModalType.ResetPresetData, Modals.ResetPresetDataModal}
            };

            PresetModels = new List<PresetModel>();
            foreach (var presetConfig in config)
            {
                if (!presetConfig.IsVisible)
                    continue;
                
                if (!ApplicationData.Presets.TryGetValue(presetConfig.Name, out var presetData))
                {
                    presetData = new PresetModelData
                    {
                        Layers = new Dictionary<string, LayerModelData>(),
                        IsLocked = presetConfig.Price > 0
                    };
                    
                    ApplicationData.Presets.Add(presetConfig.Name, presetData);
                }
                
                var presetModel = new PresetModel(this, SoundGameObject, presetConfig, presetData);
                PresetModels.Add(presetModel);

                if (string.IsNullOrEmpty(ApplicationData.CurrentPresetModelName) || ApplicationData.CurrentPresetModelName == presetConfig.Name)
                    CurrentPresetModel = presetModel;
            }

            TimerModel = new TimerModel(this);
            
            ShowPopup(UIPopup.PopupType.FooterPopup, new FooterPopupData(this), TweenComponent.MotionDirection.UpHeight);
            ShowScreen(UIScreen.ScreenType.PresetsScreen, new PresetsScreenData(this, PresetModels.ToArray()));
            
            _tweenRunner = new TweenRunner<FloatTween>();
            _tweenRunner.Init(this);
        }

        private void OnEnable()
        {
            ShopManager.OnSubscriptionChanged += OnSubscriptionChanged;
        }

        private void OnDisable()
        {
            ShopManager.OnSubscriptionChanged -= OnSubscriptionChanged;
        }

        private void OnSubscriptionChanged(bool isPremium)
        {
            foreach (var presetModel in PresetModels)
            {
                presetModel.PresetData.IsLocked = presetModel.PresetConfig.Price != 0 && !ApplicationData.IsSubscription;
            }
        }

        private void Update()
        {
            if (PresetModels == null)
                return;
                
            foreach (var presetModel in PresetModels)
            {
                presetModel.Update();
            }
            
            TimerModel.Update();
            
            LocalizationUpdate();
            ScreenSizeUpdate();
        }

        private void ScreenSizeUpdate()
        {
            if (Mathf.Approximately(_screenWidth, Screen.width) && Mathf.Approximately(_screenHeight,Screen.height))
                return;

            _screenWidth = Screen.width;
            _screenHeight = Screen.height;

            if (Screen.height == 0)
                return;
            
            var division = (float)Screen.width / (float)Screen.height;
            _canvasScaler.matchWidthOrHeight = _screenMatchCurve.Evaluate(division);
        }
        
        public void SetupScreenHeight(LayoutElement layoutElement)
        {
            StartCoroutine(ScreenHeightCoroutine(layoutElement));
        }

        private IEnumerator ScreenHeightCoroutine(LayoutElement layoutElement)
        {
            yield return new WaitFrames(1);
            
            var rootRectTransform = (RectTransform) _canvasScaler.transform;
            layoutElement.preferredHeight = rootRectTransform.rect.height;
        }

        #region Json
        
        public ApplicationData LoadData()
        {
            if (!PlayerPrefs.HasKey(ApplicationDataKey)) 
                return null;
            
            try
            {
                var json = PlayerPrefs.GetString(ApplicationDataKey);
                var applicationData = JsonConvert.DeserializeObject<ApplicationData>(json);
                return applicationData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public void TrySaveData()
        {
            if (!HasDataChanges)
                return;

            SaveData();
        }
        
        public void SaveData()
        {
            HasDataChanges = false;
            
            var json = JsonConvert.SerializeObject(ApplicationData);
            PlayerPrefs.SetString(ApplicationDataKey, json);
            
            PlayerPrefs.Save();
            
            Debug.LogError("SAVE");
        }

        #endregion
        
        #region UI
        
        public void ShowScreen(UIScreen.ScreenType screenType, object data = null)
        {
            if (CurrentScreenType == screenType)
                return;
            
            if (CurrentScreenType != UIScreen.ScreenType.TimerScreen && 
                CurrentScreenType != UIScreen.ScreenType.SettingsScreen && 
                CurrentScreenType != UIScreen.ScreenType.ShopScreen)
                _previousScreenType = CurrentScreenType;

            if (screenType == UIScreen.ScreenType.TimerScreen)
            {
                Screens.SettingsScreen.Close();
                Screens.ShopScreen.Close();
            }
            else if (screenType == UIScreen.ScreenType.SettingsScreen)
            {
                Screens.TimerScreen.Close();
                Screens.ShopScreen.Close();
            }
            else if (screenType == UIScreen.ScreenType.ShopScreen)
            {
                Screens.SettingsScreen.Close();
                Screens.TimerScreen.Close();
            }
            else
            {
                foreach (var screen in DictionaryScreens.Values)
                {
                    screen.Close();
                }
            }
            
            CurrentScreenType = screenType;
            
            var currentScreen = DictionaryScreens[screenType];
            if (currentScreen.IsOpened)
            {
                //currentScreen.Close();
                return;
            }

            currentScreen.gameObject.SetActive(true);
            currentScreen.SetData(data);
            currentScreen.Open();
        }

        public void HideScreen(UIScreen.ScreenType screenType)
        {
            var currentScreen = DictionaryScreens[screenType];
            currentScreen.Close();
            
            CurrentScreenType = _previousScreenType;
        }

        public UIPopup ShowPopup(UIPopup.PopupType popupType, object data, TweenComponent.MotionDirection direction)
        {
            var popup = DictionaryPopups[popupType];
            
            popup.gameObject.SetActive(true);
            popup.Tween.Direction = direction;
            popup.Tween.OpenClose();
            popup.SetData(data);

            return popup;
        }
        
        public void HidePopup(UIPopup.PopupType popupType, TweenComponent.MotionDirection direction)
        {
            var popup = DictionaryPopups[popupType];
            popup.Tween.Direction = direction;
            popup.Tween.OpenClose();
        }

        public UIModal ShowModal(UIModal.ModalType modalType, object data)
        {
            var modal = DictionaryModals[modalType];
            
            modal.gameObject.SetActive(true);
            modal.Tween.OpenClose();
            modal.SetData(data);

            return modal;
        }
        
        public void HideModal(UIModal.ModalType modalType)
        {
            var modal = DictionaryModals[modalType];
            
            modal.gameObject.SetActive(false);
        }
        
        #endregion

        public void SetCurrentPlayedPresetModel(PresetModel presetModel)
        {
            CurrentPresetModel?.Stop();
            CurrentPresetModel = presetModel;
        }

        public void StopAllSound()
        {
            CurrentPresetModel?.Stop();
        }
        
        public void SetAccentColor(Color color)
        {
            ColorLibrary.GetLibraryColor(ColorLibrary.ColorMainAccent).SetColor(color);
            ColorLibrary.UpdateAllColor();
        }
        
        #region Localization
        
        private void Reload()
        {
            LocalizationManager.Reload();
            Debug.Log("Reload");
        }

        private void Prev()
        {
            var list = LocalizationManager.LocalizationList;
            var index = list.IndexOf(LocalizationManager.CurrentLanguage) - 1;
            LocalizationManager.CurrentLanguage = list[index >= 0 ? index : list.Count - 1];
        }

        private void Next()
        {
            var list = LocalizationManager.LocalizationList;
            var index = list.IndexOf(LocalizationManager.CurrentLanguage) + 1;
            LocalizationManager.CurrentLanguage = list[index < list.Count ? index : 0];
        }

        private void LocalizationUpdate()
        {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))
                    Reload();
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    Prev();
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                    Next();
            }
        }
        
#endregion
    }
}