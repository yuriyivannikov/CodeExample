using TheraBytes.BetterUi;
using UIKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameClient
{
    public class BuyPresetModal : UIModal
    {
        [SerializeField] private TextStyleComponent _priceText;
        [SerializeField] private BetterButton _trueButton;
        [SerializeField] private BetterButton _falseButton;
        [SerializeField] private BetterButton _closeButton;
        
        private BuyPresetModalData _data;

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
            _data = (BuyPresetModalData) data;
            
            _priceText.SetLocalizationArgs(_data.Price.ToString());
            _trueButton.interactable = _data.Application.ApplicationData.SoftCurrency >= _data.Price;
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