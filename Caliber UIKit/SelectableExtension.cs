using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public static class SelectableExtension
{
    private static GameObject _inputGameObject;
    private static TMP_InputField _inputField;
    
    public static bool IsInputFocus()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
            return false;
        if (_inputGameObject != EventSystem.current.currentSelectedGameObject)
        {
            _inputGameObject = EventSystem.current.currentSelectedGameObject;
            _inputField = _inputGameObject.GetComponent<TMP_InputField>();
        }
        return _inputField != null;// && _inputField.isFocused;
    }
}