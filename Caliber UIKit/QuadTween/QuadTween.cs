using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plugins.QuadTween
{
    public class QuadTween : MonoBehaviour
    {
        /*
        [Button] public void TestReset() { Reset(); }
        [Button] public void TestPlayBackward() { PlayBackward("Show"); }
        [Button] public void TestPlayForward() { PlayForward("Show"); }
        [Button] public void TestPause() { Pause(); }
        
        [HideLabel, ShowInInspector]
        [ProgressBar(0, 1, DrawValueLabel = false)]
        private float ProgressBar
        {
            get { return _time / _maxLength; }
        }
        */
        [SerializeField]
        private string _autoPlayId;

        private List<string> GetAnimations()
        {
            return _animations.Select(e => e.Id).ToList();
        }

        [Space]
        [SerializeField]
        private List<QuadTweenAnimation> _animations = new List<QuadTweenAnimation>();
        public List<QuadTweenAnimation> Animations => _animations;

        // Events

        public event Action<string> OnComplete;

        //

        private bool _isPlay;
        private float _time;
        private float _speed = 1.0f;
        private string _id;
        private float _maxLength;

        private void Awake()
        {
            foreach (var animations in _animations)
            {
                if (animations != null)
                    animations.Init();
            }
        }

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(_autoPlayId))
            {
                ResetAndPlay(_autoPlayId);
            }
        }

        private void Update()
        {
            if (!_isPlay)
                return;

            _time += Time.deltaTime * _speed;
            foreach (var animations in _animations)
            {
                if (animations != null && animations.Id == _id)
                    animations.AddTime(Time.deltaTime * _speed);
            }

            if (_speed > 0 && _time > _maxLength || _speed < 0 && _time < 0)
            {
                _time = Mathf.Clamp(_time, 0, _maxLength);
                _isPlay = false;
                OnComplete?.Invoke(_id);
            }
        }

        private void ResetAnimation(string id = "")
        {
            _time = 0.0f;
            foreach (var animations in _animations)
            {
                if (animations != null && (animations.Id == id || string.IsNullOrEmpty(id)))
                    animations.SetTime(_time);
            }
        }

        public void ResetAndPlay(string id)
        {
            ResetAnimation(id);
            PlayForward(id);
        }

        public void PlayBackward(string id)
        {
            _maxLength = GetMaxLength(id);
            _speed = -1.0f;
            if (_maxLength > 0)
            {
                _id = id;
                _isPlay = true;
            }
        }

        public void PlayForward(string id)
        {
            _maxLength = GetMaxLength(id);
            _speed = 1.0f;
            if (_maxLength > 0)
            {
                _id = id;
                _isPlay = true;
            }
        }

        public void Pause()
        {
            _isPlay = false;
        }

        private float GetMaxLength(string id)
        {
            float maxLength = 0;
            foreach (var animations in _animations)
            {
                if (animations != null && animations.Id == id && maxLength < animations.Length)
                    maxLength = animations.Length;
            }
            return maxLength;
        }
    }
}