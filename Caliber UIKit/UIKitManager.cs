using System;
using System.Collections.Generic;
using Assets.System.Scripts;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace UIKit
{
    [ExecuteInEditMode]
    public class UIKitManager : SingletonResourcesAsset<UIKitManager>
    {
        public enum UIKitGameObjectType
        {
            Text,
            Image,
            Button,
            Toggle,
            Slider,
            ProgressBar,
            ScrollBar,
            Dropdown,
            InputField,
            ScrollView,
            Panel,
            Currency,
            IconButton,
            PrimaryButton,
            Image3D,

            FocusIndicator
        }

        // -----------------------------------------------------------------------------------------------------
        //        [Serializable]
        //        public class ButtonBaseHeightDictionaryElement : IDictionaryElement<ButtonBaseHeight, Single>
        //        {
        //            [SerializeField]
        //            private ButtonBaseHeight _key;
        //
        //            [SerializeField]
        //            private Single _value;
        //
        //            public ButtonBaseHeight Key { get { return _key; } set { _key = value; } }
        //
        //            public Single Value { get { return _value; } set { _value = value; } }
        //        }

        //        [SerializeField]
        //        private ButtonBaseHeightDictionaryElement[] _buttonHeights;

        //        public Single GetButtonBaseHeight(ButtonBaseHeight buttonBaseHeight)
        //        {
        //            var tmp = _buttonHeights.FirstOrDefault(bh => bh.Key == buttonBaseHeight);
        //            return tmp == null ? default(Single) : tmp.Value;
        //        }
        // -----------------------------------------------------------------------------------------------------

        #region Editor

        [SerializeField]
        private GameObject _textPrefabTemplate;

        [SerializeField]
        private GameObject _imagePrefabTemplate;

        [SerializeField]
        private GameObject _buttonPrefabTemplate;

        [SerializeField]
        private GameObject _togglePrefabTemplate;
        
        [SerializeField]
        private GameObject _sliderPrefabTemplate;

        [SerializeField]
        private GameObject _progressBarPrefabTemplate;

        [SerializeField]
        private GameObject _scrollBarPrefabTemplate;

        [SerializeField]
        private GameObject _dropdownPrefabTemplate;

        [SerializeField]
        private GameObject _inputFieldPrefabTemplate;

        [SerializeField]
        private GameObject _scrollViewPrefabTemplate;

        [SerializeField]
        private GameObject _panelPrefabTemplate;
        
        [SerializeField]
        private GameObject _currencyPrefabTemplate;
        
        [SerializeField]
        private GameObject _iconButtonPrefabTemplate;
        
        [SerializeField]
        private GameObject _primaryButtonPrefabTemplate;

        [SerializeField]
        private GameObject _focusIndicatorPrefabTemplate;
        
        [SerializeField]
        private GameObject _image3DPrefabTemplate;

        #endregion

        private Dictionary<UIKitGameObjectType, GameObject> _templates;

        protected Dictionary<UIKitGameObjectType, GameObject> Templates
        {
            get
            {
                if (_templates != null)
                    return _templates;

                _templates = new Dictionary<UIKitGameObjectType, GameObject>
                {
                    {UIKitGameObjectType.Text, _textPrefabTemplate},
                    {UIKitGameObjectType.Image, _imagePrefabTemplate},
                    {UIKitGameObjectType.Button, _buttonPrefabTemplate},
                    {UIKitGameObjectType.Toggle, _togglePrefabTemplate},
                    {UIKitGameObjectType.Slider, _sliderPrefabTemplate},
                    {UIKitGameObjectType.ProgressBar, _progressBarPrefabTemplate},
                    {UIKitGameObjectType.ScrollBar, _scrollBarPrefabTemplate},
                    {UIKitGameObjectType.Dropdown, _dropdownPrefabTemplate},
                    {UIKitGameObjectType.InputField, _inputFieldPrefabTemplate},
                    {UIKitGameObjectType.ScrollView, _scrollViewPrefabTemplate},
                    {UIKitGameObjectType.Panel, _panelPrefabTemplate},
                    {UIKitGameObjectType.Currency, _currencyPrefabTemplate},
                    {UIKitGameObjectType.IconButton, _iconButtonPrefabTemplate},
                    {UIKitGameObjectType.PrimaryButton, _primaryButtonPrefabTemplate},
                    {UIKitGameObjectType.Image3D, _image3DPrefabTemplate},

                    {UIKitGameObjectType.FocusIndicator, _focusIndicatorPrefabTemplate}
                };

                return _templates;
            }
        }

        protected override void OnInstantiation()
        {
            base.OnInstantiation();

        }

#if UNITY_EDITOR
        [MenuItem("Assets/Create/UI/UIKitManager")]
        public static void CreateUIKitManager()
        {
            ScriptableObjectAsset.CreateAsset<UIKitManager>();
        }

        public static GameObject AddUIKitGameObject(UIKitGameObjectType typeUIKit, GameObject parent = null)
        {
            if (!Instance.Templates.ContainsKey(typeUIKit))
            {
                Debug.LogWarning("Unknown UIKitComponent of type: " + typeUIKit.ToString());
                return null;
            }
            var prefabInstance = Instantiate(Instance.Templates[typeUIKit]);
            if (prefabInstance == null)
            {
                Debug.LogError(String.Format("Unable to instantiate {0}!", typeUIKit.ToString()));
                return null;
            }

            if (parent == null)
            {
                if (Selection.activeGameObject != null)
                    GameObjectUtility.SetParentAndAlign(prefabInstance, Selection.activeGameObject);
            }
            else
            {
                GameObjectUtility.SetParentAndAlign(prefabInstance, parent);
            }

            prefabInstance.name = prefabInstance.name.Substring(0, prefabInstance.name.Length - "(Clone)".Length); //Removing 'clone' suffix
            Selection.activeGameObject = prefabInstance.gameObject;
            EditorExtentions.MarkActiveSceneAsDirty();

            return prefabInstance;
        }

//        public static void AddUIKitComponent<T>() where T : _UIKitComponent
//        {
//            if (!Instance.Templates.ContainsKey(typeof(T)))
//            {
//                Debug.LogWarning("Unknown UIKitComponent of type: " + typeof(T).Name);
//                return;
//            }
//            var prefabInstance = Instantiate(Instance.Templates[typeof(T)]);
//            if (prefabInstance == null)
//            {
//                Debug.LogError(String.Format("Unable to instantiate {0}!", typeof(T).Name));
//                return;
//            }
//            var uiKitComponent = prefabInstance as T;
//            if (uiKitComponent == null)
//            {
//                Debug.LogError(String.Format("Instantiated prefab has type {0}, expected type: {1}. Instantiation canceled.", prefabInstance.GetType().Name, typeof(T).Name));
//                Destroy(prefabInstance);
//                return;
//            }
//            if (Selection.activeGameObject != null)
//                GameObjectUtility.SetParentAndAlign(uiKitComponent.gameObject, Selection.activeGameObject);
//
//            uiKitComponent.name = uiKitComponent.name.Substring(0, uiKitComponent.name.Length - "(Clone)".Length); //Removing 'clone' suffix
//            Selection.activeGameObject = uiKitComponent.gameObject;
//            EditorExtentions.MarkActiveSceneAsDirty();
//        }

        //[MenuItem("GameObject/UIKit/Helper/Show children", false, 100)]
        //public static void ShowChildren()
        //{
        //    if (Selection.activeGameObject != null)
        //    {
        //        var component = Selection.activeGameObject.GetComponent<UIKitComponent>();
        //        if (component == null)
        //            return;

        //        component.ShowChildren(true);
        //    }
        //}

        //[MenuItem("GameObject/UIKit/Helper/Hide children", false, 101)]
        //public static void HideChildren()
        //{
        //    if (Selection.activeGameObject != null)
        //    {
        //        var component = Selection.activeGameObject.GetComponent<UIKitComponent>();
        //        if (component == null)
        //            return;

        //        component.ShowChildren(false);
        //    }
        //}
#endif
    }
}
