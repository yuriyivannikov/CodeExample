using System;
using Assets.UI.Colors;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameUI
{
    [ExecuteInEditMode]
    public class FontStyleParameters : ScriptableObject
    {
        [Serializable]
        public class Use<T>
        {
            public bool Override;
            public T Value;
        }

        [Serializable]
        public class UseFont : Use<TMP_FontAsset> {}
        [Serializable]
        public class UseMaterial : Use<Material> {}
        [Serializable]
        public class UseInt : Use<int> {}
        [Serializable]
        public class UseFloat : Use<float> {}
        [Serializable]
        public class UseColor : Use<ColorLibraryItem> {}
        
        [SerializeField]
        private FontStyleParameters _parent;
        
        [SerializeField]
        private UseFont _font;
        
        [SerializeField]
        private UseMaterial _material;
        
        [SerializeField]
        private UseInt _size;
        
        [SerializeField]
        private UseFloat _lineSpacing;
        
        [SerializeField]
        private UseFloat _letterSpacing;
        
        [SerializeField]
        private UseFloat _paragraphSpacing;
        
        [SerializeField]
        private UseColor _color;
        
        // ----------------------------------
        
        public TMP_FontAsset GetFont => _parent == null || this == _parent || _font.Override ? _font.Value : _parent.GetFont;
        public Material GetMaterial => _parent == null || this == _parent || _material.Override ? _material.Value : _parent.GetMaterial;
        public int GetSize => _parent == null || this == _parent || _size.Override ? _size.Value : _parent.GetSize;
        public float GetLineSpacing => _parent == null || this == _parent || _lineSpacing.Override ? _lineSpacing.Value : _parent.GetLineSpacing;
        public float GetLetterSpacing => _parent == null || this == _parent || _letterSpacing.Override ? _letterSpacing.Value : _parent.GetLetterSpacing;
        public float GetParagraphSpacing => _parent == null || this == _parent || _paragraphSpacing.Override ? _paragraphSpacing.Value : _parent.GetParagraphSpacing;
        public Color GetColor => _parent == null || this == _parent || _color.Override ? _color.Value : _parent.GetColor;


#if UNITY_EDITOR
        [MenuItem("Assets/Create/UI/FontStyleParameters")]
        public static void CreateAsset()
        {
            ScriptableObjectAsset.CreateAsset<FontStyleParameters>();
        }
#endif
    }
}