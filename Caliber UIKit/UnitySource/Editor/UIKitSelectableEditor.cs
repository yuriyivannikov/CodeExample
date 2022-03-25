using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;

namespace UIKit
{
    // <summary>
    /// Является копией класса UnityEditor.UI.SelectableEditor с небольшими правками
    /// https://bitbucket.org/Unity-Technologies/ui/src/f0c70f707cf09f959ad417049cb070f8e296ffe2/UnityEditor.UI/UI/SelectableEditor.cs?at=5.5&fileviewer=file-view-default
    /// </summary>

    [CustomEditor(typeof(UIKitSelectable), true)]
    public class UIKitSelectableEditor : Editor
    {
        SerializedProperty m_Script;
        SerializedProperty m_InteractableProperty;
        SerializedProperty m_TargetGraphicProperty;
        SerializedProperty m_TransitionProperty;
        SerializedProperty m_ColorBlockProperty;
        SerializedProperty m_SpriteStateProperty;
        SerializedProperty m_AnimTriggerProperty;
        SerializedProperty m_NavigationProperty;

        GUIContent m_VisualizeNavigation = new GUIContent("Visualize", "Show navigation flows between selectable UI elements.");

        AnimBool m_ShowColorTint = new AnimBool();
        AnimBool m_ShowSpriteTrasition = new AnimBool();
        AnimBool m_ShowAnimTransition = new AnimBool();

        private static List<UIKitSelectableEditor> s_Editors = new List<UIKitSelectableEditor>();
        private static bool s_ShowNavigation = false;
        private static string s_ShowNavigationKey = "UIKitSelectableEditor.ShowNavigation";

        // Whenever adding new SerializedProperties to the Selectable and SelectableEditor
        // Also update this guy in OnEnable. This makes the inherited classes from Selectable not require a CustomEditor.
        private string[] m_PropertyPathToExcludeForChildClasses;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            m_InteractableProperty = serializedObject.FindProperty("m_Interactable");
            m_TargetGraphicProperty = serializedObject.FindProperty("m_TargetGraphic");
            m_TransitionProperty = serializedObject.FindProperty("m_Transition");
            m_ColorBlockProperty = serializedObject.FindProperty("m_Colors");
            m_SpriteStateProperty = serializedObject.FindProperty("m_SpriteState");
            m_AnimTriggerProperty = serializedObject.FindProperty("m_AnimationTriggers");
            m_NavigationProperty = serializedObject.FindProperty("m_Navigation");

            m_PropertyPathToExcludeForChildClasses = new[]
            {
                m_Script.propertyPath,
                m_NavigationProperty.propertyPath,
                m_TransitionProperty.propertyPath,
                m_ColorBlockProperty.propertyPath,
                m_SpriteStateProperty.propertyPath,
                m_AnimTriggerProperty.propertyPath,
                m_InteractableProperty.propertyPath,
                m_TargetGraphicProperty.propertyPath
            };

            var trans = GetTransition(m_TransitionProperty);
            m_ShowColorTint.value = (trans == UIKitSelectable.Transition.ColorTint);
            m_ShowSpriteTrasition.value = (trans == UIKitSelectable.Transition.SpriteSwap);
            m_ShowAnimTransition.value = (trans == UIKitSelectable.Transition.Animation);

            m_ShowColorTint.valueChanged.AddListener(Repaint);
            m_ShowSpriteTrasition.valueChanged.AddListener(Repaint);

            s_Editors.Add(this);
            RegisterStaticOnSceneGUI();

            s_ShowNavigation = EditorPrefs.GetBool(s_ShowNavigationKey);
        }

        protected virtual void OnDisable()
        {
            m_ShowColorTint.valueChanged.RemoveListener(Repaint);
            m_ShowSpriteTrasition.valueChanged.RemoveListener(Repaint);

            s_Editors.Remove(this);
            RegisterStaticOnSceneGUI();
        }

        private void RegisterStaticOnSceneGUI()
        {
            SceneView.onSceneGUIDelegate -= StaticOnSceneGUI;
            if (s_Editors.Count > 0)
                SceneView.onSceneGUIDelegate += StaticOnSceneGUI;
        }

        static UIKitSelectable.Transition GetTransition(SerializedProperty transition)
        {
            return (UIKitSelectable.Transition)transition.enumValueIndex;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!IsDerivedSelectableEditor())
                EditorGUILayout.PropertyField(m_Script);

            EditorGUILayout.PropertyField(m_InteractableProperty);

            var trans = GetTransition(m_TransitionProperty);

            var graphic = m_TargetGraphicProperty.objectReferenceValue as Graphic;
            if (graphic == null)
                graphic = (target as UIKitSelectable).GetComponent<Graphic>();

            var animator = (target as UIKitSelectable).GetComponent<Animator>();
            m_ShowColorTint.target = (!m_TransitionProperty.hasMultipleDifferentValues && trans == UIKitButton.Transition.ColorTint);
            m_ShowSpriteTrasition.target = (!m_TransitionProperty.hasMultipleDifferentValues && trans == UIKitButton.Transition.SpriteSwap);
            m_ShowAnimTransition.target = (!m_TransitionProperty.hasMultipleDifferentValues && trans == UIKitButton.Transition.Animation);

            EditorGUILayout.PropertyField(m_TransitionProperty);

            ++EditorGUI.indentLevel;
            {
                if (trans == UIKitSelectable.Transition.ColorTint || trans == UIKitSelectable.Transition.SpriteSwap)
                {
                    EditorGUILayout.PropertyField(m_TargetGraphicProperty);
                }

                switch (trans)
                {
                    case UIKitSelectable.Transition.ColorTint:
                        if (graphic == null)
                            EditorGUILayout.HelpBox("You must have a Graphic target in order to use a color transition.", MessageType.Warning);
                        break;

                    case UIKitSelectable.Transition.SpriteSwap:
                        if (graphic as Image == null)
                            EditorGUILayout.HelpBox("You must have a Image target in order to use a sprite swap transition.", MessageType.Warning);
                        break;
                }

                if (EditorGUILayout.BeginFadeGroup(m_ShowColorTint.faded))
                {
                    EditorGUILayout.PropertyField(m_ColorBlockProperty);
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(m_ShowSpriteTrasition.faded))
                {
                    EditorGUILayout.PropertyField(m_SpriteStateProperty);
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(m_ShowAnimTransition.faded))
                {
                    EditorGUILayout.PropertyField(m_AnimTriggerProperty);

                    if (animator == null || animator.runtimeAnimatorController == null)
                    {
                        Rect buttonRect = EditorGUILayout.GetControlRect();
                        buttonRect.xMin += EditorGUIUtility.labelWidth;
                        if (GUI.Button(buttonRect, "Auto Generate Animation", EditorStyles.miniButton))
                        {
                            var controller = GenerateSelectableAnimatorContoller((target as UIKitSelectable).animationTriggers, target as UIKitSelectable);
                            if (controller != null)
                            {
                                if (animator == null)
                                    animator = (target as UIKitSelectable).gameObject.AddComponent<Animator>();

                                AnimatorController.SetAnimatorController(animator, controller);
                            }
                        }
                    }
                }
                EditorGUILayout.EndFadeGroup();
            }
            --EditorGUI.indentLevel;

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(true);
            
            SerializedProperty navigationMode = m_NavigationProperty.FindPropertyRelative("m_Mode");

            if (navigationMode.enumValueIndex != (int)UIKitNavigation.Mode.None)
                navigationMode.enumValueIndex = (int)UIKitNavigation.Mode.None;

            EditorGUILayout.PropertyField(m_NavigationProperty);
            EditorGUI.EndDisabledGroup();

            // We do this here to avoid requiring the user to also write a Editor for their Selectable-derived classes.
            // This way if we are on a derived class we dont draw anything else, otherwise draw the remaining properties.
            ChildClassPropertiesGUI();

            serializedObject.ApplyModifiedProperties();
        }

        // Draw the extra SerializedProperties of the child class.
        // We need to make sure that m_PropertyPathToExcludeForChildClasses has all the Selectable properties and in the correct order.
        // TODO: find a nicer way of doing this. (creating a InheritedEditor class that automagically does this)
        private void ChildClassPropertiesGUI()
        {
            if (IsDerivedSelectableEditor())
                return;

            DrawPropertiesExcluding(serializedObject, m_PropertyPathToExcludeForChildClasses);
        }

        private bool IsDerivedSelectableEditor()
        {
            return GetType() != typeof(UIKitSelectableEditor);
        }

        private static AnimatorController GenerateSelectableAnimatorContoller(AnimationTriggers animationTriggers, UIKitSelectable target)
        {
            if (target == null)
                return null;

            // Where should we create the controller?
            var path = GetSaveControllerPath(target);
            if (string.IsNullOrEmpty(path))
                return null;

            // figure out clip names
            var normalName = string.IsNullOrEmpty(animationTriggers.normalTrigger) ? "Normal" : animationTriggers.normalTrigger;
            var highlightedName = string.IsNullOrEmpty(animationTriggers.highlightedTrigger) ? "Highlighted" : animationTriggers.highlightedTrigger;
            var pressedName = string.IsNullOrEmpty(animationTriggers.pressedTrigger) ? "Pressed" : animationTriggers.pressedTrigger;
            var disabledName = string.IsNullOrEmpty(animationTriggers.disabledTrigger) ? "Disabled" : animationTriggers.disabledTrigger;

            // Create controller and hook up transitions.
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            GenerateTriggerableTransition(normalName, controller);
            GenerateTriggerableTransition(highlightedName, controller);
            GenerateTriggerableTransition(pressedName, controller);
            GenerateTriggerableTransition(disabledName, controller);

            AssetDatabase.ImportAsset(path);

            return controller;
        }

        private static string GetSaveControllerPath(UIKitSelectable target)
        {
            var defaultName = target.gameObject.name;
            var message = string.Format("Create a new animator for the game object '{0}':", defaultName);
            return EditorUtility.SaveFilePanelInProject("New Animation Contoller", defaultName, "controller", message);
        }

        private static void SetUpCurves(AnimationClip highlightedClip, AnimationClip pressedClip, string animationPath)
        {
            string[] channels = { "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z" };

            var highlightedKeys = new[] { new Keyframe(0f, 1f), new Keyframe(0.5f, 1.1f), new Keyframe(1f, 1f) };
            var highlightedCurve = new AnimationCurve(highlightedKeys);
            foreach (var channel in channels)
                AnimationUtility.SetEditorCurve(highlightedClip, EditorCurveBinding.FloatCurve(animationPath, typeof(Transform), channel), highlightedCurve);

            var pressedKeys = new[] { new Keyframe(0f, 1.15f) };
            var pressedCurve = new AnimationCurve(pressedKeys);
            foreach (var channel in channels)
                AnimationUtility.SetEditorCurve(pressedClip, EditorCurveBinding.FloatCurve(animationPath, typeof(Transform), channel), pressedCurve);
        }

        private static string BuildAnimationPath(UIKitSelectable target)
        {
            // if no target don't hook up any curves.
            var highlight = target.targetGraphic;
            if (highlight == null)
                return string.Empty;

            var startGo = highlight.gameObject;
            var toFindGo = target.gameObject;

            var pathComponents = new Stack<string>();
            while (toFindGo != startGo)
            {
                pathComponents.Push(startGo.name);

                // didn't exist in hierarchy!
                if (startGo.transform.parent == null)
                    return string.Empty;

                startGo = startGo.transform.parent.gameObject;
            }

            // calculate path
            var animPath = new StringBuilder();
            if (pathComponents.Count > 0)
                animPath.Append(pathComponents.Pop());

            while (pathComponents.Count > 0)
                animPath.Append("/").Append(pathComponents.Pop());

            return animPath.ToString();
        }

        private static AnimationClip GenerateTriggerableTransition(string name, AnimatorController controller)
        {
            // Create the clip
            var clip = AnimatorController.AllocateAnimatorClip(name);
            AssetDatabase.AddObjectToAsset(clip, controller);

            // Create a state in the animatior controller for this clip
            var state = controller.AddMotion(clip);

            // Add a transition property
            controller.AddParameter(name, AnimatorControllerParameterType.Trigger);

            // Add an any state transition
            var stateMachine = controller.layers[0].stateMachine;
            var transition = stateMachine.AddAnyStateTransition(state);
            transition.AddCondition(AnimatorConditionMode.If, 0, name);
            return clip;
        }

        private static void StaticOnSceneGUI(SceneView view)
        {
            if (!s_ShowNavigation)
                return;

            for (int i = 0; i < UIKitSelectable.allSelectables.Count; i++)
            {
                DrawNavigationForSelectable(UIKitSelectable.allSelectables[i]);
            }
        }

        private static void DrawNavigationForSelectable(UIKitSelectable sel)
        {
            if (sel == null)
                return;

            Transform transform = sel.transform;
            bool active = Selection.transforms.Any(e => e == transform);
            Handles.color = new Color(1.0f, 0.9f, 0.1f, active ? 1.0f : 0.4f);
            DrawNavigationArrow(-Vector2.right, sel, sel.FindSelectableOnLeft());
            DrawNavigationArrow(Vector2.right, sel, sel.FindSelectableOnRight());
            DrawNavigationArrow(Vector2.up, sel, sel.FindSelectableOnUp());
            DrawNavigationArrow(-Vector2.up, sel, sel.FindSelectableOnDown());
        }

        const float kArrowThickness = 2.5f;
        const float kArrowHeadSize = 1.2f;

        private static void DrawNavigationArrow(Vector2 direction, UIKitSelectable fromObj, UIKitSelectable toObj)
        {
            if (fromObj == null || toObj == null)
                return;
            Transform fromTransform = fromObj.transform;
            Transform toTransform = toObj.transform;

            Vector2 sideDir = new Vector2(direction.y, -direction.x);
            Vector3 fromPoint = fromTransform.TransformPoint(GetPointOnRectEdge(fromTransform as RectTransform, direction));
            Vector3 toPoint = toTransform.TransformPoint(GetPointOnRectEdge(toTransform as RectTransform, -direction));
            float fromSize = HandleUtility.GetHandleSize(fromPoint) * 0.05f;
            float toSize = HandleUtility.GetHandleSize(toPoint) * 0.05f;
            fromPoint += fromTransform.TransformDirection(sideDir) * fromSize;
            toPoint += toTransform.TransformDirection(sideDir) * toSize;
            float length = Vector3.Distance(fromPoint, toPoint);
            Vector3 fromTangent = fromTransform.rotation * direction * length * 0.3f;
            Vector3 toTangent = toTransform.rotation * -direction * length * 0.3f;

            Handles.DrawBezier(fromPoint, toPoint, fromPoint + fromTangent, toPoint + toTangent, Handles.color, null, kArrowThickness);
            Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + toTransform.rotation * (-direction - sideDir) * toSize * kArrowHeadSize);
            Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + toTransform.rotation * (-direction + sideDir) * toSize * kArrowHeadSize);
        }

        private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
        {
            if (rect == null)
                return Vector3.zero;
            if (dir != Vector2.zero)
                dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
            return dir;
        }
    }
}