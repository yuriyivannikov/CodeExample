namespace GameClient 
{
    public class SoundTableItemData
    {
        public readonly Application Application;
        public readonly SoundModel SoundModel;
        public readonly bool IsFirstItem; 
        
        public SoundTableItemData(Application application, SoundModel soundModel, bool isFirstItem)
        {
            Application = application;
            SoundModel = soundModel;
            IsFirstItem = isFirstItem;
        }
    }
}
