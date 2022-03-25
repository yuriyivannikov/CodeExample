using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.System.Scripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

namespace Assets.UI.Editor
{
    public abstract class Inspector<T> : UnityEditor.Editor where T : UnityEngine.Object
    {
        protected T Target { get; private set; }

        private Type LowestReflectedType { get; set; }

        private Dictionary<String, FieldInfo> _fieldInfos = new Dictionary<String, FieldInfo>();
        private Dictionary<String, PropertyInfo> _propertyInfos = new Dictionary<String, PropertyInfo>();
        private Dictionary<String, MethodInfo> _methodInfos = new Dictionary<String, MethodInfo>();

        protected virtual Boolean IsInvalidState { get { return Target == null; } }

        protected virtual void OnEnable()
        {
            Target = target as T;
            if (Target == null)
                return;

            LowestReflectedType = EvaluateLowestReflectedType(typeof (T));

            _fieldInfos = GetAllFields(typeof (T)).ToDictionary(fieldInfo => fieldInfo.Name);
            var tmp = GetAllProperties(typeof(T));
            _propertyInfos = tmp.ToDictionary(propertyInfo => propertyInfo.Name);
            _methodInfos = GetAllMethods(typeof (T)).ToDictionary(methodInfo => methodInfo.Name);
        }

        protected virtual void OnDisable()
        {
            
        }

        public override void OnInspectorGUI()
        {
            if (IsInvalidState)
            {
                DrawDefaultInspector();
                return;
            }
        }

        protected void MarkAsDirty()
        {
            if (EditorExtentions.IsSceneObject(Target))
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            EditorUtility.SetDirty(Target);
        }

        /// <summary>
        /// Evaluates first MonoBehaviour/ScriptableObject-derived class. If impossible to find such class, evaluation stops at UnityEngine.Object.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Type EvaluateLowestReflectedType(Type t)
        {
            return t.BaseType == typeof (MonoBehaviour) || t.BaseType == typeof(ScriptableObject) || t.BaseType == typeof(UnityEngine.Object) ? t : EvaluateLowestReflectedType(t.BaseType);
        }

        private IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            var typeFields = t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            return t == LowestReflectedType ? typeFields : typeFields.Concat(GetAllFields(t.BaseType));
        }

        private IEnumerable<PropertyInfo> GetAllProperties(Type t)
        {
            if (t == null)
                return Enumerable.Empty<PropertyInfo>();

            var typeProperties = t.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            return t == LowestReflectedType ? typeProperties : typeProperties.Concat(GetAllProperties(t.BaseType));
        }

        private IEnumerable<MethodInfo> GetAllMethods(Type t)
        {
            if (t == null)
                return Enumerable.Empty<MethodInfo>();

            var typeMethods = t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            return t == LowestReflectedType
                ? typeMethods
                : typeMethods.Concat(GetAllMethods(t.BaseType).Where(methodInfo => typeMethods.All(mi => mi.Name != methodInfo.Name)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TVariableType"></typeparam>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns>true if value was updated</returns>
        protected Boolean SetValue<TVariableType>(String variableName, TVariableType value, Object[] index = null)
        {
            if (_fieldInfos.ContainsKey(variableName))
            {
                var currentValue = _fieldInfos[variableName].GetValue(Target);
                if (Equals(currentValue, value))
                    return false;

                _fieldInfos[variableName].SetValue(Target, value);
            }
            else if (_propertyInfos.ContainsKey(variableName))
            {
                var currentValue = _propertyInfos[variableName].GetValue(Target, index);
                if (Equals(currentValue, value))
                    return false;

                _propertyInfos[variableName].SetValue(Target, value, index);
            }
            else
            {
                Debug.LogWarning("Unable to find " + variableName);
                return false;
            }
            
            if (EditorExtentions.IsSceneObject(Target) && !EditorApplication.isPlaying)
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            EditorUtility.SetDirty(Target);
            return true;
        }

        protected TVariableType GetValue<TVariableType>(String variableName, Boolean isNullOkay = false, Object[] index=null)
        {
            Object result = null;
            if (_fieldInfos.ContainsKey(variableName))
            {
                result = _fieldInfos[variableName].GetValue(Target);
            }
            else if (_propertyInfos.ContainsKey(variableName))
            {
                result = _propertyInfos[variableName].GetValue(Target, index);
            }
            else
            {
                Debug.LogWarning("Unable to find " + variableName);
            }

            if (result is TVariableType)
                return (TVariableType)result;

            if (!isNullOkay)
                Debug.LogError(String.Format("Expected type for {0} is {1}. Reflected type is {2}.", variableName,
                    typeof (TVariableType).Name, result == null ? "'Unable to get type, variable is null'" : result.GetType().Name));

            return default(TVariableType);
        }

        protected Boolean HasValue(String variableName)
        {
            return _fieldInfos.ContainsKey(variableName) || _propertyInfos.ContainsKey(variableName);
        }

        protected void ExecuteMethod(String methodName, params Object[] arguments)
        {
            if (_methodInfos.ContainsKey(methodName))
            {
                _methodInfos[methodName].Invoke(Target, arguments);
            }
        }
    }

    public abstract class Inspector<TInspectingType, TInspectorData> : Inspector<TInspectingType>
        where TInspectingType : UnityEngine.Object
        where TInspectorData : class, IInspectorData, new()
    {
        protected TInspectorData InspectorData { get; private set; }

        protected override Boolean IsInvalidState { get { return base.IsInvalidState && InspectorData == null; } }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (base.IsInvalidState)
                return;

            InspectorData = (TInspectorData)(GetValue<TInspectorData>("_inspectorData", true) ?? new TInspectorData()).Copy;
        }
        
        protected override void OnDisable()
        {
            if (InspectorData.Equals(GetValue<TInspectorData>("_inspectorData", true)))
                return;

            if (SetValue("_inspectorData", InspectorData))
            {
                if (EditorExtentions.IsSceneObject(Target))
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

                EditorUtility.SetDirty(Target);
            }
        }
    }

    public interface IInspectorData
    {
        IInspectorData Copy { get; }
    }
}