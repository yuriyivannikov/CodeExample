using TheraBytes.BetterUi;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameClient
{
    public class TrueFalseModal : UIModal
    {
        [SerializeField] private BetterButton _trueButton;
        [SerializeField] private BetterButton _falseButton;
        [SerializeField] private BetterButton _closeButton;
        
        private TrueFalseModalData _data;

        private void OnEnable()
        {
            _trueButton.Click += OnTrueButtonClick;
            _falseButton.Click += OnFalseButtonClick;
            _closeButton.Click += OnCloseButtonClick;
        }

        private void OnDisable()
        {
            _trueButton.Click -= OnTrueButtonClick;
            _falseButton.Click -= OnFalseButtonClick;
            _closeButton.Click += OnCloseButtonClick;
        }

        public override void SetData(object data)
        {
            _data = (TrueFalseModalData) data;
        }
        
        private void OnTrueButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            _data.Callback?.Invoke(true);
            Close();
        }

        private void OnFalseButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            CloseModal();
        }

        private void OnCloseButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            CloseModal();
        }

        private void CloseModal()
        {
            _data.Callback?.Invoke(false);
            Close();
        }
    }
}