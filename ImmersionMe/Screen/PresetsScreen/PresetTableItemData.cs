namespace GameClient 
{
    public class PresetTableItemData
    {
        public Application Application;
        public PresetModel PresetModel;
        
        public PresetTableItemData(Application application, PresetModel presetModel)
        {
            Application = application;
            PresetModel = presetModel;
        }
    }
}
