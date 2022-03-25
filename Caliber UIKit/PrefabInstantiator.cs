using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameClient.Plugins.UIKit.Scripts
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PrefabInstantiatorIgnoreAttribute : Attribute
    {
    }

    public class PrefabInstantiator : MonoBehaviour
    {
        private static readonly Dictionary<Type, List<(FieldInfo, GameObject)>> PrefabsAll = new Dictionary<Type, List<(FieldInfo, GameObject)>>();
        private List<(FieldInfo, GameObject)> _prefabs;

        private void Awake()
        {
            var type = GetType();
            if (PrefabsAll.TryGetValue(type, out var prefabs))
            {
                _prefabs = prefabs;
            }
            else
            {
                _prefabs = new List<(FieldInfo, GameObject)>();
                foreach (var field in type.GetFields())
                {
                    var valuePrefab = field.GetValue(this);
                    if (valuePrefab is MonoBehaviour prefab && string.IsNullOrEmpty(prefab.gameObject.scene.name) && field.GetCustomAttribute<PrefabInstantiatorIgnoreAttribute>() == null)
                    {
                        _prefabs.Add((field, prefab.gameObject));
                    }
                }
                PrefabsAll[type] = _prefabs;
            }
        }

        private void OnEnable()
        {
            foreach (var pair in _prefabs)
            {
                var component = Instantiate(pair.Item2, transform).GetComponent(pair.Item1.FieldType);
                pair.Item1.SetValue(this, component);
            }
        }

        private void OnDisable()
        {
            foreach (var pair in _prefabs)
            {
                var component = (Component) pair.Item1.GetValue(this);
                Destroy(component.gameObject);

                pair.Item1.SetValue(this, pair.Item2.GetComponent(pair.Item1.FieldType));
            }
        }
    }
}