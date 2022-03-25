using System;

namespace GameClient
{
    public class BuyPresetModalData
    {
        public Application Application;
        public int Price;
        public Action<bool> Callback;
        
        public BuyPresetModalData(Application application, int price, Action<bool> callback)
        {
            Application = application;
            Price = price;
            Callback = callback;
        }
    }
}
