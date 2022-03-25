using System.Linq;
using TheraBytes.BetterUi;
using UnityEngine;

namespace UIKit
{
    public class CheckedItemsTable : Table<CheckedTableItem>
    {
        [SerializeField]
        private BetterToggleGroup _betterToggleGroup;

        public BetterToggleGroup BetterToggleGroup { get { return _betterToggleGroup; } }

        public override void Clear()
        {
            base.Clear();

            if (_betterToggleGroup)
            {
                _betterToggleGroup.SetAllTogglesOff();
            }
        }

        public override CheckedTableItem AddItem(object itemData)
        {
            var item = base.AddItem(itemData);
            if (item != null)
            {
                item.Table = this;
                if (item.Toggle != null)
                {
                    item.Toggle.group = BetterToggleGroup;
                    item.Toggle.Data = itemData;
                }
                
                item.SetData(itemData);
            }
            return item;
        }

        public CheckedTableItem GetCheckedTableItem()
        {
            return GetAllItems().FirstOrDefault(item => item.Value);
        }

        // public void SetCheckedTableItem(object itemData)
        // {
        //     foreach (var item in GetAllItems())
        //     {
        //         item.Value = item.GetData().Equals(itemData);
        //     }
        // }

        public void TurnOffWhileLoading(bool isTurnedOff)
        {
            _betterToggleGroup.enabled = !isTurnedOff;
        }
    }
}