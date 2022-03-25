using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Assets.UI.Colors;
using GameUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UIKit
{
    [ExecuteInEditMode]
    public class TextStyleComponent : MonoBehaviour
    {
        private static List<TextStyleComponent> _textFieldList;

        public static void UpdateLocalizationAll()
        {
            if (LocalizationManager.FastLanguageChange && _textFieldList != null)
            {
                foreach (var text in _textFieldList)
                {
                    if (text != null && text._isLocalizationRequired)
                    {
                        text.UpdateLocalization();
                    }
                }
            }
        }

        /*
        * Компонент помогающий переопределить значения текста TextMeshPro используя стили
        * Работает непосредственно на объекте с TextMeshProUGUI
        */

        [SerializeField]
        [HideInInspector]
        private HorizontalLayoutGroup _containerLayoutController;

        [SerializeField]
        [HideInInspector]
        private TextMeshProUGUI _textComponent;

        public TextMeshProUGUI TextComponent
        {
            get
            {
                if (_textComponent == null)
                {
                    _textComponent = transform.GetComponent<TextMeshProUGUI>();
                }

                if (_textComponent == null)
                {
                    Debug.LogError(string.Format("TextComponent reference missing {0}", transform.GetHierarchyPath()));
                }

                return _textComponent;
            }
        }

        private bool _textDirty = false;
        
        [SerializeField]
        private bool _isLocalizationRequired = true;

        [SerializeField]
        private string _localizationKey;
        public string LocalizationKey => _localizationKey;
        public string LocalizedText => _textComponent?.text;

        [SerializeField]
        private bool _isAdditionalParsing;

        [SerializeField]
        private FontStyleParameters _fontStyleParametres;

        private readonly Dictionary<string, Func<string, string, string>> _additionalTextParsingFunctions = new Dictionary<string, Func<string, string, string>>();
        private static Dictionary<string, Func<string, string>> _additionalGlobalTagFunctions;

        
        [SerializeField]
        public bool _sizeOverride;

        [SerializeField]
        [Range(1, 100)]
        private int _size = 20;

        [SerializeField]
        private bool _colorOverride;

        [SerializeField]
        protected ColorLibraryItem _color;
        public ColorLibraryItem ColorLibraryItem => _color;


        // ----------

        private string[] _localizationArgs = new string[3];

        // ---

        #region properties
        public Color Color
        {
            get => _color;
            set
            {
                _colorOverride = true;
                _color.Set(value);
                UpdateStyle();
            }
        }

        public int Size
        {
            get => _size;
            set
            {
                _sizeOverride = true;
                _size = value;
                UpdateStyle();
            }
        }

        public string text
        {
            get => _localizationKey;
            set => SetText(value);
        }

        public virtual void SetText(int value)
        {
            if (int.TryParse(_localizationKey, out var result) && result == value)
                return;
            SetText(value.ToString());
        }

        public void SetText(string value)
        {
            if (!_textDirty && _localizationKey == value)
                return;
            _localizationKey = value;
            UpdateLocalization();
        }

        public bool isLocalizationRequired
        {
            get => _isLocalizationRequired;
            set
            {
                if (_isLocalizationRequired == value)
                    return;

                _isLocalizationRequired = value;
            }
        }

        public float Alpha
        {
            get => TextComponent.alpha;
            set => TextComponent.alpha = value;
        }
        #endregion

        private void Awake()
        {
            if (LocalizationManager.FastLanguageChange)
            {
                if (_textFieldList == null)
                {
                    _textFieldList = new List<TextStyleComponent>();
                }
                _textFieldList.Add(this);
            }

            //if (_fontStyleParametres == null)
                //Debug.LogError(string.Format("FontStyle reference missing {0}", transform.GetHierarchyPath()));
            
            UpdateLocalization();
        }

        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }
        
        private void Start()
        {
            if (_color == null)
            {
                _color = new ColorLibraryItem();
                _color.Change += UpdateStyle;
            }
            UpdateStyle(); //This method should be placed here, and not inside Awake!
        }

        private void OnEnable()
        {
            if(_textDirty)
                UpdateLocalization();
            
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        private void OnDestroy()
        {
            _color.Change -= UpdateStyle;
            if (LocalizationManager.FastLanguageChange && _textFieldList != null)
            {
                _textFieldList.Remove(this);
            }
        }

#if UNITY_EDITOR
        //public void OnValidate()
        //{
        //    if (EditorApplication.isPlaying)
        //        return;
        //    if (_color == null)
        //    {
        //        _color = new ColorLibraryItem();
        //        _color.Change += UpdateStyle;
        //    }
        //
        //    UpdateLocalization();
        //    UpdateStyle();
        //}
#endif
        public void SetLocalizationArgs(string arg1)
        {
            if (_localizationArgs[0] == arg1)
                return;

            _localizationArgs[0] = arg1;
            for (var i = 1; i < _localizationArgs.Length; i++)
                _localizationArgs[i] = null;
            UpdateLocalization();
        }

        public void SetLocalizationArgs(string arg1, string arg2)
        {
            if (_localizationArgs[0] == arg1 && _localizationArgs[1] == arg2)
                return;

            _localizationArgs[0] = arg1;
            _localizationArgs[1] = arg2;
            for (var i = 2; i < _localizationArgs.Length; i++)
                _localizationArgs[i] = null;
            UpdateLocalization();
        }

        public void SetLocalizationArgs(string arg1, string arg2, string arg3)
        {
            if (_localizationArgs[0] == arg1 && _localizationArgs[1] == arg2 && _localizationArgs[2] == arg3)
                return;

            _localizationArgs[0] = arg1;
            _localizationArgs[1] = arg2;
            _localizationArgs[3] = arg2;
            for (var i = 3; i < _localizationArgs.Length; i++)
                _localizationArgs[i] = null;
            UpdateLocalization();
        }

        public void SetLocalizationArgs(params string[] args)
        {
            if (_localizationArgs.Length < args.Length)
                _localizationArgs = new string[args.Length];
            for (var i = 0; i < _localizationArgs.Length; i++)
                _localizationArgs[i] = i < args.Length ? args[i] : null;
            UpdateLocalization();
        }

        public void SetLocalizationArgs(int arg1)
        {
            if (int.TryParse(_localizationArgs[0], out var arg) && arg == arg1)
                return;
            SetLocalizationArgs(arg1.ToSeparatorString());
        }

        public void SetLocalizationArgs(int arg1, int arg2)
        {
            var arg1Unchanged = int.TryParse(_localizationArgs[0], out var parsedArg1) && parsedArg1 == arg1;
            var arg2Unchanged = int.TryParse(_localizationArgs[1], out var parsedArg2) && parsedArg2 == arg2;
            if (arg1Unchanged && arg2Unchanged)
                return;
            SetLocalizationArgs(arg1.ToSeparatorString(), arg2.ToSeparatorString());
        }

        public static void AddGlobalTagFunctions(string type, Func<string, string> function)
        {
            if (_additionalGlobalTagFunctions == null)
                _additionalGlobalTagFunctions = new Dictionary<string, Func<string, string>>();

            if (_additionalGlobalTagFunctions.ContainsKey(type))
                _additionalGlobalTagFunctions[type] = function;
            else
                _additionalGlobalTagFunctions.Add(type, function);
        }

        public void AddTextParsingFunctions(string type, Func<string, string, string> function)
        {
            if (_additionalTextParsingFunctions.ContainsKey(type))
                _additionalTextParsingFunctions[type] = function;
            else
                _additionalTextParsingFunctions.Add(type, function);
            _textDirty = true;
        }

        public void RemoveTextParsingFunctions(string type)
        {
            if (_additionalTextParsingFunctions.ContainsKey(type))
                _additionalTextParsingFunctions.Remove(type);
            _textDirty = true;
        }
        
        private string AdditionalParsing(string str)
        {
            if (_additionalTextParsingFunctions.Count > 0)
            {
                {
                    var regex = new Regex(@"<code:([a-zA-Z0-9_\.%:]*)=([a-zA-Z0-9_\.%:]*)\/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    foreach (Match matche in regex.Matches(str))
                    {
                        var type = matche.Groups[1].Value;
                        if(_additionalTextParsingFunctions.TryGetValue(type, out var method))
                            str = str.Replace(matche.Value, AdditionalParsing(method(type, matche.Groups[2].Value)));
                        else
                            str = str.Replace(matche.Value, $"%{type}:{matche.Groups[2].Value}%");
                    }
                }

                {
                    var regex = new Regex(@"<code=([a-zA-Z0-9_%:]*)\/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    foreach (Match matche in regex.Matches(str))
                    {
                        var type = matche.Groups[1].Value;
                        if(_additionalTextParsingFunctions.TryGetValue(type, out var method))
                            str = str.Replace(matche.Value, AdditionalParsing(method(type, matche.Groups[2].Value)));
                        else
                            str = str.Replace(matche.Value, $"%{type}:{matche.Groups[2].Value}%");
                    }
                }

                while (true)// цикл для обработки вложенности тега <code></code>
                {
                    var regex = new Regex(@"<code=?([a-zA-Z0-9_%:]*)>([a-zA-Z0-9_%:]*)<\/code>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var matches = regex.Matches(str);
                    foreach (Match matche in matches)
                    {
                        var type = matche.Groups[1].Value;
                        if(_additionalTextParsingFunctions.TryGetValue(type, out var method))
                            str = str.Replace(matche.Value, AdditionalParsing(method(type, matche.Groups[2].Value)));
                        else
                            str = str.Replace(matche.Value, $"%{type}:{matche.Groups[2].Value}%");
                    }
                    if (matches.Count == 0)
                        break;
                }
            }
            return str;
        }

        private static string TagParsing(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('<') == -1 || text.IndexOf('=') == -1)
                return text;
            
            //TODO: оптимизировать
            var tags = string.Empty;
            if (_additionalGlobalTagFunctions != null)
                foreach (var tag in _additionalGlobalTagFunctions)
                    tags += "|" + tag.Key;
            var regex = new Regex($"<(icon|key|colorlib{tags})=([a-zA-Z0-9_%:\\/]*)\\/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            foreach (Match match in regex.Matches(text))
                switch (match.Groups[1].Value)
                {
                    case "icon":
                        text = text.Replace(match.Value, FontIconLibrary.GetTag(match.Groups[2].Value));
                        break;
                    case "key":
                        text = text.Replace(match.Value, LocalizationManager.Localize(match.Groups[2].Value));
                        break;
                    case "colorlib":
                        var arr = match.Groups[2].Value.Split('.');
                        var color = ColorLibrary.GetColor(arr.Length == 1 ? "Main" : arr[0], arr.Length > 1 ? arr[1] : arr[0]);
                        text = text.Replace(match.Value, $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>");
                        break;
                    default:
                        if (_additionalGlobalTagFunctions.TryGetValue(match.Groups[1].Value, out var action) && action != null)
                            text = text.Replace(match.Value, action(match.Groups[2].Value));
                        break;
                }
            return text;
        }

        public virtual void UpdateLocalization()
        {
            _textDirty = false;
            var str = _isLocalizationRequired ? LocalizationManager.Localize(_localizationKey, _localizationArgs) : _localizationKey;
            TextComponent.text = TagParsing(_isAdditionalParsing ? AdditionalParsing(str) : str);
        }
        
        private void OnTextChanged(Object obj)
        {
            if (obj == TextComponent)
                UpdateText();
        }

        public virtual void UpdateText()
        {
            const float InputImageSize = 20f;
            
            var hasMeshChanged = false;
            for (var i = 0; i < TextComponent.textInfo.characterCount; i++)
            {
                var characterInfo = TextComponent.textInfo.characterInfo[i];
                if (characterInfo.elementType != TMP_TextElementType.Sprite || !characterInfo.isVisible || characterInfo.spriteAsset == null || !characterInfo.spriteAsset.name.StartsWith("Input"))
                    continue;
                
                var meshIndex = characterInfo.materialReferenceIndex;
                var vertexIndex = characterInfo.vertexIndex;
                var meshInfo = TextComponent.textInfo.meshInfo[meshIndex];
                var position = (meshInfo.vertices[vertexIndex + 2] - meshInfo.vertices[vertexIndex]) * 0.5f + meshInfo.vertices[vertexIndex] - new Vector3(InputImageSize, InputImageSize, 0) * 0.5f;
                position = new Vector3((int)position.x, (int)position.y, 0f);
                meshInfo.vertices[vertexIndex    ] = position;
                meshInfo.vertices[vertexIndex + 1] = position + new Vector3(0, InputImageSize, 0);
                meshInfo.vertices[vertexIndex + 2] = position + new Vector3(InputImageSize, InputImageSize, 0);
                meshInfo.vertices[vertexIndex + 3] = position + new Vector3(InputImageSize, 0, 0);
                hasMeshChanged = true;
            }

            if(hasMeshChanged)
                for (int i = 0; i < TextComponent.textInfo.meshInfo.Length; i++)
                {
                    TextComponent.textInfo.meshInfo[i].mesh.vertices = TextComponent.textInfo.meshInfo[i].vertices;
                    TextComponent.UpdateGeometry(TextComponent.textInfo.meshInfo[i].mesh, i);
                }
        }

        public void UpdateStyle()
        {
            if (TextComponent == null)
                return;
            
            if (_fontStyleParametres == null)
            {
                if (_sizeOverride)
                    TextComponent.fontSize = _size;
                if (_colorOverride)
                    TextComponent.color = _color;
                
                TextComponent.SetVerticesDirty(); //.SetAllDirty();
            }
            else if (_fontStyleParametres.GetFont != null)
            {
                TextComponent.font = _fontStyleParametres.GetFont;
                TextComponent.fontMaterial = _fontStyleParametres.GetMaterial != null ? _fontStyleParametres.GetMaterial : _fontStyleParametres.GetFont.material;
                TextComponent.fontSize = _sizeOverride ? _size : _fontStyleParametres.GetSize;
                TextComponent.lineSpacing = _fontStyleParametres.GetLineSpacing;
                TextComponent.characterSpacing = _fontStyleParametres.GetLetterSpacing;
                TextComponent.paragraphSpacing = _fontStyleParametres.GetParagraphSpacing;
                TextComponent.color = _colorOverride ? _color : _fontStyleParametres.GetColor;
                TextComponent.SetVerticesDirty(); //.SetAllDirty();
            }
        }

        private bool HasFontStyleMessage(FontStyleParameters fontStyleParameters)
        {
            return fontStyleParameters != null;
        }
    }
}