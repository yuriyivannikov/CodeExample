#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UIKit
{
    // <summary>
    /// Является копией класса UnityEditor.UI.NavigationDrawer с небольшими правками
    /// https://bitbucket.org/Unity-Technologies/ui/src/f0c70f707cf09f959ad417049cb070f8e296ffe2/UnityEditor.UI/UI/PropertyDrawers/NavigationDrawer.cs?at=5.5&fileviewer=file-view-default
    /// </summary>

    [CustomPropertyDrawer(typeof(UIKitNavigation), true)]
    public class UIKitNavigationDrawer : PropertyDrawer
    {
        private class Styles
        {
            readonly public GUIContent navigationContent;

            public Styles()
            {
                navigationContent = new GUIContent("Navigation");
            }
        }

        private static Styles s_Styles = null;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (s_Styles == null)
                s_Styles = new Styles();

            Rect drawRect = pos;
            drawRect.height = EditorGUIUtility.singleLineHeight;

            SerializedProperty navigation = prop.FindPropertyRelative("m_Mode");
            UIKitNavigation.Mode navMode = GetNavigationMode(navigation);

            EditorGUI.PropertyField(drawRect, navigation, s_Styles.navigationContent);

            ++EditorGUI.indentLevel;

            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            switch (navMode)
            {
                case UIKitNavigation.Mode.Explicit:
                    {
                        SerializedProperty selectOnUp = prop.FindPropertyRelative("m_SelectOnUp");
                        SerializedProperty selectOnDown = prop.FindPropertyRelative("m_SelectOnDown");
                        SerializedProperty selectOnLeft = prop.FindPropertyRelative("m_SelectOnLeft");
                        SerializedProperty selectOnRight = prop.FindPropertyRelative("m_SelectOnRight");

                        EditorGUI.PropertyField(drawRect, selectOnUp);
                        drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(drawRect, selectOnDown);
                        drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(drawRect, selectOnLeft);
                        drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(drawRect, selectOnRight);
                        drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                    break;
            }

            --EditorGUI.indentLevel;
        }

        static UIKitNavigation.Mode GetNavigationMode(SerializedProperty navigation)
        {
            return (UIKitNavigation.Mode)navigation.enumValueIndex;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            SerializedProperty navigation = prop.FindPropertyRelative("m_Mode");
            if (navigation == null)
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            UIKitNavigation.Mode navMode = GetNavigationMode(navigation);

            switch (navMode)
            {
                case UIKitNavigation.Mode.None: return EditorGUIUtility.singleLineHeight;
                case UIKitNavigation.Mode.Explicit: return 5 * EditorGUIUtility.singleLineHeight + 5 * EditorGUIUtility.standardVerticalSpacing;
                default: return EditorGUIUtility.singleLineHeight + 1 * EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }
}
#endif