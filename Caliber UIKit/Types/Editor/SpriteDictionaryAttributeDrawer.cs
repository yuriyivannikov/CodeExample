using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UIKit
{
    [CustomPropertyDrawer(typeof(SpriteDictionaryAttribute))]
    public class SpriteDictionaryAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                Rect rect = position;
                rect.height = 16;
                EditorGUI.LabelField(rect, label);
                rect.y += 16;
                rect.height = 64;
                property.objectReferenceValue = EditorGUI.ObjectField(rect, property.objectReferenceValue, typeof(Sprite), false);
            }
        }

        public override Single GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + 70f;
        }
    }
}
