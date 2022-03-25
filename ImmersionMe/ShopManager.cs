using System;
using UnityEngine;

namespace GameClient
{
    public class ShopManager
    {
        public Application Application;

        // Pack
        public const int SoftCurrencyForViewingAdsValue = 40;
        public const int SmallSoftCurrencyPackValue = 200;
        public const int MediumSoftCurrencyPackValue = 500;
        public const int BigSoftCurrencyPackValue = 900;

        public event Action OnSoftCurrencyChanged;
        public event Action<bool> OnSubscriptionChanged;

        public enum BuyPack
        {
            None = 0,
            SmallSoftCurrency = 1,
            MediumSoftCurrency = 2,
            BigSoftCurrency = 3,
            Premium = 4
        }

        public ShopManager(Application application)
        {
            Application = application;
        }
        
        public void SpendSoftCurrency(int value)
        {
            Application.ApplicationData.SoftCurrency -= value;
            OnSoftCurrencyChanged?.Invoke();
            SaveData();
        }

        public void AddSoftCurrencyForViewingAds()
        {
            Application.ApplicationData.SoftCurrency += SoftCurrencyForViewingAdsValue;
            OnSoftCurrencyChanged?.Invoke();
            SaveData();
        }
        
        public void ShopBuyPack(string pack)
        {
            switch (pack)
            {
                case "com.yappmobile.sleeptime.200_hard_currency":
                    Application.ApplicationData.SoftCurrency += SmallSoftCurrencyPackValue;
                    break;
                
                case "com.yappmobile.sleeptime.500_hard_currency":
                    Application.ApplicationData.SoftCurrency += MediumSoftCurrencyPackValue;
                    break;
                
                case "com.yappmobile.sleeptime.900_hard_currency":
                    Application.ApplicationData.SoftCurrency += BigSoftCurrencyPackValue;
                    break;
                
                case "com.yappmobile.sleeptime.monthly_subscription":
                    PremiumChange(true);
                    break;
                
                case "com.yappmobile.sleeptime.year_subscription":
                    PremiumChange(true);
                    break;
                    
                case "com.yappmobile.sleeptime.endless_subscription":
                    PremiumChange(true);
                    break;

                default:
                    Debug.LogError($"Error: BuyPack [{pack}] not found");
                    return;
            }
            
            OnSoftCurrencyChanged?.Invoke();
            SaveData();
        }

        private void SaveData()
        {
            Application.SaveData();
        }
        
        public void ResetData()
        {
            Application.ApplicationData.SoftCurrency = 0;
            OnSoftCurrencyChanged?.Invoke();
            PremiumChange(false);
        }

        private void PremiumChange(bool value)
        {
            Application.ApplicationData.IsSubscription = value;
            OnSubscriptionChanged?.Invoke(Application.ApplicationData.IsSubscription);
        }
    }
}