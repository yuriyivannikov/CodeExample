using System;
using Assets.System.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoubleClickHandlerComponent : MonoBehaviour, IPointerClickHandler
{
    /*
     * Обработчик двойного нажатия
     */
    
    public event Action DoubleClickEvent;
    
    private static GameObject _clickObject;
    private static float _clickTime;

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (_clickObject == eventData.pointerPress && Time.time < _clickTime)
            DoubleClickEvent?.Invoke();

        _clickObject = eventData.pointerPress;
        _clickTime = Time.time + 0.2f;
    }
}
