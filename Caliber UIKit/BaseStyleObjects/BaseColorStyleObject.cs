using UnityEngine;

namespace GameUI
{
    public class BaseColorStyleObject : BaseStyleObject<Color>
    {
        // ---

        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/UI/BaseColorStyleObject")]
        public static void CreateAsset()
        {
            ScriptableObjectAsset.CreateAsset<BaseColorStyleObject>();
        }
        #endif
    }
}