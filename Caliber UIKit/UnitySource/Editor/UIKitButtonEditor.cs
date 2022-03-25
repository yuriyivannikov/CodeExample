using UnityEditor;

namespace UIKit
{
    // <summary>
    /// Является копией класса UnityEditor.UI.ButtonEditor с небольшими правками
    /// https://bitbucket.org/Unity-Technologies/ui/src/f0c70f707cf09f959ad417049cb070f8e296ffe2/UnityEditor.UI/UI/ButtonEditor.cs?at=5.5&fileviewer=file-view-default
    /// </summary>

    [CustomEditor(typeof(UIKitButton), true)]
    [CanEditMultipleObjects]
    public class UIKitButtonEditor : UIKitSelectableEditor
    {
        SerializedProperty m_OnClickProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_OnClickProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}