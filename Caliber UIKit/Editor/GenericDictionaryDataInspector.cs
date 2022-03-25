using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.System.Scripts;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.UI.Editor
{
    public abstract class GenericDictionaryDataInspector<TTargetType, TArrayType, TKey, TValue> : Inspector<TTargetType>
        where TValue : class
        where TTargetType : Object
        where TArrayType : IDictionaryElement<TKey, TValue>, new()
    {
        protected virtual String HiddenDictionaryFieldName { get { return "_data"; } }

        private IEnumerable<TKey> _validKeys;
        protected virtual IEnumerable<TKey> ValidKeys { get { return _validKeys; } }

        private List<String> AvailableKeys { get { return ValidKeys.Where(k => AddedKeys.All(ak => !Equals(ak, k))).Select(t => t.ToString()).ToList(); } }

        protected virtual Boolean IsCustomKey { get { return !typeof(Enum).IsAssignableFrom(typeof(TKey)); } }

        private FieldInfo _hiddenDictionary;

        protected List<TKey> AddedKeys;
        protected List<TValue> AddedValues;

        protected override Boolean IsInvalidState { get { return (Target == null) || (_hiddenDictionary == null); } }

        protected virtual Boolean IsFoldable { get { return false; } }
        private readonly Dictionary<TKey, Boolean> _isElementFoldedOut = new Dictionary<TKey, Boolean>();

        private void FillLists(TArrayType[] dictionary)
        {
            AddedKeys = dictionary.Select(t => t.Key).ToList();
            AddedValues = dictionary.Select(t => t.Value).ToList();
        }
        
        protected virtual bool IsSpoiler
        {
            get { return false; }
        }

        private void UpdateData(TArrayType[] dictionary)
        {
            for (int i = 0; i < AddedKeys.Count; i++)
            {
                dictionary[i] = new TArrayType
                {
                    Key = AddedKeys[i],
                    Value = AddedValues[i]
                };
            }
        }
        
        protected virtual bool DrawBefore()
        {
            return false;
        }
        
        protected virtual bool DrawAfter()
        {
            return false;
        }

        protected virtual Boolean DrawCustomValue(TKey key, TValue value)
        {
            Debug.LogError(String.Format("Method DrawCustomValue isn't implemented for inspector of type: {0}!",
                GetType().Name));
            return false;
        }

        protected virtual TValue CreateValueInstance(TKey key)
        {
            return Activator.CreateInstance<TValue>();
        }

        protected virtual Boolean IsVisibleItem(TKey key, TValue value)
        {
            return true;
        }

        protected virtual void AddItem(TValue value)
        {
            
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Target == null)
                return;

            if (AddedKeys != null)
                AddedKeys.Clear();

            if (AddedValues != null)
                AddedValues.Clear();

            _hiddenDictionary = typeof(TTargetType).GetField(HiddenDictionaryFieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            if (_hiddenDictionary == null)
            {
                Debug.LogError(String.Format("Unable to find {0}!", HiddenDictionaryFieldName));
                return;
            }

            var dictionary = _hiddenDictionary.GetValue(Target) as TArrayType[];
            if (dictionary == null)
            {
                dictionary = new TArrayType[0];
                _hiddenDictionary.SetValue(Target, dictionary);
            }

            FillLists(dictionary);

            if (typeof(Enum).IsAssignableFrom(typeof(TKey)))
            {
                _validKeys = Enum.GetValues(typeof(TKey)).Cast<TKey>().ToArray();
            }
            else
            {
                if (GetType().GetProperty(EditorExtentions.GetPropertyName(() => ValidKeys), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) == null)
                {
                    Debug.LogError(String.Format("It's expected that {0} implements property {1}", GetType().Name, EditorExtentions.GetPropertyName(() => ValidKeys)));
                }
                _validKeys = new TKey[0];
            }
        }

        public override void OnInspectorGUI()
        {
            if (IsInvalidState)
            {
                DrawDefaultInspector();
                return;
            }

            var isDataChanged = false;

            if (DrawBefore())
            { 
                isDataChanged = true;
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }

            var isCustomValue = !typeof(Object).IsAssignableFrom(typeof(TValue));
            EditorGUILayout.Space();
            for (int i = 0; i < AddedKeys.Count; i++)
            {
                if (IsFoldable)
                {
                    if (!_isElementFoldedOut.ContainsKey(AddedKeys[i]))
                    {
                        _isElementFoldedOut.Add(AddedKeys[i], false);
                    }
                    _isElementFoldedOut[AddedKeys[i]] = EditorGUILayout.Foldout(_isElementFoldedOut[AddedKeys[i]], AddedKeys[i].ToString());
                    if (!_isElementFoldedOut[AddedKeys[i]])
                    {
                        continue;
                    }
                }

                if (IsVisibleItem(AddedKeys[i], AddedValues[i]))
                {
                    if (isCustomValue)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        isDataChanged = DrawKey(AvailableKeys, i) || isDataChanged;
                        //EditorGUILayout.Space();
                        if (GUILayout.Button("Remove", GUILayout.Width(80)))
                        {
                            AddedKeys.RemoveAt(i);
                            AddedValues.RemoveAt(i);
                            i--;
                            isDataChanged = true;
                            continue;
                        }
                        EditorGUILayout.EndHorizontal();

                        isDataChanged = DrawItem(AddedKeys[i], AddedValues[i]) || isDataChanged;
                        /*
                        EditorGUILayout.Space();
                        isDataChanged = DrawCustomValue(AddedValues[i]) || isDataChanged;
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        */
                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        isDataChanged = DrawKey(AvailableKeys, i) || isDataChanged;
                        var value = EditorGUILayout.ObjectField(AddedValues[i] as Object, typeof(TValue), true) as TValue;
                        if (value != AddedValues[i])
                        {
                            isDataChanged = true;
                            AddedValues[i] = value;
                        }
                        if (GUILayout.Button("-"))
                        {
                            AddedKeys.RemoveAt(i);
                            AddedValues.RemoveAt(i);
                            i--;
                            isDataChanged = true;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            if (IsCustomKey)
            {
                if (GUILayout.Button("Add"))
                {
                    AddedKeys.Add(default(TKey));
                    var newItem = isCustomValue ? CreateValueInstance(default(TKey)) : null;
                    AddedValues.Add(newItem);
                    AddItem(newItem);
                    isDataChanged = true;
                }
            }
            else
            {
                if (AddedKeys.Count < ValidKeys.Count())
                {
                    if (GUILayout.Button("Add"))
                    {
                        var newKey = ValidKeys.First(key => AddedKeys.All(t => !Equals(t, key)));
                        AddedKeys.Add(newKey);
                        var newItem = isCustomValue ? CreateValueInstance(newKey) : null;
                        AddedValues.Add(newItem);
                        AddItem(newItem);
                        isDataChanged = true;
                    }
                }
            }

            if (DrawAfter())
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                isDataChanged = true;
            }

            if (isDataChanged)
            {
                var dictionary = _hiddenDictionary.GetValue(Target) as TArrayType[];
                if ((dictionary == null) || (dictionary.Length != AddedKeys.Count))
                {
                    dictionary = new TArrayType[AddedKeys.Count];
                    _hiddenDictionary.SetValue(Target, dictionary);
                }
                UpdateData(dictionary);
                
                MarkAsDirty();
            }
        }

        protected virtual Boolean DrawItem(TKey key, TValue value)
        {
            EditorGUILayout.Space();
            Boolean isDataChanged = DrawCustomValue(key, value);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            return isDataChanged;
        }

        protected void GenerateCompleteDictionary()
        {
            if (!IsCustomKey)
            {
                bool isCustomValue = !typeof(Object).IsAssignableFrom(typeof(TValue));

                while (AddedKeys.Count < ValidKeys.Count())
                {
                    var newKey = ValidKeys.First(key => AddedKeys.All(t => !Equals(t, key)));
                    AddedKeys.Add(newKey);
                    AddedValues.Add(isCustomValue ? CreateValueInstance(newKey) : null);
                }

                var dictionary = _hiddenDictionary.GetValue(Target) as TArrayType[];
                if ((dictionary == null) || (dictionary.Length != AddedKeys.Count))
                {
                    dictionary = new TArrayType[AddedKeys.Count];
                    _hiddenDictionary.SetValue(Target, dictionary);
                }
                UpdateData(dictionary);

                MarkAsDirty();
            }
        }

        protected virtual Boolean DrawKey(List<String> availableKeysArray, Int32 i)
        {
            var isDataChanged = false;
            availableKeysArray.Insert(0, AddedKeys[i].ToString());
            var newKeyIndex = EditorGUILayout.Popup(0, availableKeysArray.ToArray());
            if (newKeyIndex > 0)
            {
                AddedKeys[i] = ValidKeys.First(t => t.ToString() == availableKeysArray[newKeyIndex]);
                isDataChanged = true;
            }
            availableKeysArray.RemoveAt(newKeyIndex);
            return isDataChanged;
        }
    }
}