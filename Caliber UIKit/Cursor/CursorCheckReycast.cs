using UnityEngine;

namespace UIKit
{
    public abstract class CursorCheckReycast : MonoBehaviour
    {
        public virtual bool Check()
        {
            return true;
        }
    }
}
