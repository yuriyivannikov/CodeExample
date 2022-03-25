namespace GameClient.Scripts
{
    public class PresetsScreenData
    {
        public readonly Application Application;
        public readonly PresetModel[] PresetModels;

        public PresetsScreenData(Application application, PresetModel[] presetModels)
        {
            Application = application;
            PresetModels = presetModels;
        }
    }
}