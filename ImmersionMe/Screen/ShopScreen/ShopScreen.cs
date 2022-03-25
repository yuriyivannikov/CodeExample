using System.Collections.Generic;
using GameClient.Purchasing;
using TheraBytes.BetterUi;
using TMPro;
using UIKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace GameClient
{
    public class ShopScreen : UIScreen
    {
        public BetterButton CloseButton;
        public TextStyleComponent AccountMoneyText;
        
        [SerializeField] private RewardedAdsButton _rewardedAdsButton;
        
        // public BetterButton SmallSoftCurrencyButton;
        // public BetterButton MediumSoftCurrencyButton;
        // public BetterButton BigSoftCurrencyButton;
        // public BetterButton PremiumButton;

        public TextStyleComponent ViewingAdsValueText;
        public TextStyleComponent SmallSoftCurrencyPackValueText;
        public TextStyleComponent MediumSoftCurrencyPackValueText;
        public TextStyleComponent BigSoftCurrencyPackValueText;
        
        // public TextStyleComponent SmallSoftCurrencyPackPriceText;
        // public TextStyleComponent MediumSoftCurrencyPackPriceText;
        // public TextStyleComponent BigSoftCurrencyPackPriceText;
        // public TextStyleComponent PremiumPackPriceText;

        // public Dictionary<ShopManager.BuyPack, BetterButton> BuyPackButtons;

        [SerializeField] private LayoutElement _contentLayoutElement;
        
        [SerializeField] public BetterButton AddTestMoneyButton;// For test

        private ShopScreenData _data;
        private ScreenOrientation _screenOrientation;
        
        private void Awake()
        {
            // BuyPackButtons = new Dictionary<ShopManager.BuyPack, BetterButton>
            // {
            //     {ShopManager.BuyPack.SmallSoftCurrency, SmallSoftCurrencyButton},
            //     {ShopManager.BuyPack.MediumSoftCurrency, MediumSoftCurrencyButton},
            //     {ShopManager.BuyPack.BigSoftCurrency, BigSoftCurrencyButton},
            //     {ShopManager.BuyPack.Premium, PremiumButton}
            // };
        }
        
        private void OnEnable()
        {
            CloseButton.Click += OnCloseButtonClick;

            AddTestMoneyButton.Click += OnAddTestMoney;// For test
            
            _rewardedAdsButton.LoadAd();
            

            // ViewingAdsButton.Click += OnViewingAdsButtonClick;

            // foreach (var button in BuyPackButtons.Values)
            // {
            //     button.Click += OnBuyPackButtonClick;
            // }
        }

        private void OnAddTestMoney(BetterButton arg1, BaseEventData arg2)
        {
            _data.Application.ShopManager.ShopBuyPack("com.yappmobile.sleeptime.500_hard_currency");
        }

        private void OnDisable()
        {
            CloseButton.Click -= OnCloseButtonClick;
            AddTestMoneyButton.Click += OnAddTestMoney;// For test
            // ViewingAdsButton.Click -= OnViewingAdsButtonClick;
            
            // foreach (var button in BuyPackButtons.Values)
            // {
            //     button.Click -= OnBuyPackButtonClick;
            // }
            
            _data.Application.ShopManager.OnSoftCurrencyChanged -= SetupMoney;
        }
    
        public override void SetData(object data)
        {
            if (data == null || _data == data)
                return;

            _data = (ShopScreenData) data;
            
            ViewingAdsValueText.text = ShopManager.SoftCurrencyForViewingAdsValue.ToString();
            SmallSoftCurrencyPackValueText.text = ShopManager.SmallSoftCurrencyPackValue.ToString();
            MediumSoftCurrencyPackValueText.text = ShopManager.MediumSoftCurrencyPackValue.ToString();
            BigSoftCurrencyPackValueText.text = ShopManager.BigSoftCurrencyPackValue.ToString();
            
            _data.Application.ShopManager.OnSoftCurrencyChanged += SetupMoney;
            SetupMoney();
        }
        
        public override object GetData()
        {
            return _data;
        }

        private void SetupMoney()
        {
            var softCurrency = _data.Application.ApplicationData.SoftCurrency;
            AccountMoneyText.text = softCurrency.ToString();
        }

        private void Update()
        {
            if (_screenOrientation == Screen.orientation || _data == null)
                return;

            _screenOrientation = Screen.orientation;
                
            _data.Application.SetupScreenHeight(_contentLayoutElement);
        }

        private void OnCloseButtonClick(BetterButton arg1, BaseEventData arg2)
        {
            _data.Application.HideScreen(ScreenType.ShopScreen);
        }
        
        // private void OnViewingAdsButtonClick(BetterButton arg1, BaseEventData arg2)
        // {
        //     //todo add ads
        //     
        //     _data.Application.ShopManager.AddSoftCurrencyForViewingAds();
        // }
        
        /*
        private void OnBuyPackButtonClick(BetterButton button, BaseEventData arg2)
        {
            var buyPack = ShopManager.BuyPack.None;
            foreach (var keyValuePair in BuyPackButtons)
            {
                if (keyValuePair.Value == button)
                {
                    buyPack = keyValuePair.Key;
                    break;
                }
            }

            if (buyPack == ShopManager.BuyPack.None)
            {
                Debug.LogError("Error: buyPack is None");
                return;
            }
            
            //todo add 
            
            _data.Application.ShopManager.ShopBuyPack(buyPack);
        }
        */
        
        public void PurchaseComplete(Product product)
        {
            /*
            var buyPack = ShopManager.BuyPack.None;
            foreach (var keyValuePair in BuyPackButtons)
            {
                if (keyValuePair.Value == button)
                {
                    buyPack = keyValuePair.Key;
                    break;
                }
            }

            if (buyPack == ShopManager.BuyPack.None)
            {
                Debug.LogError("Error: buyPack is None");
                return;
            }
            */
            
            _data.Application.ShopManager.ShopBuyPack(product.definition.id);
        }

        public void PurchaseFailed(Product product, PurchaseFailureReason reason)
        {

        }
    }
}