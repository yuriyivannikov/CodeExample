using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace GameClient
{
    public class LayerModelData
    {
        [JsonProperty(PropertyName = "sounds")]
        public Dictionary<string, SoundModelData> Sounds;
        
        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled;
        
        [JsonProperty(PropertyName = "volume")]
        public float Volume;

        [JsonProperty(PropertyName = "delay_low")]
        public float DelayLow;

        [JsonProperty(PropertyName = "delay_high")]
        public float DelayHigh;
    }
    
    public class LayerModel
    {
        public readonly Application Application;
        public GameObject SoundGameObject;
        public readonly PresetModel PresetModel;
        public readonly GameConfig.LayerAssetData LayerConfig;
        public LayerModelData LayerData;
        public readonly AK.Wwise.Switch LayerSwitch;
        public readonly AK.Wwise.RTPC LayerRTPCVolume;
        public bool IsPlay => PlayState == PresetsScreen.PlayState.TargetPlay || PlayState == PresetsScreen.PlayState.ChildPlay;
        
        public SoundModel[] SoundModels;
        public PresetsScreen.PlayState PlayState;
        public float Delay;
        public float EstimatedDelay;

        public bool Enabled
        {
            get => LayerData.Enabled;
            set
            {
                if (LayerData.Enabled == value)
                    return;

                LayerData.Enabled = value;

                if (LayerData.Enabled == false && PlayState == PresetsScreen.PlayState.ChildPlay)
                {
                    LayerStop();
                }
                else if (LayerData.Enabled && PresetModel.IsPlay)
                {
                    LayerChildPlay();
                }
            }
        }

        private bool _isWaitingDelay;
        private SoundModel _currentPlayedSoundModel;
        private bool _isRandom = true;

        public LayerModel(Application application, GameObject layerGameObject, PresetModel presetModel, GameConfig.LayerAssetData layerConfig, LayerModelData layerData, AK.Wwise.Switch layerSwitch, AK.Wwise.RTPC layerRTPCVolume)
        {
            Application = application;
            SoundGameObject = layerGameObject;
            PresetModel = presetModel;
            LayerConfig = layerConfig;
            LayerData = layerData;
            LayerSwitch = layerSwitch;
            LayerRTPCVolume = layerRTPCVolume;

            SoundModels = new SoundModel[layerConfig.Sounds.Length];
            for (var i = 0; i < layerConfig.Sounds.Length; i++)
            {
                var soundConfig = layerConfig.Sounds[i];

                if (!layerData.Sounds.TryGetValue(soundConfig.Name, out var soundData))
                {
                    soundData = new SoundModelData();
                    SetSoundDefaultData(soundData, soundConfig);
                    
                    layerData.Sounds.Add(soundConfig.Name, soundData);
                }

                var soundSwitch = GameConfig.Instance.SoundSwitchList[i];
                var soundGameObject = Utils.CreateSoundGameObject(soundConfig.Name, layerGameObject.transform, true);
                SoundModels[i] = new SoundModel(application, soundGameObject, PresetModel, this, soundConfig, soundData, soundSwitch);
            }
        }

        private void SetSoundDefaultData(SoundModelData soundData, GameConfig.SoundAssetData soundConfig)
        {
            soundData.Enabled = soundConfig.Enabled;
            soundData.VolumeLow = soundConfig.Volume.x;
            soundData.VolumeHigh = soundConfig.Volume.y;
        }

        public void Update()
        {
            foreach (var soundModel in SoundModels)
            {
                soundModel.Update();
            }
            
            if (_isWaitingDelay)
            {
                EstimatedDelay -= Time.deltaTime;
                
                if (EstimatedDelay <= 0)
                    SelectSound();
            }
        }
        
        public void SetVolume(float value)
        {
            LayerData.Volume = value;
            UpdateVolume();
        }
        
        public void UpdateVolume()
        {
            AkSoundEngine.SetRTPCValue(LayerRTPCVolume.Id, LayerData.Volume);
        }

        public void LayerTargetPlay()
        {
            PresetModel.SetCurrentPresetModelToApplication();
            
            PlayState = PresetsScreen.PlayState.TargetPlay;
            UpdateVolume();
            SelectSound();
        }

        public void LayerChildPlay()
        {
            if (!Enabled)
                return;
            
            PlayState = PresetsScreen.PlayState.ChildPlay;
            UpdateVolume();
            SelectSound();
        }

        public void LayerStop()
        {
            PlayState = PresetsScreen.PlayState.Normal;
            EstimatedDelay = 0;
            _isWaitingDelay = false;

            foreach (var soundModel in SoundModels)
            {
                if (soundModel.IsPlay)
                    soundModel.SoundStop();
            }
        }

        public void SoundFinished()
        {
            var isSoundEnabled = false;
            foreach (var soundModel in SoundModels)
            {
                if (soundModel.Enabled)
                {
                    isSoundEnabled = true;
                    break;
                }
            }

            if (!isSoundEnabled)
            {
                LayerStop();
                return;
            }
            
            _isWaitingDelay = true;
            Delay = EstimatedDelay = Random.Range(LayerData.DelayLow, LayerData.DelayHigh);
        }
        
        private void SelectSound()
        {
            EstimatedDelay = 0;
            _isWaitingDelay = false;
            
            if (_isRandom)
                RandomSound();
            // else
            // NextSound();
        }
        
        /*
        private void NextSound()
        {
            var soundItems = SoundsTable.GetAllItems();
            
            var newItem = _currentSoundItem;
            foreach (var soundItem in soundItems)
            {
                var item = (SoundTableItem) soundItem;
                
                if (!item.IsChildPlayPossible)
                    continue;
                
                if (item == _currentSoundItem)
                    continue;
            }

            var increaseSoundIndex = _soundIndex + 1;
            var index = increaseSoundIndex < soundItems.Count ? increaseSoundIndex : 0;
            SoundPlay(newItem);
        }
        */
        
        private void RandomSound()
        {
            var enabledSounds = new List<SoundModel>();
            foreach (var soundModel in SoundModels)
            {
                if (!soundModel.Enabled || soundModel == _currentPlayedSoundModel)
                    continue;
                
                enabledSounds.Add(soundModel);
            }

            if (enabledSounds.Count == 0 && (_currentPlayedSoundModel == null || !_currentPlayedSoundModel.Enabled))
            {
                LayerStop();
                return;
            }

            var newSound = _currentPlayedSoundModel;
            if (enabledSounds.Count == 1)
            {
                newSound = enabledSounds[0];
            }
            else if (enabledSounds.Count > 1)
            {
                var index = Random.Range(0, enabledSounds.Count);
                newSound = enabledSounds[index];
            }
            
            SoundPlay(newSound);
        }

        private void SoundPlay(SoundModel soundModel)
        {
            _currentPlayedSoundModel = soundModel;
            soundModel.SoundChildPlay();
        }

        public void ResetData()
        {
            foreach (var soundModel in SoundModels)
            {
                SetSoundDefaultData(soundModel.SoundData, soundModel.SoundConfig);
            }
        }
    }
}