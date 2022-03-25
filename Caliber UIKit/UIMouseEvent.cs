using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIKit
{
    public class UIMouseEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action Enter;
        public event Action Exit;
        
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            Enter?.Invoke();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            Exit?.Invoke();
        }
    }
}
