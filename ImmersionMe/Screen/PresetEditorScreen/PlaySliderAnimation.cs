using System.Collections;
using System.Collections.Generic;
using Assets.UI.Colors;
using PolyAndCode.UI.effect;
using TheraBytes.BetterUi;
using UnityEngine;

namespace GameClient 
{
    public class PlaySliderAnimation : MonoBehaviour
    {
        [SerializeField] private List<BetterSlider> _sliderList;

        [SerializeField] private float _waitTime = 5f;

        private float _fullCycleTime;
        
        private void Awake()
        {
            _fullCycleTime = _waitTime * 2;
        }

        private void OnEnable()
        {
            UpdateColor();
            ColorLibrary.CallbackUpdateAllColor += OnCallbackUpdateAllColor;

            if (_fullCycleTime == 0)
            {
                Debug.LogError("PlaySliderAnimation: fullCycleTime is zero");
                return;
            }

            var counter = 0;
            foreach (var slider in _sliderList)
            {
                var uiGradient = slider.FillImage.gameObject.GetComponent<UIGradient>();
                StartCoroutine(PlayAnimation(_waitTime * counter, slider, uiGradient));
                counter++;
            }
        }

        private void OnDisable()
        {
            ColorLibrary.CallbackUpdateAllColor -= OnCallbackUpdateAllColor;
            StopAllCoroutines();
        }

        private IEnumerator PlayAnimation(float waitTime, BetterSlider slider, UIGradient uiGradient)
        {
            slider.value = 0.02f;
            uiGradient.offset = Vector2.right;
            
            yield return new WaitForSeconds(waitTime);
            
            // while (slider.value < slider.maxValue)
            // {
            //
            // }

            while (uiGradient.offset.x > 0)
            {
                var normalTime = Time.deltaTime / _fullCycleTime;
                
                var gradientValue = uiGradient.offset.x - normalTime;
                uiGradient.offset = new Vector2(gradientValue, 0);
                
                slider.value += normalTime;
                
                yield return null;
            }

            while (uiGradient.offset.x > -0.5f)
            {
                var normalTime = Time.deltaTime / _waitTime / 2;
                
                var gradientValue = uiGradient.offset.x - normalTime;
                uiGradient.offset = new Vector2(gradientValue, 0);
                
                yield return null;
            }

            slider.transform.SetSiblingIndex(_sliderList.Count);
            
            StartCoroutine(PlayAnimation(0f, slider, uiGradient));
        }
        
        private void Update()
        {
            
        }

        private void UpdateColor()
        {
            if (_sliderList.Count == 0)
                return;

            var accentColor = ColorLibrary.GetColor(ColorLibrary.ColorMainAccent);
            
            var firstSlider = _sliderList[0];
            var uiGradient = firstSlider.FillImage.gameObject.GetComponent<UIGradient>();
            
            var alphaKeys = uiGradient.gradient.alphaKeys;
            var colorKeys = new [] {new GradientColorKey(accentColor, 0)};
            var gradient = new Gradient();
            gradient.SetKeys(colorKeys, alphaKeys);
            
            foreach (var slider in _sliderList)
            {
                uiGradient = slider.FillImage.gameObject.GetComponent<UIGradient>();
                uiGradient.gradient = gradient;
            }
        }
        
        private void OnCallbackUpdateAllColor()
        {
            UpdateColor();
        }
    }
}