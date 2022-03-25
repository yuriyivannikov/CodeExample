using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

public class CompareLozalization : EditorWindow
{
    [MenuItem("uGUI/Localization/Compare")]
    static void Init()
    {
        CompareLozalization window = (CompareLozalization) EditorWindow.GetWindow(typeof(CompareLozalization));
        window.Show();
    }

    private class LocalizationItemValue
    {
        public string FileName;
        public string Value;
        public string WarningText;

        public LocalizationItemValue(string value, string fileName)
        {
            Value = value;
            FileName = fileName;
        }
    }

    private class LocalizationItem
    {
        public string Code = "";
        public bool IsWarning = false;

        public bool IsNotFound = false;
        public bool IsNotTranslated = false;
        public bool IsDuplicate = false;
        public bool IsNoText = false;
        public bool IsDifferentFiles = false;

        public Dictionary<string, LocalizationItemValue> values = new Dictionary<string, LocalizationItemValue>();
    }

    private Dictionary<string, int> _columnWidth = new Dictionary<string, int>()
    {
        { "Key", 320 },
        { "File", 220 },
        { "Warning", 400 }
    }; 

    private Vector2 scroll;
    private bool _isScan = false;
    private string _localizationFilesPath;
    private int _maxLengthCode;
    private int _maxLengthFileName;

    //private bool _isFull = false;

    private bool _isNotFound = true;
    private bool _isNotTranslated = true;
    private bool _isDuplicate = true;
    private bool _isNoText = true;
    private bool _isDifferentFiles = true;

    private List<string> _languages;

    private List<LocalizationItem> _data;

    private FileInfo texttest;

    private List<string> _languageDirectories;

    void OnEnable()
    {
        _localizationFilesPath = Application.dataPath + "/UI/Resources/Localization/Languages/";

        RefreshLanguageDirectories();
        
        _languages = new List<string>();
        _data = new List<LocalizationItem>();

        _languages.Add("ru-RU");
        _languages.Add("en-US");
    }

    private void RefreshLanguageDirectories()
    {
        if (_languageDirectories == null)
        {
            _languageDirectories = new List<string>();
        }
        else
        {
            _languageDirectories.Clear();
        }
        if (Directory.Exists(_localizationFilesPath))
        {
            foreach (string dirPath in Directory.GetDirectories(_localizationFilesPath))
            {
                _languageDirectories.Add(dirPath.Substring(_localizationFilesPath.Length));
            }
        }
    }
    
    void OnGUI()
    {
        GUIStyle gsAlterQuest = new GUIStyle();
        gsAlterQuest.normal.background = new Texture2D(1, 1);
        gsAlterQuest.normal.background.SetPixels(new Color[] { Color.red });
        gsAlterQuest.normal.background.Apply();

        if (GUILayout.Button("Refresh languages", GUILayout.Width(200)))
        {
            RefreshLanguageDirectories();
            for (int i = _languages.Count - 1; i >= 0; i--)
            {
                if (!_languageDirectories.Contains(_languages[i]))
                {
                    _languages.RemoveAt(i);
                }
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Languages", GUILayout.Width(EditorGUIUtility.labelWidth));
        
        for (int i = 0; i < _languages.Count; i++)
        {
            int languageIndex = _languageDirectories.IndexOf(_languages[i]);

            int newlanguageIndex = EditorGUILayout.Popup(languageIndex, _languageDirectories.ToArray(), GUILayout.Width(120f));
            if (languageIndex != newlanguageIndex)
            {
                _languages[i] = _languageDirectories[newlanguageIndex];
            }

            if (GUILayout.Button("-", GUILayout.Width(24)))
            {
                _data.Clear();
                _languages.RemoveAt(i);
            }

            EditorGUILayout.LabelField("", GUILayout.Width(16));
        }

        EditorGUI.BeginDisabledGroup(_languages.Count >= _languageDirectories.Count);
        if (GUILayout.Button("Add", GUILayout.Width(32)))
        {
            _data.Clear();
            _languages.Add("");
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        
        EditorGUI.BeginDisabledGroup(_languages.Count < 2);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear", GUILayout.Width(100), GUILayout.MinHeight(32)))
        {
            _data.Clear();
        }

        if (GUILayout.Button("Start", GUILayout.MinHeight(32)))
        {
            _isScan = true;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();


        EditorGUILayout.LabelField("Filter");
        EditorGUILayout.BeginHorizontal();

        //_isFull = EditorGUILayout.ToggleLeft("Full", _isFull, GUILayout.Width(200f));

        _isNotFound = EditorGUILayout.ToggleLeft("Not found", _isNotFound, GUILayout.Width(200f));
        _isNotTranslated = EditorGUILayout.ToggleLeft("Not translated", _isNotTranslated, GUILayout.Width(200f));
        _isDuplicate = EditorGUILayout.ToggleLeft("Duplicate", _isDuplicate, GUILayout.Width(200f));
        _isNoText = EditorGUILayout.ToggleLeft("No text", _isNoText, GUILayout.Width(200f));
        _isDifferentFiles = EditorGUILayout.ToggleLeft("Different files", _isDifferentFiles, GUILayout.Width(200f));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Column width");
        EditorGUILayout.BeginHorizontal();

        foreach (string key in _columnWidth.Keys.ToList())
        {
            _columnWidth[key] = EditorGUILayout.IntField(key, _columnWidth[key], GUILayout.Width(200));
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Key", GUILayout.Width(_columnWidth["Key"]));
        EditorGUILayout.LabelField("File", GUILayout.Width(_columnWidth["File"]));

        foreach (string language in _languages)
        {
            EditorGUILayout.LabelField(language/*, GUILayout.Width(400)*/);
        }
        EditorGUILayout.LabelField("Warning", GUILayout.Width(_columnWidth["Warning"]));

        EditorGUILayout.EndHorizontal();

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(Screen.width));
        foreach (LocalizationItem item in _data)
        {
            /*
            if (!_isFull && !item.IsWarning )
            {
                continue;
            }
            */
            if (!(_isNotFound && item.IsNotFound || _isNoText && item.IsNoText || _isNotTranslated && item.IsNotTranslated 
                || _isDifferentFiles && item.IsDifferentFiles || _isDuplicate && item.IsDuplicate))
            {
                continue;
            }
            
            EditorGUILayout.BeginHorizontal();

            if (item.IsWarning)
            {
                EditorGUILayout.LabelField("", gsAlterQuest, GUILayout.Width(2f));
            }
            else
            {
                EditorGUILayout.LabelField("", GUILayout.Width(2f));
            }

            EditorGUILayout.TextField(item.Code, GUILayout.Width(_columnWidth["Key"]));
            
            EditorGUILayout.LabelField(item.values.ContainsKey(_languages[0]) ? item.values[_languages[0]].FileName : "", GUILayout.Width(_columnWidth["File"]));

            foreach (string language in _languages)
            {
                if (item.values.ContainsKey(language))
                {
                    EditorGUILayout.LabelField(new GUIContent(item.values[language].Value, item.values[language].Value)/*, GUILayout.Width(400)*/);
                }
                else
                {
                    EditorGUILayout.LabelField(""/*, GUILayout.Width(400)*/);
                }
            }

            if (item.IsWarning)
            {

                string text = "";

                foreach (string language in _languages)
                {
                    if (item.values[language].WarningText != null)
                    {
                        text += (text != "") ? " " : "" + language + ": " + item.values[language].WarningText;
                    }
                }
                
                EditorGUILayout.TextField(text, GUILayout.Width(_columnWidth["Warning"]));
            }
            else
            {
                EditorGUILayout.LabelField("");
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("", GUILayout.Width(_columnWidth["Key"]));
        EditorGUILayout.LabelField("", GUILayout.Width(_columnWidth["File"]));

        foreach (string language in _languages)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(language, GUILayout.Width(100));

            if (GUILayout.Button("Save log", GUILayout.Width(100)))
            {
                string path = EditorUtility.SaveFilePanel("Path to Save file", "", language + "_Log.txt", "txt");
                Debug.Log(path);
                if (path != "")
                {
                    var sr = File.CreateText(path);
                    sr.WriteLine(GetLog(language));
                    sr.Close();
                }
            }

            if (GUILayout.Button("Save", GUILayout.Width(100)))
            {
                SaveToFiles(language);
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.LabelField(" ", GUILayout.Width(_columnWidth["Warning"]));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("");
    }

    void Update()
    {
        if (_isScan)
        {
            Scan();
            _isScan = false;
            Repaint();
        }
    }

    private void Scan()
    {
        _maxLengthCode = 0;
        _maxLengthFileName = 0;
        _data.Clear();
        if (_languages.Count < 2)
        {
            return;
        }
        
        foreach (string language in _languages)
        {
            string localizationFilesPath = _localizationFilesPath + language + "/";

            List<string> localizationExternalFiles = new List<String>();

            if (Directory.Exists(localizationFilesPath))
            {
                localizationExternalFiles.AddRange(GetFiles(localizationFilesPath));
            }

            foreach (string filePath in localizationExternalFiles)
            {
                string fileName = filePath.Substring(localizationFilesPath.Length);
                
                var fileData = POFileHelper.Parse(File.ReadAllText(filePath));
                foreach (var element in fileData)
                {
                    LocalizationItem item = _data.FirstOrDefault(e => e.Code == element.Key);
                    if (item == null)
                    {
                        item = new LocalizationItem();
                        item.Code = element.Key;
                        _maxLengthCode = Math.Max(_maxLengthCode, item.Code.Length);
                        _maxLengthFileName = Math.Max(_maxLengthFileName, fileName.Length);
                        _data.Add(item);
                    }
                    if (item.values.ContainsKey(language))
                    {
                        item.IsDuplicate = true;
                        Debug.LogError("Duplicate! " + language + " - " + fileName + " - " + element.Key);
                    }
                    else
                    {
                        item.values.Add(language, new LocalizationItemValue(element.Value.Translation, fileName));
                    }
                }
            }
        }

        foreach (LocalizationItem item in _data)
        {
            foreach (string language in _languages)
            {
                LocalizationItemValue itemValue;
                if (!item.values.ContainsKey(language))
                {
                    item.IsWarning = true;
                    item.IsNotFound = true;
                    itemValue = new LocalizationItemValue("", "");
                    item.values.Add(language, itemValue);
                    itemValue.WarningText += "Not found1! ";
                }
                else
                {
                    itemValue = item.values[language];
                    if (itemValue.Value == "" || itemValue.Value == "$" + item.Code)
                    {
                        item.IsWarning = true;
                        item.IsNoText = true;
                        itemValue.WarningText += "No text! ";
                    }

                    if (_languages[0] != language && itemValue.Value == item.values[_languages[0]].Value)
                    {
                        item.IsWarning = true;
                        item.IsNotTranslated = true;
                        itemValue.WarningText += "Not translated! ";
                    }
                
                    if (itemValue.FileName != "" && itemValue.FileName.ToUpper() != item.values[_languages[0]].FileName.ToUpper())
                    {
                        item.IsWarning = true;
                        item.IsDifferentFiles = true;
                        itemValue.WarningText += "Different Files ("+ itemValue.FileName + " != " + item.values[_languages[0]].FileName + ")! ";
                    }
                }
            }
        }
    }

    private string GetLog(string language)
    {
        string result = "**********\n" + language + "\n**********\n\n";

        foreach (LocalizationItem item in _data)
        {
            if (item.IsWarning && item.values[language].WarningText != "")
            {
                string filename = item.values.ContainsKey(_languages[0]) ? item.values[_languages[0]].FileName : "";
                result += item.Code.PadRight(_maxLengthCode + 3) + filename.PadRight(_maxLengthFileName + 3) + item.values[language].WarningText /* + " - " + item.values[_languages[0]].Value*/ + "\n";
            }
        }
        return result;
    }

    private void SaveToFiles(string language)
    {
        string localizationFilesPath = _localizationFilesPath + _languages[0] + "/";

        string path = EditorUtility.SaveFolderPanel("Path to", "", "");
        Debug.Log(path);

        Dictionary<string, string> files = new Dictionary<string, string>();
        
        foreach (LocalizationItem item in _data)
        {
            if (item.values.ContainsKey(_languages[0]))
            {
                string fileName = item.values[_languages[0]].FileName;
                if (!files.ContainsKey(fileName) && File.Exists(localizationFilesPath + fileName))
                {
                    files.Add(fileName, File.ReadAllText(localizationFilesPath + fileName));
                }

                if (files.ContainsKey(fileName))
                {
                    string value = (item.values.ContainsKey(language) ? item.values[language].Value : "");
                    files[fileName] = Regex.Replace(files[fileName], "msgid\\s*\"" + item.Code + "\"[\f\n\t\r\\s]*msgstr\\s*\"(.*)\"",
                        "msgid \"" + item.Code + "\"\nmsgstr \"" + (value == "" ? string.Format("${0}", item.Code) : value) + "\"", RegexOptions.IgnoreCase);
                }
            }
        }
        
        foreach (var file in files)
        {
            Debug.Log(file.Key + " - " + file.Value.Length);

            FileInfo fileInfo = new FileInfo(path + "/" + file.Key);
            if (!Directory.Exists(fileInfo.Directory.FullName))
            {
                fileInfo.Directory.Create();
            }
            var sr = File.CreateText(path + "/" + file.Key);
            sr.WriteLine(file.Value);
            sr.Close();
        }


    }

    private List<string> GetFiles(string path)
    {
        List<string> result = new List<string>();
        if (Directory.Exists(path))
        {
            foreach (string dirPath in Directory.GetDirectories(path))
            {
                result.AddRange(GetFiles(dirPath));
            }
            result.AddRange(Directory.GetFiles(path, "*.TXT"));
        }
        return result;
    }
}
