using TheraBytes.BetterUi;
using UIKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameClient
{
    public class NoMoneyModal : UIModal
    {
        [SerializeField] private BetterButton _shopButton;
        [SerializeField] private BetterButton _okButton;
        [SerializeField] private BetterButton _closeButton;
        
        private NoMoneyModalData _data;

        private void OnEnable()
        {
            _shopButton.Click += OnShopButtonClick;
            _okButton.Click += OnOkButtonClick;
            _closeButton.Click += OnCloseButtonClick;
        }

        private void OnDisable()
        {
            _shopButton.Click -= OnShopButtonClick;
            _okButton.Click -= OnOkButtonClick;
            _closeButton.Click -= OnCloseButtonClick;
        }

        public override void SetData(object data)
        {
            _data = (NoMoneyModalData) data;
        }
        
        private void OnShopButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            _data.Application.ShowScreen(UIScreen.ScreenType.ShopScreen, new ShopScreenData(_data.Application));
            Close();
        }

        private void OnOkButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            Close();
        }

        private void OnCloseButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            Close();
        }
    }
}