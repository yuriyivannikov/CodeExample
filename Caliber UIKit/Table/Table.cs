using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIKit
{
    public abstract class Table<T> : MonoBehaviour where T: TableItem
    {
        [SerializeField]
        protected Transform _parentTransform = null;

        [SerializeField]
        protected T _itemTemplate;

        private readonly Dictionary<T, object> _content = new Dictionary<T, object>();

        public int Count { get { return _content.Count; } }

        protected virtual void Start()
        {
            if (_itemTemplate != null)
            {
                if (_itemTemplate.transform.parent != null)
                {
                    _itemTemplate.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("Error: itemTemplate not found");
            }
        }

        public virtual void AddData(IEnumerable<object> dataObjects)
        {
            foreach (var itemData in dataObjects)
            {
                AddItem(itemData);
            }
        }

        public void UpdateData(IEnumerable<object> dataObjects)
        {
            var list = dataObjects.ToList();
            
            for (int i = list.Count; i < _content.Count; i++)
            {
                var item = _content.ElementAt(i);
                _content.Remove(item.Key);
                Destroy(item.Key);
            }
            
            int index = 0;
            foreach (var itemData in dataObjects)
            {
                if (index < _content.Count)
                {
                    var item = _content.ElementAt(index);
                    item.Key.SetData(itemData);
                    _content[item.Key] = itemData;
                }
                else
                {
                    AddItem(itemData);
                }
                index++;
            }
        }

        public virtual T GetItemTemplate(object itemData)
        {
            return _itemTemplate;
        }

        public virtual T AddItem(object itemData)
        {
            if (itemData != null && _content.ContainsValue(itemData))
            {
                return null;
            }

            T itemTemplate = GetItemTemplate(itemData);
            if (itemTemplate == null)
            {
                Debug.LogError("Error: " + gameObject.name + " itemTemplate not found");
                return null;
            }
            
            var item = Instantiate(itemTemplate);
            item.gameObject.SetActive(false);
            item.gameObject.name = string.Format("{0}{1}", item.gameObject.name, _content.Count);
            item.transform.SetParent(_parentTransform != null ? _parentTransform : transform, false);
            _content.Add(item, itemData);
            item.SetData(itemData);
            item.gameObject.SetActive(true);
            return item;
        }

        public virtual void Clear()
        {
            foreach (var item in _content.Keys)
            {
                Destroy(item.gameObject);
            }

            _content.Clear();
        }

        public List<T> GetAllItems()
        {
            return _content.Keys.ToList();
        }
    }
}