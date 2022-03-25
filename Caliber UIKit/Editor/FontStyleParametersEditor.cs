using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using GameUI;

namespace UIKit
{
    [CustomEditor(typeof(FontStyleParameters))]
    public class FontStyleParametersEditor : Editor
    {
        private SerializedProperty _parent;

        Dictionary<SerializedProperty, Tuple<SerializedProperty, SerializedProperty>> _properties = new Dictionary<SerializedProperty, Tuple<SerializedProperty, SerializedProperty>>();

        private void FindProperty(string fieldName)
        {
            var property = serializedObject.FindProperty(fieldName);

            var overrideProperty = property.FindPropertyRelative("Override");
            var valueProperty = property.FindPropertyRelative("Value");

            _properties.Add(property, new Tuple<SerializedProperty, SerializedProperty>(overrideProperty, valueProperty));
        }

        public void OnEnable()
        {
            _parent = serializedObject.FindProperty("_parent");

            FindProperty("_font");
            FindProperty("_material");
            FindProperty("_size");
            FindProperty("_lineSpacing");
            FindProperty("_letterSpacing");
            FindProperty("_paragraphSpacing");
            FindProperty("_color");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_parent);
            EditorGUILayout.Space();

            foreach (var property in _properties)
            {
                if (_parent.objectReferenceValue != null)
                {
                    EditorGUILayout.PropertyField(property.Value.Item1, new GUIContent(property.Key.displayName));
                    EditorGUI.indentLevel++;

                    if (property.Value.Item1.boolValue)
                        EditorGUILayout.PropertyField(property.Value.Item2);

                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.PropertyField(property.Value.Item2, new GUIContent(property.Key.displayName));
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                UpdateAllText();
            }
        }

        private void UpdateAllText()
        {
            EditorExtentions.ExecuteActionForObjects<TextStyleComponent>(EditorExtentions.IsSceneObject, obj =>
            {
                if (obj != null)
                    obj.UpdateStyle();
            });
        }
    }
}

