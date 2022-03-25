using System.Collections.Generic;

namespace UIKit
{
    // <summary>
    /// Является копией класса UnityEngine.UI.ListPool с небольшими правками
    /// https://bitbucket.org/Unity-Technologies/ui/src/f0c70f707cf09f959ad417049cb070f8e296ffe2/UnityEngine.UI/UI/Core/Utility/ListPool.cs?at=5.5&fileviewer=file-view-default
    /// </summary>

    internal static class UIKitListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly UIKitObjectPool<List<T>> s_ListPool = new UIKitObjectPool<List<T>>(null, l => l.Clear());

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }
}