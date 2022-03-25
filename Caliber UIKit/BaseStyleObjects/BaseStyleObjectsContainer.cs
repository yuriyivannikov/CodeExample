using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameUI
{
    public class BaseStyleObjectsContainer : ScriptableObject
    {
        [SerializeField]
        private List<ScriptableObject> baseStyleObjects = new List<ScriptableObject>();

        public string[] GetNames()
        {
            List<string> listNames = new List<string>();

            foreach (ScriptableObject scriptableObject in baseStyleObjects)
            {
                if (scriptableObject != null)
                    listNames.Add(scriptableObject.name);
            }

            return listNames.ToArray();
        }

        public string[] GetNamesWithIntegerValues()
        {
            List<string> listNames = new List<string>();

            int value;
            int counter = 0;

            foreach (ScriptableObject scriptableObject in baseStyleObjects)
            {
                if (scriptableObject != null)
                {
                    value = ((BaseIntegerStyleObject)GetBaseStyleObjectByIndex(counter)).value;

                    listNames.Add(string.Format("{0} [{1}]", scriptableObject.name, value));
                }

                counter++;
            }

            return listNames.ToArray();
        }

        public string[] GetNamesWithFloatValues()
        {
            List<string> listNames = new List<string>();

            float value;
            int counter = 0;

            foreach (ScriptableObject scriptableObject in baseStyleObjects)
            {
                if (scriptableObject != null)
                {
                    value = ((BaseStyleObject<float>)GetBaseStyleObjectByIndex(counter)).value;

                    listNames.Add(string.Format("{0} [{1}]", scriptableObject.name, value));
                }

                counter++;
            }

            return listNames.ToArray();
        }

        public ScriptableObject GetBaseStyleObjectByIndex(int index)
        {
            if (index >= baseStyleObjects.Count)
            {
                if (baseStyleObjects.Count == 0)
                {
                    Debug.Log("Error: BaseStyleObjectsContainer is Empty");
                    return null;
                }
               
                Debug.Log(String.Format("Error: BaseStyleObjectsContainer not contain index: {0}", index));
                index = 0;
            }

            if (baseStyleObjects[index] == null)
            {
                Debug.Log(String.Format("Error: BaseStyleObjectsContainer not contain index: {0}", index));
                return null;
            }

            return baseStyleObjects[index];
        }

        public IEnumerable<ScriptableObject> GetBaseStyleObjects()
        {
            return baseStyleObjects;
        }

        // ---

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/UI/BaseStyleObjectsContainer")]
        public static void CreateAsset()
        {
            ScriptableObjectAsset.CreateAsset<BaseStyleObjectsContainer>();
        }
#endif
    }
}