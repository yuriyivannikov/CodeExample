using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIKit 
{
    public class SearchTextNotLocale : EditorWindow
    {
        private class ListItem
        {
            public TextStyleComponent Label { get; private set; }
            public string Name { get; private set; }
            public bool IsRequired { get; private set; }
            public string Key { get; private set; }
            public bool IsExists { get; private set; }
            public string Value { get; private set; }

            public ListItem(TextStyleComponent label)
            {
                Label = label;
                Name = label.name;
                IsRequired = label.isLocalizationRequired;
                Key = label.LocalizationKey;
                IsExists = LocalizationManager.Exists(label.text);
                Value = label.TextComponent.text;
            }
        }

        private Vector2 scroll;

        private Dictionary<Scene, List<ListItem>> _list = new Dictionary<Scene, List<ListItem>>();
        private bool _isFull = false;
        private bool _isDynamic = false;
        private bool _isSearch = false;

        private bool _filter = false;
        private string _filterName = "";
        private string _filterKey = "";
        private string _filterValue = "";

        //private Dictionary<TooltipProviderVisual, TooltipVisual> _tooltipList = new Dictionary<TooltipProviderVisual, TooltipVisual>();

        private void AddObject(TextStyleComponent obj)
        {
            Scene scene = obj.gameObject.scene;
            if (!_list.ContainsKey(scene))
            {
                _list.Add(scene, new List<ListItem>());
            }
            if (!_list[scene].Exists(e => e.Label == obj))
            {
                _list[scene].Add(new ListItem(obj));
            }
        }

        [MenuItem("uGUI/Localization/Search text not locale")]
        static void Init()
        {
            SearchTextNotLocale window = (SearchTextNotLocale)EditorWindow.GetWindow(typeof(SearchTextNotLocale));
            window.Show();
        }

        private string GetObjectPath(Component obj)
        {
            string result = "";
            while (obj)
            {
                result = obj.name + (result != "" ? "/" : "") + result;
                obj = obj.transform.parent;
            }
            return result;
        }

        void OnGUI()
        {

            //EditorGUILayout.HelpBox("Selecting a language in the property \"Current Language\" to \"Assets\\UI\\BASE\\Resources\\Managers\\LocalizationManager.asset\"", MessageType.None);
        
            EditorGUI.BeginDisabledGroup(_isSearch);

            List<SystemLanguage> languageList = LocalizationManager.LocalizationList;
            int languageIndex = languageList.IndexOf(LocalizationManager.CurrentLanguage);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Language", GUILayout.Width(EditorGUIUtility.labelWidth));
        
            if (GUILayout.Button("<<", GUILayout.Width(32f)))
            {
                LocalizationManager.CurrentLanguage = languageList[languageIndex - 1 >= 0 ? languageIndex - 1 : languageList.Count - 1];
            }
        
            List<string> languageStrList = new List<String>();
            foreach (var item in languageList)
            {
                languageStrList.Add(item.ToString());
            }
        
            int newlanguageIndex = EditorGUILayout.Popup(languageIndex, languageStrList.ToArray(), GUILayout.Width(200f));
            if (languageIndex != newlanguageIndex)
            {
                languageIndex = newlanguageIndex;
                LocalizationManager.CurrentLanguage = languageList[languageIndex];
            }
        
            if (GUILayout.Button(">>", GUILayout.Width(32f)))
            {
                LocalizationManager.CurrentLanguage = languageList[languageIndex + 1 < languageList.Count ? languageIndex + 1 : 0];
            }
        
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Search to \"" + LocalizationManager.LocalizationName + "\"", GUILayout.MinHeight(32)))
            {
                _isSearch = true;
            }

            if (GUILayout.Button("Clear", GUILayout.Width(50), GUILayout.MinHeight(32)))
            {
                ClearLists();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            _isFull = EditorGUILayout.ToggleLeft("Full", _isFull, GUILayout.Width(200f));
            _isDynamic = EditorGUILayout.ToggleLeft("Dynamic", _isDynamic, GUILayout.Width(200f));
            _filter = EditorGUILayout.ToggleLeft("Filter", _filter, GUILayout.Width(200f));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (_filter)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Filter");
                EditorGUILayout.BeginHorizontal();

                _filterName = EditorGUILayout.TextField(_filterName, GUILayout.Width(256f));
                EditorGUILayout.LabelField("", GUILayout.Width(16f));
                _filterKey = EditorGUILayout.TextField(_filterKey, GUILayout.Width(300f));
                EditorGUILayout.LabelField("", GUILayout.Width(16f));
                _filterValue = EditorGUILayout.TextField(_filterValue);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
        
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(UnityEngine.Screen.width) /*, GUILayout.Height(500)*/);

            GUIStyle gsAlterQuest = new GUIStyle();
            gsAlterQuest.normal.background = new Texture2D(1, 1);
            gsAlterQuest.normal.background.SetPixels(new Color[] { Color.red });
            gsAlterQuest.normal.background.Apply();

            if (!_isSearch)
            {
                int count = 0;
                foreach (var list in _list)
                {
                    bool isDrawCaption = false;

                    foreach (var item in list.Value)
                    {
                        if (!_isDynamic && Regex.IsMatch(item.Key, "%*%"))
                        {
                            continue;
                        }

                        if (_filter)
                        {
                            if (_filterName != "" && (item.Name == null || !item.Name.ToUpper().Contains(_filterName.ToUpper())))
                            {
                                continue;
                            }

                            if (_filterKey != "" && (item.Key == null || !item.Key.ToUpper().Contains(_filterKey.ToUpper())))
                            {
                                continue;
                            }

                            if (_filterValue != "" && (item.Value == null || !item.Value.ToUpper().Contains(_filterValue.ToUpper())))
                            {
                                continue;
                            }
                        }
                    
                        bool isWarning = item.IsRequired && !item.IsExists || !item.IsRequired && item.IsExists;
                        if (_isFull || isWarning)
                        {
                            if (!isDrawCaption)
                            {
                                isDrawCaption = true;
                                EditorGUILayout.BeginVertical("box");
                                EditorGUILayout.Space();
                                EditorGUILayout.LabelField(list.Key.name);
                                EditorGUILayout.Space();
                            }
                            EditorGUILayout.BeginHorizontal();

                            if (isWarning)
                            {
                                EditorGUILayout.LabelField("", gsAlterQuest, GUILayout.Width(2f));
                            }
                            else
                            {
                                EditorGUILayout.LabelField("", GUILayout.Width(2f));
                            }

                            EditorGUILayout.ObjectField(item.Label, typeof(TextStyleComponent), false, GUILayout.Width(256f));
                            EditorGUILayout.Toggle(item.IsRequired, GUILayout.Width(16f));
                            EditorGUILayout.TextField(item.Key, GUILayout.Width(300f));
                            EditorGUILayout.Toggle(item.IsExists, GUILayout.Width(16f));
                            EditorGUILayout.TextField(item.Value);

                            EditorGUILayout.EndHorizontal();
                            count++;
                        }

                    }

                    if (isDrawCaption)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }
                }

                if (count == 0)
                {
                    EditorGUILayout.LabelField("Empty");
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUI.EndDisabledGroup();

        }

        void Update()
        {
            if (_isSearch)
            {
                ClearLists();
                Search();
                _isSearch = false;
                Repaint();
            }
        }

        private void ClearLists()
        {
            /*
            foreach (var item in _tooltipList)
            {
                if (item.Value != null)
                {
                    Destroy(item.Value.gameObject);
                }
            }
            _tooltipList.Clear();
            _list.Clear();
            */
        }

        private void Search()
        {
            SearchTooltips();
            SearchText();

            /*
        if (SceneManager.GetActiveScene().path != activeScenePath)
        {
            EditorSceneManager.CloseScene(SceneManager.GetActiveScene(), true);
            EditorSceneManager.OpenScene(activeScenePath);
        }

        EditorExtentions.ExecuteActionForPrefabs(prefab => true, (prefabSceneInstance, path) =>
        {
            var list = prefabSceneInstance.transform.GetCompleteChildList();
            list.Add(prefabSceneInstance.transform);
            foreach (var obj in list)
            {
                TextStyleComponent component2 = obj.GetComponent<TextStyleComponent>() as TextStyleComponent;
                if (component2 != null)
                {
                    if (component2.isLocalizationRequired && !LocalizationManager.Exists(component2.text))
                        result += "\nTextStyleComponent - " + path + " | " + component2.text;
                }
            }
        });
        */
        }

        private void SearchText()
        {
            EditorExtentions.ExecuteActionForObjects<TextStyleComponent>(EditorExtentions.IsSceneObject, component =>
            {
                AddObject((TextStyleComponent)component);
            });
        }

        private void SearchTooltips()
        {
            /*
            EditorExtentions.ExecuteActionForObjects<TooltipProviderVisual>(EditorExtentions.IsSceneObject, provider =>
            {
                if (!_tooltipList.ContainsKey(provider))
                {
                    //_tooltipList.Add(provider, TooltipManager.Create(provider.GetPrefab()));
                }
            });
            */
        }
    }
}