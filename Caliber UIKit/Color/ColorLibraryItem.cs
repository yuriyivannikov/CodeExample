using System;
using UnityEngine;

namespace Assets.UI.Colors
{
    [Serializable]
    public class ColorLibraryItem
    {
#if !UNITY_EDITOR
        private Color? _realColor;
#endif
        public event Action Change;

        [SerializeField]
        private string _colorID;

        [SerializeField]
        private bool _visible = false;
        
        [SerializeField]
       // [Range(0f, 1f)]
        private Single _alpha = 1f;
        
        [SerializeField]
        private Color _color = UnityEngine.Color.white;
        
        public ColorLibraryItem()
        {
            
        }

        public ColorLibraryItem(ColorLibraryItem colorLibraryItem)
        {
            Set(colorLibraryItem);
        }

        public void Set(ColorLibraryItem colorLibraryItem)
        {
            _colorID = colorLibraryItem._colorID;
            _alpha = colorLibraryItem._alpha;
        }

        public void Set(Color color)
        {
            _colorID = null;
            _color = color;
#if !UNITY_EDITOR
                _realColor = _color;
#endif
            Change?.Invoke();
        }

        public void Set(ColorLibrary.Color color)
        {
            _colorID = color.ID;
            _color = color.GetColor;
#if !UNITY_EDITOR
            _realColor = _color;
#endif
            Change?.Invoke();
        }

        private Color Get()
        {
            Color color = _color;
            if (_colorID != "")
            {
                ColorLibrary.Color colorData = ColorLibrary.GetColorByID(_colorID);
                if (colorData != null)
                    color = colorData.GetColor;
                
                color.a *= _alpha;
            }
            return color;
        }

        public Color Color
        {
            get
            {
#if UNITY_EDITOR
                return Get();
#else
                if (!_realColor.HasValue)
                {
                    _realColor = Get(); 
                }
                return _realColor.Value;
#endif
            }
            set
            {
                _colorID = "";
                _alpha = 1.0f;
                _color = value;
            }
        }

        public void UpdateColor()
        {
#if !UNITY_EDITOR
            var colorData = ColorLibrary.GetColorByID(_colorID);
            if (colorData != null && !_realColor.Equals(colorData))
            {
                Set(colorData);
            }
#endif
        }

        public static implicit operator Color(ColorLibraryItem colorLibraryItem)
        {
            return colorLibraryItem != null ? colorLibraryItem.Color : Color.white;
        }
    }
}
