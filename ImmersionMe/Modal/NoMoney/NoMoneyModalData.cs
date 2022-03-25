namespace GameClient
{
    public class NoMoneyModalData
    {
        public Application Application;
        public int Price;
        
        public NoMoneyModalData(Application application, int price)
        {
            Application = application;
            Price = price;
        }
    }
}