/// Credit Tomasz Schelenz 
/// Sourced from - https://bitbucket.org/ddreaper/unity-ui-extensions/issues/46/feature-uiknob#comment-29243988

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// KNOB controller
/// s
/// Fields
/// - direction - direction of rotation CW - clockwise CCW - counter clock wise
/// - knobValue - Output value of the control
/// - maxValue - max value knob can rotate to, if higher than loops value or set to 0 - it will be ignored, and max value will be based on loops
/// - loops - how any turns around knob can do
/// - clampOutput01 - if true the output knobValue will be clamped between 0 and 1 regardless of number of loops.
/// - snapToPosition - snap to step. NOTE: max value will override the step.
/// - snapStepsPerLoop - how many snap positions are in one knob loop;
/// - OnValueChanged - event that is called every frame while rotating knob, sends <float> argument of knobValue
/// NOTES
/// - script works only in images rotation on Z axis;
/// - while dragging outside of control, the rotation will be canceled
/// </summary>
/// 
namespace UIKit
{
    [AddComponentMenu("UI/Knob", 35)]
    public class UIKitKnob : UIKitSelectable, IDragHandler, IInitializePotentialDragHandler
    {
        public enum Direction { CW, CCW };
        [Tooltip("Direction of rotation CW - clockwise, CCW - counterClockwise")]
        public Direction DirectionRotation = Direction.CW;
        [Tooltip("Max value of the knob, maximum RAW output value knob can reach, overrides snap step")] 
        [SerializeField]
        private float _maxValue;
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                if (Math.Abs(_maxValue - value) < Mathf.Epsilon)
                    return;
                
                _maxValue = value;
                if (MaxValue >= 0 && _knobValue + _currentLoops > MaxValue)
                    SetValue(MaxValue, true, false);
            }
        }
        [Tooltip("How many rotations knob can do, if higher than max value, the latter will limit max value")]
        public int Loops = 1;
        [Tooltip("Clamp output value between 0 and 1, useful with loops > 1")]
        public bool IsClampOutput01;
        [Tooltip("snap to position?")]
        public bool IsSnapToPosition;
        [Tooltip("Number of positions to snap")]
        public int SnapStepsPerLoop = 10;
        [Space(30)]
        public KnobFloatValueEvent OnValueChanged;
        
        public bool IsDrag => _canDrag;
        public float CurrentValue => _knobValue + _currentLoops;

        private float _currentLoops;
        private float _knobValue;
        private float _previousValue;
        private float _initAngle;
        private float _currentAngle;
        private Vector2 _currentVector;
        private Quaternion _initRotation;
        private bool _canDrag;
		private bool _screenSpaceOverlay;

        protected override void Awake()
        {
			_screenSpaceOverlay = GetComponentInParent<Canvas>().rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            _canDrag = false;
        }

        // public override void OnPointerEnter(PointerEventData eventData)
        // {
        //     _canDrag = true;
        // }
        // public override void OnPointerExit(PointerEventData eventData)
        // {
        //     _canDrag = false;
        // }

        public override void OnPointerDown(PointerEventData eventData)
        {
            _canDrag = true;

            base.OnPointerDown(eventData);

            _initRotation = transform.rotation;
			if (_screenSpaceOverlay)
            {
				_currentVector = eventData.position - (Vector2)transform.position;
            }
            else
            {
				_currentVector = eventData.position - (Vector2)Camera.main.WorldToScreenPoint(transform.position);
            }
            _initAngle = Mathf.Atan2(_currentVector.y, _currentVector.x) * Mathf.Rad2Deg;
        }

        public void OnDrag(PointerEventData eventData)
        {
            //CHECK IF CAN DRAG
            if (!_canDrag || !interactable)
                return;

            if (_screenSpaceOverlay)
			{
				_currentVector = eventData.position - (Vector2)transform.position;
			}
			else
			{
				_currentVector = eventData.position - (Vector2)Camera.main.WorldToScreenPoint(transform.position);
			}
            _currentAngle = Mathf.Atan2(_currentVector.y, _currentVector.x) * Mathf.Rad2Deg;

            var addRotation = Quaternion.AngleAxis(_currentAngle - _initAngle, this.transform.forward);
            addRotation.eulerAngles = new Vector3(0, 0, addRotation.eulerAngles.z);

            var finalRotation = _initRotation * addRotation;

            if (DirectionRotation == Direction.CW)
            {
                _knobValue = 1 - (finalRotation.eulerAngles.z / 360f);

                if (IsSnapToPosition)
                {
                    SnapToPosition();
                    finalRotation.eulerAngles = new Vector3(0, 0, 360 - 360 * _knobValue);
                }
            }
            else
            {
                _knobValue = (finalRotation.eulerAngles.z / 360f);

                if (IsSnapToPosition)
                {
                    SnapToPosition();
                    finalRotation.eulerAngles = new Vector3(0, 0, 360 * _knobValue);
                }
            }

            //PREVENT OVERROTATION
            if (Mathf.Abs(_knobValue - _previousValue) > 0.5f)
            {
                if (_knobValue < 0.5f && Loops > 1 && _currentLoops < Loops - 1)
                {
                    _currentLoops++;
                }
                else if (_knobValue > 0.5f && _currentLoops >= 1)
                {
                    _currentLoops--;
                }
                else
                {
                    if (_knobValue > 0.5f && _currentLoops == 0)
                    {
                        _knobValue = 0;
                        transform.localEulerAngles = Vector3.zero;
                        InvokeEvents(_knobValue + _currentLoops);
                        return;
                    }

                    if (_knobValue < 0.5f && _currentLoops == Loops - 1)
                    {
                        _knobValue = 1;
                        transform.localEulerAngles = Vector3.zero;
                        InvokeEvents(_knobValue + _currentLoops);
                        return;
                    }
                }
            }

            //CHECK MAX VALUE
            if (MaxValue >= 0)
            {
                if (_knobValue + _currentLoops > MaxValue)
                {
                    _knobValue = MaxValue - _currentLoops;
                    var maxAngle = DirectionRotation == Direction.CW ? 360f - 360f * MaxValue : 360f * MaxValue;
                    transform.localEulerAngles = new Vector3(0, 0, maxAngle);
                    InvokeEvents(_knobValue);
                    return;
                }
            }

            transform.rotation = finalRotation;
            InvokeEvents(_knobValue + _currentLoops);

            _previousValue = _knobValue;
        }
        
        private void SnapToPosition()
        {
            var snapStep = 1 / (float)SnapStepsPerLoop;
            var newValue = Mathf.Round(_knobValue / snapStep) * snapStep;
            _knobValue = newValue;
        }
        
        private void InvokeEvents(float value, bool isManualChange = true)
        {
            if (IsClampOutput01)
                value /= Loops;
            
            OnValueChanged.Invoke(value, isManualChange);
        }
        
        public void SetValue(float value, bool sendCallback = true, bool isManualChange = true)
        {
            _currentLoops = (int) value;
            _previousValue = _knobValue;
            _knobValue = value - _currentLoops;

            if (MaxValue >= 0 && _knobValue + _currentLoops > MaxValue)
                _knobValue = MaxValue - _currentLoops;

            var angle = DirectionRotation == Direction.CW ? 360f - 360f * _knobValue : 360f * _knobValue;
            transform.localEulerAngles = new Vector3(0, 0, angle);

            if (sendCallback)
                InvokeEvents(value, isManualChange);
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }
    }

    [Serializable]
    public class KnobFloatValueEvent : UnityEvent<float, bool> { }
}