using UnityEditor;
using UnityEngine;

namespace Assets.UI.Colors
{
    [CustomPropertyDrawer(typeof(ColorLibraryItem))]
    public class ColorLibraryItemDrawer : PropertyDrawer
    {
        private const float SizeItem = 24f;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty visibleProperty = property.FindPropertyRelative("_visible");
            if (!visibleProperty.boolValue)
            {
                return 16f;
            }
            float height = 16f + 20f;

            string colorID = property.FindPropertyRelative("_colorID").stringValue;
            ColorLibrary.Palette palette = ColorLibrary.GetPaletteByColorID(colorID);
            if (palette != null)
            {
                int colCount = (int)((Screen.width - 19) / (SizeItem + 2f));
                int rowCount = palette.Colors.Count / colCount + (palette.Colors.Count % colCount > 0 ? 1 : 0);
                height += rowCount * (SizeItem + 2f) + 12;
            }
            return height;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect contentPosition = position;
            contentPosition.height = 16f;
            contentPosition.width = EditorGUIUtility.labelWidth;
            SerializedProperty visibleProperty = property.FindPropertyRelative("_visible");
            visibleProperty.boolValue = EditorGUI.Foldout(contentPosition, visibleProperty.boolValue, label);

            string colorID = property.FindPropertyRelative("_colorID").stringValue;
            ColorLibrary.Palette palette = ColorLibrary.GetPaletteByColorID(colorID);
            string paletteName = palette != null ? palette.Name : "";
            ColorLibrary.Color color = palette != null ? palette.GetColorByID(colorID) : null;

            /*
            ColorLibrary.ElementData ColorLibraryData = null;
            if (library != null)
            {
                ColorLibraryData = library[property.FindPropertyRelative("_colorID").stringValue];
            }
            */
            contentPosition.x = EditorGUIUtility.labelWidth + 15;
            
            if (palette == null)
            {
                contentPosition.width = Screen.width - contentPosition.x - 16;
                SerializedProperty colorProperty = property.FindPropertyRelative("_color");
                if (colorProperty != null)
                {
                    Color newColor = EditorGUI.ColorField(contentPosition, colorProperty.colorValue);
                    if (colorProperty.colorValue != newColor)
                    {
                        colorProperty.colorValue = newColor;
                        colorProperty.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            else
            {
                Color c = color != null ? color.GetColor : Color.white;
                c.a *= property.FindPropertyRelative("_alpha").floatValue;
                contentPosition.width = contentPosition.height;
                EditorGUIUtility.DrawColorSwatch(contentPosition, c);
                
                contentPosition.x += contentPosition.width + 4;
                contentPosition.width = Screen.width - 8 - contentPosition.x - 68;
    
                EditorGUI.Slider(contentPosition, property.FindPropertyRelative("_alpha"), 0f, 1f, GUIContent.none);
    
                contentPosition.x += contentPosition.width + 4;
                contentPosition.width = 48;
                
                if (EditorGUI.DropdownButton(contentPosition, new GUIContent("Clear"), FocusType.Keyboard))
                {
                    property.FindPropertyRelative("_colorID").stringValue = "";
                    property.FindPropertyRelative("_alpha").floatValue = 1.0f;
                    visibleProperty.boolValue = false;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            if (visibleProperty.boolValue)
            {
                EditorGUI.BeginProperty(position, GUIContent.none, property);
                contentPosition = position;
                contentPosition.height = 16f;
                contentPosition.y += 18f;
                
                EditorGUI.LabelField(new Rect(contentPosition.x, contentPosition.y, EditorGUIUtility.labelWidth, contentPosition.height), new GUIContent("Palette"));
                
                if (EditorGUI.DropdownButton(new Rect(contentPosition.x + EditorGUIUtility.labelWidth, contentPosition.y, contentPosition.width - EditorGUIUtility.labelWidth, contentPosition.height), new GUIContent(paletteName), FocusType.Keyboard))
                {
                    var menu = new GenericMenu();
                    foreach (var p in ColorLibrary.Instance.Palettes)
                    {
                        if (p.Colors.Count > 0)
                        {
                            menu.AddItem(new GUIContent(p.Name), palette == p, () =>
                            {
                                property.FindPropertyRelative("_colorID").stringValue = p.Colors[0].ID;
                                property.serializedObject.ApplyModifiedProperties();
                            });
                        }
                    }
                    menu.ShowAsContext();
                }
                
                contentPosition.y += 18f;
                contentPosition.x = position.x + 16f;
                if (palette != null)
                {
                    int colCount = (int)((Screen.width - 19) / (SizeItem + 2f));
                    for (int i = 0; i < palette.Colors.Count; i++)
                    {
                        /*
                        if (library.ElementList[i].Type == ColorLibrary.ElementType.Separator)
                        {
                            continue;
                        }
                        */
                        Rect rect = new Rect(contentPosition.x + (i % colCount) * (SizeItem + 2), contentPosition.y + 2 + (int)(i / colCount) * (SizeItem + 2), SizeItem, SizeItem);
                        if (colorID == palette.Colors[i].ID)
                        {
                            EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), Color.black);
                            EditorGUIUtility.DrawColorSwatch(rect, palette.Colors[i].GetColor);
                        }
                        else
                        {
                            EditorGUIUtility.DrawColorSwatch(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10), palette.Colors[i].GetColor);
                        }
                        if (GUI.Button(rect, new GUIContent("", palette.Colors[i].Name), new GUIStyle()))
                        {
                            property.FindPropertyRelative("_colorID").stringValue = palette.Colors[i].ID;
                        }
                    }
                }
                EditorGUI.EndProperty();
                
            }

        }
    }
}

