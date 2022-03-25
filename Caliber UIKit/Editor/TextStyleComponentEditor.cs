using UnityEngine;
using GameUI;
using UnityEditor;

namespace UIKit
{
    [CustomEditor(typeof(TextStyleComponent))]
    public class TextStyleComponentEditor : Editor
    {
        private SerializedProperty _isLocalizationRequired;
        private SerializedProperty _localizationKey;
        private SerializedProperty _isAdditionalParsing;
        private SerializedProperty _fontStyleParametres;

        private SerializedProperty _sizeOverride;
        private SerializedProperty _size;

        private SerializedProperty _colorOverride;
        private SerializedProperty _color;

        public void OnEnable()
        {
            _isLocalizationRequired = serializedObject.FindProperty("_isLocalizationRequired");
            _localizationKey = serializedObject.FindProperty("_localizationKey");
            _isAdditionalParsing = serializedObject.FindProperty("_isAdditionalParsing");
            _fontStyleParametres = serializedObject.FindProperty("_fontStyleParametres");

            _sizeOverride = serializedObject.FindProperty("_sizeOverride");
            _size = serializedObject.FindProperty("_size");

            _colorOverride = serializedObject.FindProperty("_colorOverride");
            _color = serializedObject.FindProperty("_color");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_isLocalizationRequired);
            EditorGUILayout.PropertyField(_localizationKey);
            EditorGUILayout.PropertyField(_isAdditionalParsing);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_fontStyleParametres);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_sizeOverride, new GUIContent("Size"));
            EditorGUI.indentLevel++;
            if (_sizeOverride.boolValue)
                EditorGUILayout.PropertyField(_size, new GUIContent("Value"));
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(_colorOverride, new GUIContent("Color"));
            EditorGUI.indentLevel++;
            if (_colorOverride.boolValue)
                EditorGUILayout.PropertyField(_color, new GUIContent("Value"));
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                var text = (target as TextStyleComponent);
                if (text != null)
                {
                    text.UpdateLocalization();
                    text.UpdateStyle();
                }
            }
        }
    }
}
