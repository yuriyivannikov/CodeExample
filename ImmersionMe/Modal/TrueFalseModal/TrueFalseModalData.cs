using System;

namespace GameClient
{
    public class TrueFalseModalData
    {
        public Application Application;
        public Action<bool> Callback;
        
        public TrueFalseModalData(Application application, Action<bool> callback)
        {
            Application = application;
            Callback = callback;
        }
    }
}
