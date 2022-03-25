namespace GameClient
{
    public class PresetEditorScreenData
    {
        public readonly Application Application;
        public readonly PresetModel PresetModel;
        
        

        public PresetEditorScreenData(Application application, PresetModel presetModel)
        {
            Application = application;
            PresetModel = presetModel;
        }
    }
}
