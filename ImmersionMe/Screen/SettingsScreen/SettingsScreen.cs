using System.Collections.Generic;
using TheraBytes.BetterUi;
using TMPro;
using UIKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameClient
{
    public class SettingsScreen : UIScreen
    {
        private static readonly Dictionary<SystemLanguage, string> DictionaryLanguage = new Dictionary<SystemLanguage, string>
        {
            {SystemLanguage.English, "English"},
            {SystemLanguage.Italian, "Italiano"},
            {SystemLanguage.German, "Deutsch"},
            {SystemLanguage.Russian, "Русский"},
            {SystemLanguage.Unknown, "Localization Key"},
        };

        [SerializeField] public BetterButton BackButton;
        [SerializeField] public TMP_Dropdown Dropdown;
        [SerializeField] public BetterButton ResetAllButton;
        [SerializeField] public SimpleTable ColorTable;
        [SerializeField] public List<Color> ColorList;

        [SerializeField] private BetterButton FeedbackButton;
        [SerializeField] private BetterButton PrivacyPolicyButton;
        [SerializeField] private BetterButton TermsAndConditionsButton;

        [SerializeField] private LayoutElement _contentLayoutElement;

        private SettingsScreenData _data;
        private ScreenOrientation _screenOrientation;

        private void OnEnable()
        {
            BackButton.Click += OnBackButtonClick;
            LocalizationManager.EventLanguageChanged += OnLanguageChanged;
            Dropdown.onValueChanged.AddListener(OnDropdownLanguageValueChanged);
            ResetAllButton.Click += OnResetAllButtonClick;

            FeedbackButton.Click += OnClickFeedbackButton;
            PrivacyPolicyButton.Click += OnClickPrivacyPolicyButton;
            TermsAndConditionsButton.Click += OnClickTermsAndConditionsButton;
            
            AddListenerToItems(true);
        }

        private void Update()
        {
            if (_screenOrientation == Screen.orientation || _data == null)
                return;

            _screenOrientation = Screen.orientation;
                
            _data.Application.SetupScreenHeight(_contentLayoutElement);
        }
        
        private void OnClickFeedbackButton(BetterButton betterButton, BaseEventData baseEventData)
        {
            UnityEngine.Application.OpenURL("mailto:immersionapp@yandex.ru");
        }
        
        private void OnClickPrivacyPolicyButton(BetterButton arg1, BaseEventData arg2)
        {
            UnityEngine.Application.OpenURL("http://u0204579.cp.regruhosting.ru/yapp-mobile/immersion/privacypolicy.html");
        }
        
        private void OnClickTermsAndConditionsButton(BetterButton arg1, BaseEventData arg2)
        {
            UnityEngine.Application.OpenURL("http://u0204579.cp.regruhosting.ru/yapp-mobile/immersion/terms&conditions.html");
        }

        private void OnDisable()
        {
            BackButton.Click -= OnBackButtonClick;
            LocalizationManager.EventLanguageChanged -= OnLanguageChanged;
            Dropdown.onValueChanged.RemoveListener(OnDropdownLanguageValueChanged);
            ResetAllButton.Click -= OnResetAllButtonClick;
            
            FeedbackButton.Click -= OnClickFeedbackButton;
            PrivacyPolicyButton.Click -= OnClickPrivacyPolicyButton;
            TermsAndConditionsButton.Click -= OnClickTermsAndConditionsButton;
            
            AddListenerToItems(false);
            
            _data.Application.TrySaveData();
        }
    
        public override void SetData(object data)
        {
            if (data == null || _data != null)
                return;

            _data = (SettingsScreenData) data;
            
            // --- ColorTable
            
            ColorTable.Clear();

            var dataList = new List<ColoPickerTableItemData>();
            foreach (var color in ColorList)
            {
                dataList.Add(new ColoPickerTableItemData(color));
            }
            
            ColorTable.AddData(dataList);

            var currentColor = _data.Application.ApplicationData.Color;
            foreach (var item in ColorTable.GetAllItems())
            {
                var colorItem = (ColoPickerTableItem) item;
                if (colorItem.GetColor() == currentColor)
                    colorItem.Toggle.isOn = true;
            }

            AddListenerToItems(true);
            
            // --- ColorTable
            
            UpdateData();
        }
        
        private void AddListenerToItems(bool isAddListener)
        {
            foreach (var item in ColorTable.GetAllItems())
            {
                var colorItem = (ColoPickerTableItem) item;
                if (isAddListener)
                    colorItem.Toggle.onValueChanged.AddListener(OnColorToggleValueChanged);
                else
                    colorItem.Toggle.onValueChanged.RemoveListener(OnColorToggleValueChanged);
            }
        }

        private void UpdateData()
        {
            LanguageChanged(LocalizationManager.CurrentLanguage);
        }
        
        public override object GetData()
        {
            return _data;
        }
        
        private void OnColorToggleValueChanged(bool value)
        {
            if (!value)
                return;
                
            foreach (var item in ColorTable.GetAllItems())
            {
                var colorItem = (ColoPickerTableItem) item;
                if (colorItem.Toggle.isOn)
                {
                    var newColor = colorItem.GetColor();

                    _data.Application.ApplicationData.ColorR = newColor.r;
                    _data.Application.ApplicationData.ColorG = newColor.g;
                    _data.Application.ApplicationData.ColorB = newColor.b;
                    _data.Application.ApplicationData.ColorA = newColor.a;
                    
                    _data.Application.HasDataChanges = true;

                    _data.Application.SetAccentColor(newColor);
                }
            }
        }
    
        private void OnBackButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            _data.Application.HideScreen(ScreenType.SettingsScreen);
        }

        private void OnLanguageChanged(SystemLanguage language)
        {
            LanguageChanged(language);
        }

        private void LanguageChanged(SystemLanguage language)
        {
            var localizeLanguage = DictionaryLanguage[language];

            var index = 0;
            foreach (var optionData in Dropdown.options)
            {
                if (optionData.text == localizeLanguage)
                {
                    Dropdown.SetValueWithoutNotify(index);
                    break;
                }

                index++;
            }
        }

        private void OnDropdownLanguageValueChanged(int value)
        {
            var option = Dropdown.options[value];
            foreach (var keyValuePair in DictionaryLanguage)
            {
                if (keyValuePair.Value == option.text)
                {
                    LocalizationManager.CurrentLanguage = keyValuePair.Key;
                    _data.Application.ApplicationData.Language = LocalizationManager.CurrentLanguage;
                    _data.Application.HasDataChanges = true;
                    break;
                }
            }
        }
        
        private void OnResetAllButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            Debug.LogError("RESET ALL");
            
            LocalizationManager.CurrentLanguage = SystemLanguage.Unknown;
            _data.Application.ApplicationData.Language = LocalizationManager.CurrentLanguage;
            
            _data.Application.ShopManager.ResetData();
            
            _data.Application.HasDataChanges = true;
        }
    }
}