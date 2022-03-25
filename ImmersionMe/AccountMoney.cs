using GameClient;
using TheraBytes.BetterUi;
using UIKit;
using UnityEngine;
using UnityEngine.EventSystems;
using Application = GameClient.Application;

public class AccountMoney : MonoBehaviour
{
    public class AccountMoneyData
    {
        public Application Application;

        public AccountMoneyData(Application application)
        {
            Application = application;
        }
    }
    
    [SerializeField] private BetterButton Button;
    [SerializeField] private TextStyleComponent Text;
    
    private AccountMoneyData _data;
    private int _softCurrency;
    private float _lerpSoftCurrencyTime;

    private void OnEnable()
    {
        if (Button != null)
            Button.Click += ButtonOnClick;
    }
    
    private void OnDisable()
    {
        if (Button != null)
            Button.Click -= ButtonOnClick;
    }
    
    public void SetData(AccountMoneyData data)
    {
        _data = data;
        
        _softCurrency = _data.Application.ApplicationData.SoftCurrency;
        Text.text = _softCurrency.ToString();
        
        UpdateData();
    }

    private void UpdateData()
    {
        if (_data == null)
            return;
        
        if (_data.Application.ApplicationData.IsSubscription)
            return;
        
        if (_softCurrency == _data.Application.ApplicationData.SoftCurrency)
            return;
        
        const float animationLength = 2f;
        
        _lerpSoftCurrencyTime += Time.deltaTime / animationLength;
        _softCurrency = (int) Mathf.Lerp(_softCurrency, _data.Application.ApplicationData.SoftCurrency, _lerpSoftCurrencyTime);

        if (_lerpSoftCurrencyTime >= 1)
            _lerpSoftCurrencyTime = 0;
        
        Text.text = _softCurrency.ToString();
    }
    
    private void Update()
    {
        UpdateData();
    }

    private void ButtonOnClick(BetterButton arg1, BaseEventData arg2)
    {
        _data.Application.ShowScreen(UIScreen.ScreenType.ShopScreen, new ShopScreenData(_data.Application));
    }
}
