using System.Collections;
using System.Globalization;
using UIKit;
using UnityEngine;

namespace Assets.UI
{
    public class AnimatedNumber : MonoBehaviour
    {
        [SerializeField]
        private TextStyleComponent _label;

        [Header("Visual")]

        //[SerializeField]
        //private bool _isInteger = true;
        [SerializeField]
        private bool _isSeparator = true;

        [Header("Animation")]
        [SerializeField]
        [Range(0f, 10f)]
        private float _delayTime = 0.5f;

        [SerializeField]
        private AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);

        private Coroutine _animationCoroutine;
        private int _value;

        private void SetValue(int value)
        {
            _value = value;

            if (_label == null)
                return;

            if (_isSeparator)
            {
                var num = new NumberFormatInfo();
                num.NumberGroupSeparator = " ";
                _label.text = _value.ToString("N0", num);
            }
            else
            {
                _label.text = _value.ToString();
            }
        }

        public void SetValue(int value, bool instant = false)
        {
            if (_animationCoroutine != null)
            {
                AsyncActions.Instance.StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            if (!instant)
            {
                _animationCoroutine = AsyncActions.Instance.StartCoroutine(AnimationAsync(value));
            }
            else
            {
                SetValue(value);
            }
        }

        private IEnumerator AnimationAsync(int value)
        {
            yield return new WaitForEndOfFrame();

            float life = 0;
            var oldValue = _value;

            while (life < _delayTime)
            {
                life += Time.deltaTime;

                SetValue(oldValue + (int) ((value - oldValue) * _curve.Evaluate(life / _delayTime)));
                
                //SetValue(Mathf.Lerp(oldValue, value, _curve.Evaluate(life / _delayTime)));
                yield return new WaitFrames(1);
            }

            SetValue(value);
            _animationCoroutine = null;
        }
    }
}