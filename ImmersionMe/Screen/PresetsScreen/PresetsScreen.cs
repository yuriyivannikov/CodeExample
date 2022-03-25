using GameClient.Scripts;
using UIKit;
using UnityEngine;

namespace GameClient
{
    public class PresetsScreen : UIScreen
    {
        public enum PlayState
        {
            Normal,
            TargetPlay,
            ChildPlay
        }
        
        [SerializeField] private SimpleTable _presetsTable;
        [SerializeField] private AccountMoney _accountMoney;

        private PresetsScreenData _data;
        
        public override void SetData(object data)
        {
            if (data == null)
                return;
            
            var newData = (PresetsScreenData) data;
            if (_data == newData)
                return;

            _data = newData;
            
            _accountMoney.SetData(new AccountMoney.AccountMoneyData(_data.Application));
            
            _presetsTable.Clear();

            foreach (var presetModel in _data.PresetModels)
            {
                var itemData = new PresetTableItemData(_data.Application, presetModel);
                _presetsTable.AddItem(itemData);
            }
        }
    }
}
