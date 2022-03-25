namespace GameClient
{
    public class TimerScreenData
    {
        public Application Application;
        public TimerModel TimerModel;

        public TimerScreenData(Application application, TimerModel timerModel)
        {
            Application = application;
            TimerModel = timerModel;
        }
    }
}