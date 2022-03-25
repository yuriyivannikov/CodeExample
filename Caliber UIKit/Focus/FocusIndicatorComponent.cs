using UnityEngine;

namespace UIKit
{
    public class FocusIndicatorComponent : MonoBehaviour
    {
        /*
         * Непосредственно фокус 
         */
        
        /*
        public enum AnimationState
        {
            Show,
            Hide,
            HideInProgress
        }

        [Serializable]
        public class FocusIndicatorStyleUse
        {
            public string[] Container
            {
                get
                {
                    var container = FocusStyleManager.FocusIndicatorStyleNames;
                    string[] stringArray = new string[container.Length + 1];
                    stringArray[0] = "Override";

                    container.CopyTo(stringArray, 1);

                    return stringArray;
                }
            }

            public int Index;
            [SerializeField]
            private FocusIndicatorStyleObject.FocusIndicatorStyleInfo _focusIndicatorStyleInfo;

            public FocusIndicatorStyleUse()
            {
                _focusIndicatorStyleInfo = new FocusIndicatorStyleObject.FocusIndicatorStyleInfo();
            }

            public FocusIndicatorStyleObject.FocusIndicatorStyleInfo GetValue()
            {
                if (Index == 0)
                    return _focusIndicatorStyleInfo;

                return FocusStyleManager.Instance[Container[Index]].FocusStyleInfo;
            }
        }
        
        [SerializeField]
        private FocusIndicatorStyleUse _focusIndicatorStyleUse;
        
        public FocusIndicatorStyleUse FocusStyleUse
        {
            get
            {
                if (_focusIndicatorStyleUse == null)
                    _focusIndicatorStyleUse = new FocusIndicatorStyleUse();

                return _focusIndicatorStyleUse;
            }
        }
        
        private AnimationState _animationState = AnimationState.Hide;
        private IEnumerator _animationCoroutine;
        private float _time;

        private Image _image;

        private CanvasGroup _canvasGroup;
        private CanvasRenderer _canvasRenderer;

        private void Awake()
        {
            _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasRenderer = gameObject.GetComponent<CanvasRenderer>();
                if (_canvasRenderer == null)
                    Debug.LogError("Error: CanvasRenderer not found!");
            }
        }

        private void Start()
        {
            SetAlpha(0);
            SetColor(FocusStyleUse.GetValue().FocusColor);
        }

        private void OnDisable()
        {
            if (_animationState == AnimationState.Hide)
                return;

            SetAlpha(0);

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            if (_animationState == AnimationState.Show)
            {
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }

            _animationState = AnimationState.Hide;
        }

        public void Show()
        {
            _animationState = AnimationState.Show;
            
            if (_time != 0)
            {
                var focusIndicatorStyleInfo = FocusStyleUse.GetValue();

                _time = focusIndicatorStyleInfo.ShowAnimation.Time - (_time/focusIndicatorStyleInfo.HideAnimation.Time)*focusIndicatorStyleInfo.ShowAnimation.Time;
            }

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = UpdateAnimationCoroutine();
            StartCoroutine(UpdateAnimationCoroutine());
        }

        public void Hide()
        {
            _animationState = AnimationState.HideInProgress;

            if (_time != 0)
            {
                var focusIndicatorStyleInfo = FocusStyleUse.GetValue();

                _time = focusIndicatorStyleInfo.HideAnimation.Time - (_time/ focusIndicatorStyleInfo.ShowAnimation.Time)* focusIndicatorStyleInfo.HideAnimation.Time;
            }

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = UpdateAnimationCoroutine();
            StartCoroutine(_animationCoroutine);
        }

        private IEnumerator UpdateAnimationCoroutine()
        {
            yield return null;

            var focusIndicatorStyleInfo = FocusStyleUse.GetValue();

            FocusIndicatorStyleObject.AnimationInfo animationInfo = (_animationState == AnimationState.Show ? focusIndicatorStyleInfo.ShowAnimation : focusIndicatorStyleInfo.HideAnimation);
            
            if (_time < animationInfo.Time)
            {
                _time += Time.deltaTime;

                float delta = _time / animationInfo.Time;
                float AnimationCurveValue = animationInfo.AnimationCurve.Evaluate(delta);

                SetAlpha(AnimationCurveValue);

                _animationCoroutine = UpdateAnimationCoroutine();
                StartCoroutine(_animationCoroutine);
                yield break;
            }

            if (_animationState == AnimationState.HideInProgress)
                _animationState = AnimationState.Hide;

            _time = 0;
            yield break;
        }

        public void SetAlpha(float value)
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = value;
            else
            {
                if (_canvasRenderer != null)
                    _canvasRenderer.SetAlpha(value);
            }
        }

        public void SetColor(Color value)
        {
            if (_image == null)
                _image = GetComponent<Image>();

            if (_image)
                _image.color = value;
        }

        public AnimationState GetAnimationState()
        {
            return _animationState;
        }
        */
    }
}