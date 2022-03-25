using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIKit
{
    /// <summary>
    /// Является копией класса UnityEngine.UI.Navigation с небольшими правками
    /// https://bitbucket.org/Unity-Technologies/ui/src/f0c70f707cf09f959ad417049cb070f8e296ffe2/UnityEngine.UI/UI/Core/Navigation.cs?at=5.5&fileviewer=file-view-default
    /// </summary>

    [Serializable]
    public struct UIKitNavigation
    {
        /*
         * This looks like it's not flags, but it is flags,
         * the reason is that Automatic is considered horizontal
         * and verical mode combined
         */
        [Flags]
        public enum Mode
        {
            None = 0, // No navigation
            Horizontal = 1, // Automatic horizontal navigation
            Vertical = 2, // Automatic vertical navigation
            Automatic = 3, // Automatic navigation in both dimensions
            Explicit = 4, // Explicitly specified only
        }

        // Which method of navigation will be used.
        [FormerlySerializedAs("mode")]
        [SerializeField]
        private Mode m_Mode;

        // Game object selected when the joystick moves up. Used when navigation is set to "Explicit".
        [FormerlySerializedAs("selectOnUp")]
        [SerializeField]
        private UIKitSelectable m_SelectOnUp;

        // Game object selected when the joystick moves down. Used when navigation is set to "Explicit".
        [FormerlySerializedAs("selectOnDown")]
        [SerializeField]
        private UIKitSelectable m_SelectOnDown;

        // Game object selected when the joystick moves left. Used when navigation is set to "Explicit".
        [FormerlySerializedAs("selectOnLeft")]
        [SerializeField]
        private UIKitSelectable m_SelectOnLeft;

        // Game object selected when the joystick moves right. Used when navigation is set to "Explicit".
        [FormerlySerializedAs("selectOnRight")]
        [SerializeField]
        private UIKitSelectable m_SelectOnRight;

        public Mode mode { get { return m_Mode; } set { m_Mode = value; } }
        public UIKitSelectable selectOnUp { get { return m_SelectOnUp; } set { m_SelectOnUp = value; } }
        public UIKitSelectable selectOnDown { get { return m_SelectOnDown; } set { m_SelectOnDown = value; } }
        public UIKitSelectable selectOnLeft { get { return m_SelectOnLeft; } set { m_SelectOnLeft = value; } }
        public UIKitSelectable selectOnRight { get { return m_SelectOnRight; } set { m_SelectOnRight = value; } }

        static public UIKitNavigation defaultNavigation
        {
            get
            {
                var defaultNav = new UIKitNavigation();
                defaultNav.m_Mode = Mode.Automatic;
                return defaultNav;
            }
        }
    }
}