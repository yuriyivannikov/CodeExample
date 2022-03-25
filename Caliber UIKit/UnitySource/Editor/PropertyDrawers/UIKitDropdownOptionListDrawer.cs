#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

using UnityEngine;

namespace UIKit
{
    // <summary>
    /// Является копией класса UnityEditor.UI.DropdownOptionListDrawer с небольшими правками
    /// https://bitbucket.org/Unity-Technologies/ui/src/f0c70f707cf09f959ad417049cb070f8e296ffe2/UnityEditor.UI/UI/PropertyDrawers/DropdownOptionListDrawer.cs?at=5.5&fileviewer=file-view-default
    /// </summary>

    [CustomPropertyDrawer(typeof(UIKitDropdown.OptionDataList), true)]
    class UIKitDropdownOptionListDrawer : PropertyDrawer
    {
        private ReorderableList m_ReorderableList;

        private void Init(SerializedProperty property)
        {
            if (m_ReorderableList != null)
                return;

            SerializedProperty array = property.FindPropertyRelative("m_Options");

            m_ReorderableList = new ReorderableList(property.serializedObject, array);
            m_ReorderableList.drawElementCallback = DrawOptionData;
            m_ReorderableList.drawHeaderCallback = DrawHeader;
            m_ReorderableList.elementHeight += 16;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            m_ReorderableList.DoList(position);
        }

        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Options");
        }

        private void DrawOptionData(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty itemData = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty itemText = itemData.FindPropertyRelative("m_Text");
            SerializedProperty itemImage = itemData.FindPropertyRelative("m_Image");

            RectOffset offset = new RectOffset(0, 0, -1, -3);
            rect = offset.Add(rect);
            rect.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(rect, itemText, GUIContent.none);
            rect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, itemImage, GUIContent.none);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);

            return m_ReorderableList.GetHeight();
        }
    }
}
#endif