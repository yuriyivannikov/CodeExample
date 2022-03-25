using System;
using UnityEngine;
using GameSystem;

namespace UIKit
{
    public class CursorManager : MonoSingleton<CursorManager>
    {    
        /*
        [SerializeField]
        private CursorsData _cursorsData;

        [SerializeField]
        private string _defaultName = "Default";
        */
        private GameObject _object = null;

        public static GameObject Object { get { return Instance._object; } }
        public static Action<GameObject> Change;
       
        public static bool _isLocked = false;
        /*
        private float _time = 0;
        private int _frame = 0;
        private bool _isMoveChange = true;

        private CursorsData.ElementData _cursor;
        private CursorsData.ElementDataCursorAnimation _cursorAnimation;

        private CursorsData.ElementData _cursorDefault;

        private Dictionary<Type, CursorsData.ElementData> _cursorDictionary = new Dictionary<Type, CursorsData.ElementData>();

        private PointerEventData _pointerData = new PointerEventData(EventSystem.current);

        public static void ApplyCursor(string cursorKey, bool isMoveChange = true)
        {
            Instance._isMoveChange = isMoveChange;
            CursorsData.ElementData element = Instance._cursorsData[cursorKey];
            if (element != null)
            {
                Instance.resetAnimation();
                Instance._cursor = element;
                Instance._cursorAnimation = element.Over;
            }
        }

        public static void SetDefaultCursor(string cursorKey = "Default")
        {
            Instance._cursorDefault = Instance._cursorsData[cursorKey];
            Instance.resetAnimation();
        }

        private void resetAnimation()
        {
            _time = 0;
            _frame = 0;
        }

        public void Init()
        {
            if (_cursorsData != null)
            {
                SetDefaultCursor();

                foreach (var element in _cursorsData.Data)
                {
                    foreach (string objectName in element.Value.Objects)
                    {
                        Type type = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .FirstOrDefault(t => t.Name == objectName);
                        if (type != null)
                            _cursorDictionary.Add(type, element.Value);
                    }
                }
            }
        }

#if UNITY_EDITOR

        private string GetObjectPath(Component obj)
        {
            string result = "";
            while (obj != null)
            {
                result = obj.name + (result != "" ? "/" : "") + result;
                obj = obj.transform.parent;
            }
            return result;
        }

#endif

        private CursorsData.ElementData FindCursor(Component component, out GameObject gameObject)
        {
            gameObject = null;
            while (component != null)
            {
                foreach (KeyValuePair<Type, CursorsData.ElementData> pair in _cursorDictionary)
                {
                    var obj = component.GetComponent(pair.Key);
                    if (obj != null)
                    {
                        CursorCheckReycast checkReycast = obj is CursorCheckReycast ? obj as CursorCheckReycast : obj.gameObject.GetComponent<CursorCheckReycast>();
                        if ((!(obj is UIKitSelectable) || (obj as UIKitSelectable).interactable) && (checkReycast == null || checkReycast.Check()))
                        {
                            gameObject = component.gameObject;
                            return (pair.Value != null) ? pair.Value : _cursorDefault;
                        }
                    }
                }
                component = component.transform.parent;
            }
            return _cursorDefault;
        }

        private void UpdateCursor()
        {
            if (_cursorAnimation.Sprites.Count > 0 && _cursorAnimation[_frame].Sprite != null)
                Cursor.SetCursor(_cursorAnimation[_frame].Sprite, _cursorAnimation[_frame].Hotspot, CursorMode.Auto);
        }

        void Update()
        {
            if (IsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                return;
            }

            if (_cursorDictionary.Count == 0)
                return;

            _pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_pointerData, results);

            CursorsData.ElementData cursor;
            GameObject gameObject = null;

            if (_isMoveChange)
            {
                cursor = _cursorDefault;
                if (results.Count > 0)
                    cursor = FindCursor(results[0].gameObject.transform, out gameObject);
            }
            else
                cursor = _cursor;
            
            if (!Input.GetMouseButton(0))
            {
                if (_cursor != cursor || Input.GetMouseButtonUp(0))
                {
                    resetAnimation();
                    _cursor = cursor;
                    _cursorAnimation = cursor.Over.Sprites.Count > 0 ? cursor.Over : _cursorDefault.Over;
                    UpdateCursor();
                }
            }
            else if (_cursor != null && _cursor == cursor && Input.GetMouseButtonDown(0))
            {
                if (_cursor.Press == null)
                {
                    Debug.LogErrorFormat("Error: Not Found Cursor.Press: {0}", _cursor);
                }
                else if (_cursor.Press.Sprites.Count > 0)
                {
                    resetAnimation();
                    _cursorAnimation = cursor.Press;
                    UpdateCursor();
                }
            }
            
            if (_cursorAnimation != null && _cursorAnimation.Sprites.Count > 1)
            {
                int newFrame = (int)(_time / (1 / _cursorAnimation.AnimationSpeed));
                if (newFrame != _frame && (_cursorAnimation.AnimationLoop || _time < _cursorAnimation.Sprites.Count / _cursorAnimation.AnimationSpeed))
                {
                    if (newFrame >= _cursorAnimation.Sprites.Count)
                    {
                        resetAnimation();
                    }
                    else
                    { 
                        _frame = newFrame;
                    }

                    UpdateCursor();
                }
                _time += Time.deltaTime;
            }

            if (Object != gameObject)
            {
                _object = gameObject;
                if (Change != null)
                {
                    Change(Object);
                }
            }
            
        }
        */
        
        public static bool IsLocked
        {
            get { return _isLocked; }
            set
            {
                _isLocked = value;

                Cursor.lockState = (_isLocked) ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !_isLocked;
            }
        }
    }
}