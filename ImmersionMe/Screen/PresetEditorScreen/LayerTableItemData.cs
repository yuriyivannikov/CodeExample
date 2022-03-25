namespace GameClient 
{
    public class LayerTableItemData
    {
        public readonly Application Application;
        public readonly LayerModel LayerModel;
        public readonly bool IsFirstItem;

        public LayerTableItemData(Application application, LayerModel layerModel, bool isFirstItem)
        {
            Application = application;
            LayerModel = layerModel;
            IsFirstItem = isFirstItem;
        }
    }
}