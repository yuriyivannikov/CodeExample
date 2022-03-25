using UnityEditor;

namespace UIKit
{
    // <summary>
    /// Является копией класса UnityEditor.UI.DropdownEditor с небольшими правками
    /// https://bitbucket.org/Unity-Technologies/ui/src/f0c70f707cf09f959ad417049cb070f8e296ffe2/UnityEditor.UI/UI/DropdownEditor.cs?at=5.5&fileviewer=file-view-default
    /// </summary>

    [CustomEditor(typeof(UIKitDropdown), true)]
    [CanEditMultipleObjects]
    public class UIKitDropdownEditor : UIKitSelectableEditor
    {
        SerializedProperty m_Template;
        SerializedProperty m_CaptionText;
        SerializedProperty m_CaptionImage;
        SerializedProperty m_ItemText;
        SerializedProperty m_ItemImage;
        SerializedProperty m_OnSelectionChanged;
        SerializedProperty m_Value;
        SerializedProperty m_Options;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Template = serializedObject.FindProperty("m_Template");
            m_CaptionText = serializedObject.FindProperty("m_CaptionText");
            m_CaptionImage = serializedObject.FindProperty("m_CaptionImage");
            m_ItemText = serializedObject.FindProperty("m_ItemText");
            m_ItemImage = serializedObject.FindProperty("m_ItemImage");
            m_OnSelectionChanged = serializedObject.FindProperty("m_OnValueChanged");
            m_Value = serializedObject.FindProperty("m_Value");
            m_Options = serializedObject.FindProperty("m_Options");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Template);
            EditorGUILayout.PropertyField(m_CaptionText);
            EditorGUILayout.PropertyField(m_CaptionImage);
            EditorGUILayout.PropertyField(m_ItemText);
            EditorGUILayout.PropertyField(m_ItemImage);
            EditorGUILayout.PropertyField(m_Value);
            EditorGUILayout.PropertyField(m_Options);
            EditorGUILayout.PropertyField(m_OnSelectionChanged);
            serializedObject.ApplyModifiedProperties();
        }
    }
}