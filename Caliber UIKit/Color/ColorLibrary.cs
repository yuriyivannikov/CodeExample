using System;
using System.Collections.Generic;
using System.Linq;
using Assets.System.Scripts;
using TheraBytes.BetterUi;
using UIKit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace Assets.UI.Colors
{
    public class ColorLibrary : SingletonSerializedScriptableAsset<ColorLibrary>
    {
        public enum ColorblindTypes
        {
            None = 0,
            Special = 1,
            Alternative = 2
        }

        public static ColorblindTypes ColorblindType = ColorblindTypes.None;

        public struct ColorData
        {
            public string PaletteName;
            public string ColorName;

            public ColorData(string paletteName, string colorName)
            {
                PaletteName = paletteName;
                ColorName = colorName;
            }
        }

        public static class PaletteName
        {
            public const string Main = "Main";
            public const string Currencies = "Currencies";
        }
        
        public static readonly ColorData ColorMainAccent = new ColorData(PaletteName.Main, "Accent");
        public static readonly ColorData ColorMainBlack = new ColorData(PaletteName.Main, "Black");
        public static readonly ColorData ColorMainWhite = new ColorData(PaletteName.Main, "White");
        
        public static readonly ColorData ColorCurrenciesSc = new ColorData(PaletteName.Currencies, "sc");
        public static readonly ColorData ColorCurrenciesHc = new ColorData(PaletteName.Currencies, "hc");
        public static readonly ColorData ColorCurrenciesTokens = new ColorData(PaletteName.Currencies, "tokens");
        public static readonly ColorData ColorCurrenciesFreeXp = new ColorData(PaletteName.Currencies, "free_xp");

        [Serializable]
        public class Color
        {
            [SerializeField] //[HideInInspector]
            private string _id;
            public string ID => _id;

            [SerializeField]
            private bool _isColorblind = false;
            
            [SerializeField]
            private string _name = "";
            public string Name => _name;

            [SerializeField]
            private UnityEngine.Color _color = UnityEngine.Color.white;

            [SerializeField]
            private UnityEngine.Color _specialColor = UnityEngine.Color.white;

            [SerializeField]
            private UnityEngine.Color _alternativeColor = UnityEngine.Color.white;

            public Color()
            {
                _id = Guid.NewGuid().ToString();
                //guid может не сохраниться в _id
            }

            public UnityEngine.Color GetColor
            {
                get
                {
                    if (_isColorblind)
                        switch (ColorblindType)
                        {
                            case ColorblindTypes.Special:
                                return _specialColor;
                            case ColorblindTypes.Alternative:
                                return _alternativeColor;
                        }
                    return _color;
                }
            }
            
            public void SetColor(UnityEngine.Color color)
            {
                _color = color;
            }
        }

        [Serializable]
        public class Palette
        {
            [SerializeField]
            private string _name;
            public string Name => _name;

            [SerializeField]
            private List<Color> _colors = new List<Color>();
            public List<Color> Colors => _colors;

            public Color this[string name]
            {
                get
                {
                    for (var i = 0; i < _colors.Count; i++)
                    {
                        var e = _colors[i];
                        if (e.Name == name)
                            return e;
                    }
                    return null;
                }
            }

            public Color GetColorByID(string id)
            {
                for (var i = 0; i < _colors.Count; i++)
                {
                    var e = _colors[i];
                    if (e.ID == id) return e;
                }
                return null;
            }

#if UNITY_EDITOR
            private void DrawAddButton()
            {
                _colors.Add(new Color());
            }
#endif
        }

        [SerializeField]
        private List<Palette> _palettes;
        public List<Palette> Palettes => _palettes;

        public static UnityEngine.Color GetColor(ColorData colorData)
        {
            return GetColor(colorData.PaletteName, colorData.ColorName);
        }

        public static UnityEngine.Color GetColor(string paletteName, string name)
        {
            var palette = Instance[paletteName];
            if (palette != null)
            {
                var color = palette[name];
                if (color != null)
                    return color.GetColor;

                color = palette["Default"];
                if (color != null)
                    return color.GetColor;
            }
            return UnityEngine.Color.white;
        }
        
        public static Color GetLibraryColor(ColorData colorData)
        {
            return GetLibraryColor(colorData.PaletteName, colorData.ColorName);
        }

        public static Color GetLibraryColor(string paletteName, string name)
        {
            var palette = Instance[paletteName];
            return palette != null ? palette[name] : null;
        }

        public Palette this[string name]
        {
            get
            {
                for (var i = 0; i < Instance._palettes.Count; i++)
                {
                    var e = Instance._palettes[i];
                    if (e.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return e;
                }
                return null;
            }
        }

        public static Palette GetPaletteByColorID(string id)
        {
            for (var i = 0; i < Instance._palettes.Count; i++)
            {
                var palette = Instance._palettes[i];
                for (var j = 0; j < palette.Colors.Count; j++)
                {
                    var color = palette.Colors[j];
                    if (color.ID == id)
                        return palette;
                }
            }
            return null;
        }

        public static Color GetColorByID(string id)
        {
            for (var i = 0; i < Instance._palettes.Count; i++)
            {
                var palette = Instance._palettes[i];
                for (var j = 0; j < palette.Colors.Count; j++)
                {
                    var color = palette.Colors[j];
                    if (color.ID == id)
                        return color;
                }
            }
            return null;
        }

#if UNITY_EDITOR
//        public void OnValidate()
//        {
//            UpdateAllColor();
//        }
#endif

        public static event Action CallbackUpdateAllColor;
        
#if UNITY_EDITOR
        public static void UpdateAllColor()
        {
            EditorExtentions.ExecuteActionForObjects<Graphic>(EditorExtentions.IsSceneObject, obj =>
            {
                if (obj != null)
                {
                    obj.SetVerticesDirty();
                    obj.SetMaterialDirty();
                }
            });
        
            EditorExtentions.ExecuteActionForObjects<UIKitSelectable>(EditorExtentions.IsSceneObject, obj =>
            {
                if (obj != null)
                {
                    obj.ActivateColors();
                }
            });
        
            EditorExtentions.ExecuteActionForObjects<TextStyleComponent>(EditorExtentions.IsSceneObject, obj =>
            {
                if (obj != null)
                {
                    obj.UpdateStyle();
                }
            });
            
            CallbackUpdateAllColor?.Invoke();
        }
#else
        public static void UpdateAllColor()
        {
            Debug.LogWarning("UpdateAllColor");

            ExecuteActionForObjects<BetterImage>(obj =>
            {
                if (obj != null)
                {
                    obj.ColorLibraryItem.UpdateColor();

                    obj.SetVerticesDirty();
                    obj.SetMaterialDirty();
                }
            });
            
            ExecuteActionForObjects<UIKitSelectable>(obj =>
            {
                if (obj != null)
                {
                    obj.ActivateColors();
                }
            });

            ExecuteActionForObjects<TextStyleComponent>(obj =>
            {
                if (obj != null)
                {
                    obj.ColorLibraryItem.UpdateColor();
                    obj.UpdateStyle();
                }
            });
            
            CallbackUpdateAllColor?.Invoke();
        }
        
        private static void ExecuteActionForObjects<T>(Action<T> action) where T : UnityEngine.Object
        {
            foreach (var obj in FindObjectsOfType<T>(true).OfType<T>())
            {
                action?.Invoke(obj);
            }
        }
#endif

#if UNITY_EDITOR
        [MenuItem("Assets/Create/Configs/UI/ColorLibrary")]
        public static void CreateAsset()
        {
            ScriptableObjectAsset.CreateAsset<ColorLibrary>();
        }
#endif
    }
}
