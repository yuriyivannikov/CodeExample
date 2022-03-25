using TheraBytes.BetterUi;
using UIKit;
using UnityEngine;
using UnityEngine.UI;

namespace GameClient 
{
    public class ColoPickerTableItem : TableItem
    {
        [SerializeField] public BetterToggle Toggle;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _checkmarkImage;

        private ColoPickerTableItemData _data;

        public override void SetData(object data)
        {
            if (data == null)
                return;
            
            _data = (ColoPickerTableItemData) data;
            _backgroundImage.color = _data.Color;
            _checkmarkImage.color = _data.Color;
        }

        public Color GetColor() => _data.Color;
    }
}