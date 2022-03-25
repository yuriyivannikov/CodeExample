using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIKit
{
    /// <summary>
    /// Является копией класса UnityEditor.UI.ToggleGroup с небольшими правками
    /// https://bitbucket.org/Unity-Technologies/ui/src/f0c70f707cf09f959ad417049cb070f8e296ffe2/UnityEngine.UI/UI/Core/ToggleGroup.cs?at=5.5&fileviewer=file-view-default
    /// </summary>

    //[AddComponentMenu("UI/Toggle Group", 32)]
    [DisallowMultipleComponent]
    public class UIKitToggleGroup : UIBehaviour
    {
        [SerializeField]
        private bool m_AllowSwitchOff = false;
        public bool allowSwitchOff { get { return m_AllowSwitchOff; } set { m_AllowSwitchOff = value; } }

        protected List<UIKitToggle> m_Toggles = new List<UIKitToggle>();
        
        protected UIKitToggleGroup()
        { }

        protected void ValidateToggleIsInGroup(UIKitToggle toggle)
        {
            if (toggle == null || !m_Toggles.Contains(toggle))
                throw new ArgumentException(string.Format("Toggle {0} is not part of ToggleGroup {1}", new object[] { toggle, this }));
        }

        public virtual void NotifyToggleOn(UIKitToggle toggle, bool sendCallback = true)
        {
            ValidateToggleIsInGroup(toggle);

            // disable all toggles in the group
            for (var i = 0; i < m_Toggles.Count; i++)
            {
                if (m_Toggles[i] == toggle)
                    continue;

                m_Toggles[i].isOn = false;
            }
        }

        public virtual void UnregisterToggle(UIKitToggle toggle)
        {
            if (m_Toggles.Contains(toggle))
                m_Toggles.Remove(toggle);
        }

        public virtual void RegisterToggle(UIKitToggle toggle)
        {
            if (!m_Toggles.Contains(toggle))
                m_Toggles.Add(toggle);
        }

        public bool AnyTogglesOn()
        {
            return m_Toggles.Find(x => x.isOn) != null;
        }

        public IEnumerable<UIKitToggle> ActiveToggles()
        {
            return m_Toggles.Where(x => x.isOn);
        }

        public void SetAllTogglesOff()
        {
            bool oldAllowSwitchOff = m_AllowSwitchOff;
            m_AllowSwitchOff = true;

            for (var i = 0; i < m_Toggles.Count; i++)
                m_Toggles[i].isOn = false;

            m_AllowSwitchOff = oldAllowSwitchOff;
        }
    }
}