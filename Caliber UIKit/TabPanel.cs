using System;
using System.Collections.Generic;
using TheraBytes.BetterUi;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameClient.UI.Common
{
    public class TabPanel : MonoBehaviour
    {
        public List<TabPanelItem> Tabs = new List<TabPanelItem>();
        public TabPanelItem SelectedItem { get; private set; }
        public event Action<TabPanelItem> TabChanged;

        private void Awake()
        {
            Reset();
        }
        
        private void OnEnable()
        {
            for (var i = 0; i < Tabs.Count; i++)
                Tabs[i].Button.Click += OnTabClick;
        }

        private void OnDisable()
        {
            for (var i = 0; i < Tabs.Count; i++)
                Tabs[i].Button.Click -= OnTabClick;
        }

        public void Reset()
        {
            OpenTab(Tabs[0]);
        }

        public void OpenTab(TabPanelItem item)
        {
            foreach (var tab in Tabs)
            {
                tab.DeactiveGroup.SetActive(tab != item);
                tab.ActiveGroup.SetActive(tab == item);
                tab.ContentGroup.SetActive(tab == item);
            }

            if (SelectedItem != item)
            {
                SelectedItem = item;
                TabChanged?.Invoke(SelectedItem);
            }
        }
        
        private void OnTabClick(BetterButton button, BaseEventData eventData)
        {
            OpenTab(Tabs.Find(e => e.Button == button));
        }
    }
}
