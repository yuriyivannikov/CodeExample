using UIKit;
using UnityEngine;

namespace GameClient.Plugins.UIKit.Scripts
{
    public class DigitsTextStyleComponent: TextStyleComponent
    {
        
        [SerializeField]
        private bool _decimalOverflowUpperOverride;

        [SerializeField]
        [Range(1, 99999999)]
        private int _decimalOverflowUpperLimit = 999;
        
        [SerializeField]
        private string _decimalOverflowUpperKey = "999+";

        public override void SetText(int value)
        {
            if (_decimalOverflowUpperOverride && value > _decimalOverflowUpperLimit)
            {
                SetText(_decimalOverflowUpperKey);
            }
            else
            {
                base.SetText(value);
            }
        }
    }
}