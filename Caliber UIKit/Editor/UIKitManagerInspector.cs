using System;
using Assets.System.Scripts;
using Assets.UI.Editor;
using UIKit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.UI.UIKit.Editor.Managers
{
    [CustomEditor(typeof(UIKitManager))]
    public class UIKitManagerInspector : Inspector<UIKitManager>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            if (Target == null)
                return;
        }

        public override void OnInspectorGUI()
        {
            if (IsInvalidState)
                return;

            EditorGUILayout.LabelField("Templates");
            var isChanged = SetValue("_textPrefabTemplate", EditorGUILayout.ObjectField("Text", GetValue<GameObject>("_textPrefabTemplate", true), typeof(GameObject), false));
            isChanged = SetValue("_imagePrefabTemplate", EditorGUILayout.ObjectField("Image", GetValue<GameObject>("_imagePrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_buttonPrefabTemplate", EditorGUILayout.ObjectField("Button", GetValue<GameObject>("_buttonPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_togglePrefabTemplate", EditorGUILayout.ObjectField("Toggle", GetValue<GameObject>("_togglePrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_sliderPrefabTemplate", EditorGUILayout.ObjectField("Slider", GetValue<GameObject>("_sliderPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_progressBarPrefabTemplate", EditorGUILayout.ObjectField("ProgressBar", GetValue<GameObject>("_progressBarPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_scrollBarPrefabTemplate", EditorGUILayout.ObjectField("ScrollBar", GetValue<GameObject>("_scrollBarPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_dropdownPrefabTemplate", EditorGUILayout.ObjectField("Dropdown", GetValue<GameObject>("_dropdownPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_inputFieldPrefabTemplate", EditorGUILayout.ObjectField("InputField", GetValue<GameObject>("_inputFieldPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_scrollViewPrefabTemplate", EditorGUILayout.ObjectField("ScrollView", GetValue<GameObject>("_scrollViewPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_panelPrefabTemplate", EditorGUILayout.ObjectField("Panel", GetValue<GameObject>("_panelPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_currencyPrefabTemplate", EditorGUILayout.ObjectField("Currency", GetValue<GameObject>("_currencyPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_iconButtonPrefabTemplate", EditorGUILayout.ObjectField("IconButton", GetValue<GameObject>("_iconButtonPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_primaryButtonPrefabTemplate", EditorGUILayout.ObjectField("PrimaryButton", GetValue<GameObject>("_primaryButtonPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            isChanged = SetValue("_image3DPrefabTemplate", EditorGUILayout.ObjectField("Image3D", GetValue<GameObject>("_image3DPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            
            EditorGUILayout.Space();
            isChanged = SetValue("_focusIndicatorPrefabTemplate", EditorGUILayout.ObjectField("FocusIndicator", GetValue<GameObject>("_focusIndicatorPrefabTemplate", true), typeof(GameObject), false)) || isChanged;
            
            EditorGUILayout.Space();
            
            if (isChanged)
                EditorUtility.SetDirty(Target);
        }

        private void ExecuteUpdateScript<T>(Func<T, Boolean> updateScript) where T: MonoBehaviour
        {
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                    continue;

                EditorSceneManager.CloseScene(SceneManager.GetActiveScene(), true);
                EditorSceneManager.OpenScene(scene.path);

                var isSceneChanged = false;
                EditorExtentions.ExecuteActionForObjects<T>(EditorExtentions.IsSceneObject, component =>
                {
                    isSceneChanged = updateScript(component) || isSceneChanged;
                });

                if (isSceneChanged)
                {
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                }
            }

            //--- Code for updating prefabs

            var isDataChanged = false;

            EditorExtentions.ExecuteActionForPrefabs(prefab => true, (prefabSceneInstance, path) =>
            {
                var list = prefabSceneInstance.transform.GetCompleteChildList();
                list.Add(prefabSceneInstance.transform);
                var isPrefabChanged = false;
                foreach (var obj in list)
                {
                    var component = obj.GetComponent<T>();
                    if (component == null)
                        continue;

                    isPrefabChanged = updateScript(component);
                    isDataChanged = isPrefabChanged || isDataChanged;
                }
                if (isPrefabChanged)
                {
                    Debug.Log("Prefab " + prefabSceneInstance.name + " at path: " + path);
                    PrefabUtility.ReplacePrefab(prefabSceneInstance, AssetDatabase.LoadMainAssetAtPath(path));
                }
            });

            if (isDataChanged)
            {
                AssetDatabase.SaveAssets();
            }
        }
    }
}