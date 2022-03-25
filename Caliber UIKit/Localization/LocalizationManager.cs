using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.System.Scripts;
using UnityEngine;

namespace UIKit
{
    public class LocalizationStringElement
    {
        public string Translation;
        public List<string> PluralTranslations;
    }

    public class LocalizationManager : SingletonResourcesAsset<LocalizationManager>
    {
        public static List<SystemLanguage> LocalizationList = new List<SystemLanguage>
        {
#if !DISABLE_CONSOLE
            SystemLanguage.Unknown,
#endif
            SystemLanguage.English,
            SystemLanguage.Russian,
            SystemLanguage.German,
            SystemLanguage.Italian
        };

        private Dictionary<SystemLanguage, string> _soundLanguages = new Dictionary<SystemLanguage, string>()
        {
#if !DISABLE_CONSOLE
            {SystemLanguage.Unknown, "Russian"},
#endif
            {SystemLanguage.English, "English(US)"},
            {SystemLanguage.Russian, "Russian"},
            {SystemLanguage.German, "German"},
            {SystemLanguage.Italian, "Italian"},
        };

        public static event Action<SystemLanguage> EventLanguageChanged;

        #region SerializableData
        [SerializeField]
        private SystemLanguage _defaultLanguage = SystemLanguage.English;

        [SerializeField]
        private SystemLanguage _currentLanguage = SystemLanguage.English;

        [SerializeField]
        private string _localizationDataSubfolder = "Localization/";

        [SerializeField]
        private bool _fastLanguageChange;

#if UNITY_EDITOR
        [SerializeField]
        private bool _saveUnknownKeys;
#endif
        #endregion

        private Dictionary<string, LocalizationStringElement> _localizationData = new Dictionary<string, LocalizationStringElement>();
        private Func<int, int> _pluralFormEvaluationMethod;
        private List<string> _unknownKeys;
        private string _loadedMissionName;
        private readonly List<string> _missionKeysForUnload = new List<string>();
        private string _currentLocalizationName = "Unknown";

        public static string LocalizationName => Enum.GetName(typeof(SystemLanguage), Instance._currentLanguage);
        public static string WwiseLanguage => Instance._soundLanguages[Instance._currentLanguage];
        public static bool FastLanguageChange => Instance._fastLanguageChange;

        private static string[] _substituteArgs = new string[3];
        private static StringBuilder _substituteBuilder = new StringBuilder();

        protected override void OnInstantiation()
        {
            base.OnInstantiation();

            UpdateLocalizationData();
        }

        private void UpdateLocalizationData()
        {
            if (_currentLanguage != SystemLanguage.Unknown)
                _currentLocalizationName = _currentLanguage.GetLocaleName();
            else
#if !DISABLE_CONSOLE
                _currentLocalizationName = "Unknown";
#else
            if (Application.systemLanguage != SystemLanguage.Unknown)
                _currentLocalizationName = Application.systemLanguage.GetLocaleName();
            else
                _currentLocalizationName = _defaultLanguage.GetLocaleName();
#endif
            _localizationData.Clear();
            LoadLocalization();
        }
        
        public static SystemLanguage CurrentLanguage
        {
            get => Instance._currentLanguage;
            set
            {
                if (Instance._currentLanguage == value)
                    return;

                Debug.Log("SetCurrentLanguage: " + value);
                Instance._currentLanguage = value;
                Reload();

                UpdateWwiseLanguage();
                
                EventLanguageChanged?.Invoke(Instance._currentLanguage);
            }
        }

        public static void UpdateWwiseLanguage()
        {
            if (AkSoundEngine.GetCurrentLanguage() == WwiseLanguage)
                return;
            
            AkSoundEngine.SetCurrentLanguage(WwiseLanguage);
        }

        public static void Reload()
        {
            Instance.UpdateLocalizationData();
            TextStyleComponent.UpdateLocalizationAll();
        }

        private static void UpdateText(GameObject obj)
        {
            if (obj == null)
                return;

            foreach (Transform child in obj.transform)
            {
                UpdateText(child.gameObject);
            }

            TextStyleComponent component = obj.GetComponent<TextStyleComponent>();
            if (component)
            {
                if (component.isLocalizationRequired)
                    component.UpdateLocalization();
            }
        }

        public static void UpdateAllText()
        {
#if UNITY_EDITOR
            EditorExtentions.ExecuteActionForObjects<TextStyleComponent>(EditorExtentions.IsSceneObject, text =>
            {
                if ((text as TextStyleComponent).isLocalizationRequired)
                    text.ExecuteMethod("UpdateLocalization");
            });
#endif
        }


        private void LoadLocalization()
        {
            var localizationFilesPath = _localizationDataSubfolder + _currentLocalizationName + "/";

            var localizationExternalFilesPath = Application.streamingAssetsPath + "/" + localizationFilesPath;
            var localizationExternalFiles = new List<string>();

            if (Directory.Exists(localizationExternalFilesPath))
            {
                localizationExternalFiles.AddRange(Directory.GetFiles(localizationExternalFilesPath, "*.TXT"));
                if (Directory.Exists(localizationExternalFilesPath + "Hints/"))
                {
                    localizationExternalFiles.AddRange(Directory.GetFiles(localizationExternalFilesPath + "Hints/", "*.TXT"));
                }
            }

            foreach (var externalFile in localizationExternalFiles)
            {
                AddLocalizationToData(File.ReadAllText(externalFile));
            }

            // Internal Files

            var localizationFiles = Resources.LoadAll<TextAsset>(localizationFilesPath);

            if (localizationFiles == null)
            {
                Debug.LogError("Can't load localization from " + localizationFilesPath);
                return;
            }

            foreach (var localizationFile in localizationFiles)
            {
                AddLocalizationToData(localizationFile.text, true);
            }

            _pluralFormEvaluationMethod = POFileHelper.EvaluationMethod;
        }

        private void AddLocalizationToData(string localFileText, bool suppressError = false)
        {
            var fileData = POFileHelper.Parse(localFileText);
            foreach (var element in fileData)
            {
                var key = element.Key.ToUpper();
                if (_localizationData.ContainsKey(key))
                {
                    if (!suppressError)
                    {
                        Debug.LogError(string.Format("This localization key already contains: {0}", key));
                    }
                    continue;
                }
                _localizationData.Add(key, element.Value);
            }
        }

        public static bool Exists(string localizationKey)
        {
            var key = localizationKey?.ToUpper();
            return !string.IsNullOrEmpty(key) && Instance._localizationData.ContainsKey(key);
        }

        public static bool TryGetKey(string localizationKey, out string key)
        {
            return CheckLocalizationKey(localizationKey, out key);
        }

        private static bool CheckLocalizationKey(string localizationKey, out string keyUpper)
        {
            keyUpper = string.Empty;

            if (localizationKey == null)
            {
                Debug.LogError("Error: localizationKey is null");
                return false;
            }

            if (Instance._localizationData == null)
            {
                Debug.Log("There is no localization for " + Application.systemLanguage);
                return false;
            }

            keyUpper = localizationKey.ToUpper();

            if (!Instance._localizationData.ContainsKey(keyUpper))
            {
                Instance.SaveUnknownKey(keyUpper);
                return false;
            }

            return true;
        }

        private static string InsertSubstituteArgs(string translation, string[] substituteValues)
        {
            var nonNullValuesCount = 0;
            if (substituteValues != null)
            {
                foreach (var value in substituteValues)
                {
                    if (value == null)
                        break;
                    nonNullValuesCount++;
                }
            }

            if (nonNullValuesCount == 0)
                return translation;

            var readingPlaceholder = false;
            var startIndex = 0;
            _substituteBuilder.Clear();
            var hasParseError = false;
            for (var charIndex = 0; charIndex < translation.Length; charIndex++)
            {
                var ch = translation[charIndex];
                if (ch == '{')
                {
                    if (readingPlaceholder)
                    {
                        hasParseError = true;
                        break;
                    }

                    readingPlaceholder = true;
                    _substituteBuilder.Append(translation.Substring(startIndex, charIndex - startIndex));
                    startIndex = charIndex;
                }
                else if (ch == '}')
                {
                    if (!readingPlaceholder)
                    {
                        hasParseError = true;
                        break;
                    }

                    var placeholder = translation.Substring(startIndex + 1, charIndex - startIndex - 1);
                    readingPlaceholder = false;
                    startIndex = charIndex + 1;
                    int argIndex;

                    if (int.TryParse(placeholder, out argIndex))
                    {
                        if (argIndex < nonNullValuesCount)
                            _substituteBuilder.Append(substituteValues[argIndex]);
                        else
                            Debug.LogWarning($"No substitute arg with index {argIndex} for translation: {translation}");
                    }
                    else
                    {
                        var indexOfColon = placeholder.IndexOf(':');
                        if (indexOfColon == -1 || indexOfColon + 1 == placeholder.Length || !int.TryParse(placeholder.Substring(0, indexOfColon), out argIndex))
                        {
                            hasParseError = true;
                            break;
                        }

                        var arg = string.Empty;
                        if (argIndex < nonNullValuesCount)
                            arg = substituteValues[argIndex];
                        else
                            Debug.LogWarning($"No substitute arg with index {argIndex} for translation: {translation}");

                        if (placeholder[indexOfColon + 1] != 'D' || !int.TryParse(placeholder.Substring(indexOfColon + 2), out var numZeros))
                        {
                            hasParseError = true;
                            break;
                        }

                        for (var i = 0; i < numZeros - arg.Length; i++)
                            _substituteBuilder.Append('0');

                        _substituteBuilder.Append(arg);
                    }
                }
            }

            if (hasParseError)
            {
                Debug.LogError($"Error parsing translation for placeholders: {translation}");
                return translation;
            }

            _substituteBuilder.Append(translation.Substring(startIndex, translation.Length - startIndex));
            return _substituteBuilder.ToString();
        }

        public static string Localize(string localizationKey)
        {
            if (!CheckLocalizationKey(localizationKey, out var keyUpper))
                return localizationKey;

            for (var i = 0; i < _substituteArgs.Length; i++)
                _substituteArgs[i] = null;

            return InsertSubstituteArgs(Instance._localizationData[keyUpper].Translation, _substituteArgs);
        }

        public static string Localize(string localizationKey, string arg1)
        {
            if (!CheckLocalizationKey(localizationKey, out var keyUpper))
                return localizationKey;

            _substituteArgs[0] = arg1;
            for (var i = 1; i < _substituteArgs.Length; i++)
                _substituteArgs[i] = null;

            return InsertSubstituteArgs(Instance._localizationData[keyUpper].Translation, _substituteArgs);
        }

        public static string Localize(string localizationKey, string arg1, string arg2)
        {
            if (!CheckLocalizationKey(localizationKey, out var keyUpper))
                return localizationKey;

            _substituteArgs[0] = arg1;
            _substituteArgs[1] = arg2;
            for (var i = 2; i < _substituteArgs.Length; i++)
                _substituteArgs[i] = null;

            return InsertSubstituteArgs(Instance._localizationData[keyUpper].Translation, _substituteArgs);
        }

        public static string Localize(string localizationKey, string arg1, string arg2, string arg3)
        {
            if (!CheckLocalizationKey(localizationKey, out var keyUpper))
                return localizationKey;

            _substituteArgs[0] = arg1;
            _substituteArgs[1] = arg2;
            _substituteArgs[2] = arg3;
            for (var i = 3; i < _substituteArgs.Length; i++)
                _substituteArgs[i] = null;

            return InsertSubstituteArgs(Instance._localizationData[keyUpper].Translation, _substituteArgs);
        }

        public static string Localize(string localizationKey, string[] args)
        {
            if (!CheckLocalizationKey(localizationKey, out var keyUpper))
                return localizationKey;

            return InsertSubstituteArgs(Instance._localizationData[keyUpper].Translation, args);
        }

        public static string LocalizeDateTimeAgo(DateTime dateTime)
        {
            dateTime = dateTime.ToLocalTime();
            var daysAgo = DateTime.Now.Day - dateTime.Day;
            if (daysAgo < 0)
                return dateTime.ToLongDateString();
            if (daysAgo == 0)
                return Localize("DATETIME_TODAY", dateTime.ToShortTimeString());
            if (daysAgo == 1)
                return Localize("DATETIME_YESTERDAY", dateTime.ToShortTimeString());
            if (daysAgo < 5)
                return Localize("DATETIME_DAYS_AGO_2", daysAgo.ToString());
            if (daysAgo < 7)
                return Localize("DATETIME_DAYS_AGO_5", daysAgo.ToString());
            if (daysAgo < 14)
                return Localize("DATETIME_WEEK_AGO");
            return Localize("DATETIME_LONG_TIME_AGO");
        }

        public static string LocalizeDateTimeLeft(DateTime dateTime, bool isCropped)
        {
            var timeLeft = dateTime - DateTime.Now;
            if (isCropped)
            {
                if (timeLeft.Days >= 365)
                    return $"{(int) Math.Round(timeLeft.TotalDays / 365f)}{Localize("QUESTS_LASTING_YEARS")}";
                if (timeLeft.Days > 0)
                    return $"{(int) Math.Round(timeLeft.TotalDays)}{Localize("QUESTS_LASTING_DAYS")}";
                if (timeLeft.Hours > 0)
                    return $"{(int) Math.Round(timeLeft.TotalHours)}{Localize("QUESTS_LASTING_HOUR")}";
                if (timeLeft.Minutes > 0)
                    return $"{(int) Math.Round(timeLeft.TotalMinutes)}{Localize("QUESTS_LASTING_MINUTE")}";
                return $"{timeLeft.Seconds}{Localize("QUESTS_LASTING_SECOND")}";
            }
            var str = $"{timeLeft.Hours}:{timeLeft.Minutes:00}:{timeLeft.Seconds:00}";
            if (timeLeft.Days > 0 && timeLeft.Days % 365 != 0)
                str = $"{timeLeft.Days % 365}{Localize("QUESTS_LASTING_DAYS")} {str}";
            if (timeLeft.Days > 365)
                str = $"{(int) (timeLeft.TotalDays / 365f)}{Localize("QUESTS_LASTING_YEARS")} {str}";
            return str;
        }

        public void SaveUnknownKey(string key)
        {
#if UNITY_EDITOR
            if (!_saveUnknownKeys)
                return;

            var unknownKeysFilePath = Application.persistentDataPath + "/unknownKeys.po";

            if (_unknownKeys == null)
            {
                _unknownKeys = new List<string>();
            }

            if (!_unknownKeys.Contains(key))
            {
                _unknownKeys.Add(key);
            }
            else
            {
                return;
            }

            var strToSave = string.Format("# unknown localization keys for {0} \n\n", _currentLocalizationName);

            foreach (var savedKey in _unknownKeys)
            {
                strToSave += string.Format("msgid \"{0}\"\nmsgstr \"{0}\"\n\n", savedKey);
            }

            TextWriter tw = new StreamWriter(unknownKeysFilePath);
            tw.Write(strToSave);
            tw.Close();

            Debug.Log(string.Format("Unknown key {0} added to {1}", key, unknownKeysFilePath));
#endif
        }

        public static Dictionary<string, LocalizationStringElement> GetLocalesStartsWith(string prefix)
        {
#if UNITY_EDITOR
            if (Instance._localizationData == null) return null;
            return Instance._localizationData.Where(locPair => locPair.Key.StartsWith(prefix)).ToDictionary(i => i.Key, i => i.Value);
#else
            return null;
#endif
        }
    }
}