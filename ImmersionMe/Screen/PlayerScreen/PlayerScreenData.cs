namespace GameClient
{
    public class PlayerScreenData
    {
        public readonly Application Application;
        public readonly PresetModel PresetModel;

        public PlayerScreenData(Application application, PresetModel presetModel)
        {
            Application = application;
            PresetModel = presetModel;
        }
    }
}
