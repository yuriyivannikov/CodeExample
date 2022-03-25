using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.UI.Editor
{
    public class CustomControls
    {
        public class CustomControlInfo
        {
            public Single Width { get; set; }
            public Single Height { get; set; }
        }

        public class ButtonInfo : CustomControlInfo
        {
            public Boolean IsPressed { get; set; }
        }

        private const Single CharacterSpace = 8f;

        public static Single Slider(Single value, IEnumerable<Single> availableValues, params GUILayoutOption[] options)
        {
            var values = availableValues.OrderBy(t => t).ToList();
            var selectedValue = EditorGUILayout.Slider(value, values[0], values[values.Count-1], options);
            var result = selectedValue;
            foreach (var availableValue in values)
            {
                if (selectedValue >= availableValue)
                {
                    result = availableValue;
                }
            }
            return result;
        }

        public static Int32 ToggleGroup(Vector2 position, Single height, String[] titles, Int32 selectedIndex, Single space)
        {
            var result = -1;
            for (var i = 0; i < titles.Length; i++)
            {
                var buttonSize = new Vector2(titles[i].Length*CharacterSpace + CharacterSpace*2, height);
                if (GUI.Toggle(new Rect(position, buttonSize), i==selectedIndex, titles[i], "Button"))
                {
                    result = i;
                    selectedIndex = i;
                }
                position += new Vector2(space + buttonSize.x, 0);
            }
            return result;
        }

        public static CustomControlInfo Label(Vector2 position, Single height, String title)
        {
            var width = title.Length * CharacterSpace + CharacterSpace * 2;
            return new ButtonInfo
            {
                Height = height,
                Width = width,
                IsPressed = GUI.Button(new Rect(position, new Vector2(width, height)), title)
            };
        }

        public static ButtonInfo Button(Vector2 position, Single height, String title)
        {
            var width = title.Length*CharacterSpace + CharacterSpace*2;
            return new ButtonInfo
            {
                Height = height,
                Width = width,
                IsPressed = GUI.Button(new Rect(position, new Vector2(width, height)), title)
            };
        }

        public static ButtonInfo ToggleButton(Vector2 position, Boolean isSelected, Single height, String title)
        {
            var width = title.Length * CharacterSpace + CharacterSpace * 2;
            return new ButtonInfo
            {
                Height = height,
                Width = width,
                IsPressed = GUI.Toggle(new Rect(position, new Vector2(width, height)), isSelected, title, "Button")
            };
        }

        public static Int32 ComboBox(Rect rect, Int32 selectedIndex, String[] displayedOptions, String label = null)
        {
            if (selectedIndex < 0 || selectedIndex >= displayedOptions.Length)
            {
                var brokenIndex = selectedIndex;
                Debug.LogError("Unexpected ComboBox selectedIndex: " + brokenIndex);
                var options = new String[displayedOptions.Length + 1];
                options[0] = " ";
                displayedOptions.CopyTo(options, 1);
                selectedIndex = label == null
                    ? EditorGUI.Popup(rect, 0, options)
                    : EditorGUI.Popup(rect, label, 0, options);
                return selectedIndex == 0 ? brokenIndex : selectedIndex - 1;
            }

            return label == null
                ? EditorGUI.Popup(rect, selectedIndex, displayedOptions)
                : EditorGUI.Popup(rect, label, selectedIndex, displayedOptions);
        }

        public static Int32 ComboBox(Rect rect, Int32 selectedIndex, IEnumerable<String> displayedOptions, String label = null)
        {
            return ComboBox(rect, selectedIndex, displayedOptions.ToArray(), label);
        }

        /// <summary>
        /// Combobox used to display enum values
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="rect"></param>
        /// <param name="enumValue"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static T ComboBox<T>(Rect rect, Enum enumValue, String label = null)
        {
            return (T)Enum.ToObject(typeof(T), ComboBox(rect, Convert.ToInt32(enumValue), Enum.GetNames(typeof(T)), label));
        }

        public static T ObjectField<T>(Rect rect, T obj, Boolean allowSceneObjects) where T : UnityEngine.Object
        {
            return (T)EditorGUI.ObjectField(rect, obj, typeof(T), allowSceneObjects);
        }

        public static T ObjectField<T>(Rect rect, String label, T obj, Boolean allowSceneObjects) where T : UnityEngine.Object
        {
            return (T)EditorGUI.ObjectField(rect, label, obj, typeof(T), allowSceneObjects);
        }

        public static T ObjectField<T>(String label, T obj, Boolean allowSceneObjects, params GUILayoutOption[] options) where T : UnityEngine.Object
        {
            return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), allowSceneObjects, options);
        }

        public static T ObjectField<T>(T obj, Boolean allowSceneObjects, params GUILayoutOption[] options) where T : UnityEngine.Object
        {
            return (T)EditorGUILayout.ObjectField(obj, typeof(T), allowSceneObjects, options);
        }
    }
}