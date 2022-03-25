using System;
using Assets.System.Scripts.Localization;
using UnityEngine;

namespace UIKit
{
    public class UITimer : MonoBehaviour
    {
        public TextStyleComponent Label;

        public GameObject LoadingIndication;

        public bool IsCropped;

        public event Action Finish;
        public bool IsActive { get; private set; }

        private DateTime _dateTime;
        private int _lastSeconds = Int32.MaxValue;

        private void Update()
        {
            if (!IsActive || Label == null)
                return;

            var timer = _dateTime - DateTime.Now;
            var seconds = timer.TotalSeconds;
            if (_lastSeconds == (int) seconds)
                return;

            if (seconds > 0)
            {
                Label.text = LocalizationManager.LocalizeDateTimeLeft(_dateTime, IsCropped);
            }
            else
            {
                IsActive = false;
                Finish?.Invoke();
            }

            if (LoadingIndication != null)
                LoadingIndication.SetActive(seconds <= 0);

            Label.gameObject.SetActive(seconds > 0);

            _lastSeconds = (int) seconds;
        }

        public void SetDate(DateTime dateTime)
        {
            _lastSeconds = int.MaxValue;
            _dateTime = dateTime;

            var timer = _dateTime - DateTime.Now;
            IsActive = timer.TotalSeconds > 0;
        }
    }
}