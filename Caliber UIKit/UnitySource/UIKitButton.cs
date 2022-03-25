using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UIKit
{
    /// <summary>
    /// Является копией класса UnityEngine.UI.Button с небольшими правками
    /// https://bitbucket.org/Unity-Technologies/ui/src/f0c70f707cf09f959ad417049cb070f8e296ffe2/UnityEngine.UI/UI/Core/Button.cs?at=5.5&fileviewer=file-view-default
    /// </summary>

    // Button that's meant to work with mouse or touch-based devices.
    //[AddComponentMenu("UI/Button", 30)]
    public class UIKitButton : UIKitSelectable, IPointerClickHandler, IPointerUpHandler, ISubmitHandler
    {
        [Serializable]
        public class ButtonClickedEvent : UnityEvent { }

        // Event delegates triggered on click.
        [FormerlySerializedAs("onClick")]
        [SerializeField]
        private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

        protected UIKitButton()
        { }

        public ButtonClickedEvent onClick
        {
            get { return m_OnClick; }
            set { m_OnClick = value; }
        }

        private void Press()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            m_OnClick.Invoke();
        }

        // Trigger all registered callbacks.
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != m_InteractableKey)
                return;

            Press();
        }
        
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        private IEnumerator OnFinishSubmit()
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }
    }
}