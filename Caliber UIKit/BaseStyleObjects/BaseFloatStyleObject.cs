using UnityEngine;
using System.Collections;
using System;

namespace GameUI
{
    public class BaseFloatStyleObject : BaseStyleObject<float>
    {
        // ---

        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/UI/BaseFloatStyleObject")]
        public static void CreateAsset()
        {
            ScriptableObjectAsset.CreateAsset<BaseFloatStyleObject>();
        }
        #endif
    }
}