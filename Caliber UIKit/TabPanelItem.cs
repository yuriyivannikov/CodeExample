using System;
using System.Collections.Generic;
using TheraBytes.BetterUi;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameClient.UI.Common
{
    public class TabPanelItem : MonoBehaviour
    {
        public BetterButton Button;
        public GameObject DeactiveGroup;
        public GameObject ActiveGroup;
        public GameObject ContentGroup;
        public GameObject Indicator;
    }
}
