using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace GameClient
{
    public class PresetModelData
    {
        [JsonProperty(PropertyName = "layers")]
        public Dictionary<string, LayerModelData> Layers;
        
        [JsonProperty(PropertyName = "locked")]
        public bool IsLocked;
    }

    public class PresetModel
    {
        public Application Application;
        public GameObject SoundGameObject;
        public GameConfig.PresetAssetData PresetConfig;
        public PresetModelData PresetData;
        
        public LayerModel[] LayerModels;
        public PresetsScreen.PlayState PlayState;
        public AK.Wwise.Switch PresetSwitch => PresetConfig.PresetSwitch;
        public bool IsPlay => PlayState == PresetsScreen.PlayState.TargetPlay || PlayState == PresetsScreen.PlayState.ChildPlay;
        
        public PresetModel(Application application, GameObject parentGameObject, GameConfig.PresetAssetData presetConfig, PresetModelData presetData)
        {
            Application = application;
            PresetConfig = presetConfig;
            PresetData = presetData;
            
            SoundGameObject = Utils.CreateSoundGameObject(PresetConfig.Name, parentGameObject.transform);
            SoundGameObject.transform.SetParent(parentGameObject.transform);
            
            LayerModels = new LayerModel[PresetConfig.Layers.Length];
            for (var i = 0; i < PresetConfig.Layers.Length; i++)
            {
                var layerConfig = PresetConfig.Layers[i];

                if (!PresetData.Layers.TryGetValue(layerConfig.Name, out var layerData))
                {
                    layerData = new LayerModelData();
                    layerData.Sounds = new Dictionary<string, SoundModelData>();
                    SetLayerDefaultData(layerData, layerConfig);
                    
                    PresetData.Layers.Add(layerConfig.Name, layerData);
                }

                var layerSwitch = GameConfig.Instance.LayerSwitchList[i];
                var layerRTPCVolume = GameConfig.Instance.LayerRTPCVolumeList[i];
                var layerSoundGameObject = Utils.CreateSoundGameObject(layerConfig.Name, SoundGameObject.transform);
                LayerModels[i] = new LayerModel(Application, layerSoundGameObject, this, layerConfig, layerData, layerSwitch, layerRTPCVolume);
            }
        }

        private void SetLayerDefaultData(LayerModelData layer , GameConfig.LayerAssetData layerConfig)
        {
            layer.Enabled = layerConfig.Enabled;
            layer.Volume = layerConfig.Volume;
            layer.DelayLow = layerConfig.Delay.x;
            layer.DelayHigh = layerConfig.Delay.y;
        }

        public void SetCurrentPresetModelToApplication()
        {
            Application.SetCurrentPlayedPresetModel(this);
        }

        public void Update()
        {
            foreach (var layerModel in LayerModels)
            {
                layerModel.Update();
            }
        }
        
        public void Play()
        {
            if (PlayState == PresetsScreen.PlayState.TargetPlay)
                return;
            
            SetCurrentPresetModelToApplication();
            
            PlayState = PresetsScreen.PlayState.TargetPlay;
            
            foreach (var layerModel in LayerModels)
            {
                layerModel.LayerChildPlay();
            }
        }

        public void Stop()
        {
            PlayState = PresetsScreen.PlayState.Normal;
            
            foreach (var layerModel in LayerModels)
            {
                layerModel.LayerStop();
            }
        }

        public void ResetData()
        {
            foreach (var layerModel in LayerModels)
            {
                SetLayerDefaultData(layerModel.LayerData, layerModel.LayerConfig);
                layerModel.ResetData();
            }
            
            Application.SaveData();
        }
    }
}