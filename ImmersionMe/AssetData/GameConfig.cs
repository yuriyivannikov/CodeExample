using System;
using System.Collections.Generic;
using Assets.System.Scripts;
using UnityEngine;

namespace GameClient
{
    public class GameConfig : SingletonSerializedScriptableAsset<GameConfig>
    {
        [Header("Application")]
        public AK.Wwise.RTPC ApplicationBusRTPCVolume;
        
        [Header("Preset")]
        public AK.Wwise.RTPC PresetRTPCVolume;
        
        //[Header("Layer")]
        public List<AK.Wwise.Switch> LayerSwitchList;
        public List<AK.Wwise.RTPC> LayerRTPCVolumeList;
        
        [Header("Sound")]
        public AK.Wwise.Event SoundPlayEvent;
        public AK.Wwise.Event SoundStopEvent;
        public AK.Wwise.RTPC SoundRTPCVolume;

        public List<AK.Wwise.Switch> SoundSwitchList;
        
        //[Header("Data")]
        public PresetAssetData[] Data;
        
        [Serializable]
        public class PresetAssetData
        {
            public bool IsVisible;
            public string Name;
            public AK.Wwise.Switch PresetSwitch;
            [Range(1,100)]
            public int Volume = 50;
            public LayerAssetData[] Layers;
            public int Price;
            public Sprite PreviewImage;
            public Sprite Image;
            [Range(0,1)]
            public float HorizontalNormalizedPosition;
            [Range(0,1)]
            public float VerticalNormalizedPosition;
        }
        
        [Serializable]
        public class LayerAssetData
        {
            public string Name;
            [Range(1,100)]
            public int Volume = 50;
            public Vector2Int Delay = new Vector2Int(1, 5);
            public SoundAssetData[] Sounds;
            public bool Enabled = true;
        }
        
        [Serializable]
        public class SoundAssetData
        {
            public string Name;
            public Vector2Int Volume = new Vector2Int(25, 75);
            public bool IsLocked;
            public bool Enabled = true;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/PresetData")]
        public static void Create()
        {
            ScriptableObjectAsset.CreateAsset<GameConfig>();
        }
#endif
    }
}