using UnityEngine;
using System.Collections;
using System;

namespace GameUI
{
    public class BaseIntegerStyleObject : BaseStyleObject<int>
    {
        // ---

        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/UI/BaseIntegerStyleObject")]
        public static void CreateAsset()
        {
            ScriptableObjectAsset.CreateAsset<BaseIntegerStyleObject>();
        }
        #endif
    }
}