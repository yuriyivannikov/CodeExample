using TheraBytes.BetterUi;

namespace UIKit
{
    public abstract class CheckedTableItem : TableItem
    {
        public CheckedItemsTable Table { get; set; }

        public abstract BetterToggle Toggle { get; }

        public bool Value
        {
            get
            {
                return Toggle.isOn;
            }
            set
            {
                if (Toggle.isOn == value)
                    return;

                Toggle.isOn = value;
            }
        }
    }
}