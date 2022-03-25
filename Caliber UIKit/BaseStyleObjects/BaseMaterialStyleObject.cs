using UnityEngine;
using System.Collections;
using System;

namespace GameUI
{
    public class BaseMaterialStyleObject : BaseStyleObject<Material>
    {

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/UI/BaseMaterialStyleObject")]
        public static void CreateAsset()
        {
            ScriptableObjectAsset.CreateAsset<BaseMaterialStyleObject>();
        }
#endif
    }
}