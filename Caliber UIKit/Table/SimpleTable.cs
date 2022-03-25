using System;

namespace UIKit
{
    public class SimpleTable : Table<TableItem>
    {
        public override TableItem AddItem(Object itemData)
        {
            var item = base.AddItem(itemData);
            /*
            if (item != null)
            {
                item.SetData(itemData);
            }
            */
            return item;
        }
    }
}