using Newtonsoft.Json;
using UnityEngine;

namespace GameClient
{
    public class SoundModelData
    {
        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled;
        
        [JsonProperty(PropertyName = "volume_low")]
        public float VolumeLow;
        
        [JsonProperty(PropertyName = "volume_high")]
        public float VolumeHigh;
    }
    
    public class SoundModel
    {
        public readonly Application Application;
        public GameObject SoundGameObject;
        public readonly PresetModel PresetModel;
        public readonly LayerModel LayerModel;
        public readonly GameConfig.SoundAssetData SoundConfig;
        public readonly AK.Wwise.Switch SoundSwitch;

        public bool IsPlay => PlayState == PresetsScreen.PlayState.TargetPlay || PlayState == PresetsScreen.PlayState.ChildPlay;

        public SoundModelData SoundData;
        public PresetsScreen.PlayState PlayState;
        public float DurationSound;
        public float EstimatedDurationSound;
        public float EstimatedDelay;
        
        public bool Enabled
        {
            get => SoundData.Enabled;
            set
            {
                if (SoundData.Enabled == value)
                    return;

                SoundData.Enabled = value;
                
                if (!SoundData.Enabled && PlayState == PresetsScreen.PlayState.ChildPlay)
                {
                    SoundStop();
                    LayerModel.SoundFinished();
                }
                else if (SoundData.Enabled && LayerModel.PlayState == PresetsScreen.PlayState.Normal && PresetModel.IsPlay)
                {
                    LayerModel.LayerChildPlay();
                }
            }
        }
        
        private bool _isWaitingDelay;
        
        public const float Delay = 2f;

        public SoundModel(Application application, GameObject soundGameObject, PresetModel presetModel, LayerModel layerModel, GameConfig.SoundAssetData soundConfig, SoundModelData soundData, AK.Wwise.Switch soundSwitch)
        {
            Application = application;
            SoundGameObject = soundGameObject;
            PresetModel = presetModel;
            LayerModel = layerModel;
            SoundConfig = soundConfig;
            SoundData = soundData;
            SoundSwitch = soundSwitch;
        }
        
        public void Update()
        {
            if (!IsPlay)
                return;
            
            if (EstimatedDurationSound > 0)
            {
                EstimatedDurationSound -= Time.deltaTime;
            }
            else if (EstimatedDelay > 0)
            {
                EstimatedDelay -= Time.deltaTime;
            }

            switch (PlayState)
            {
                case PresetsScreen.PlayState.Normal:
                    break;
                case PresetsScreen.PlayState.TargetPlay:
                    
                    if (EstimatedDurationSound <= 0 && !_isWaitingDelay)
                    {
                        _isWaitingDelay = true;
                        EstimatedDelay = Delay;
                    }
                    else if (EstimatedDelay <= 0 && _isWaitingDelay)
                    {
                        SoundTargetPlay();
                    }
                    
                    break;
                case PresetsScreen.PlayState.ChildPlay:
                    
                    if (EstimatedDurationSound <= 0)
                    {
                        SoundStop();
                        LayerModel.SoundFinished();
                    }
                    
                    break;
                default:
                    Debug.LogError("Error");
                    break;
            }
        }

        public void SoundTargetPlay()
        {
            PresetModel.SetCurrentPresetModelToApplication();
            
            PlayState = PresetsScreen.PlayState.TargetPlay;
            _isWaitingDelay = false;
            EstimatedDelay = 0f;
            SetRTPC();
            Play();
        }

        public void SoundChildPlay()
        {
            PlayState = PresetsScreen.PlayState.ChildPlay;
            SetRTPC();
            Play();
        }

        public void SoundStop()
        {
            if (PlayState == PresetsScreen.PlayState.Normal)
                return;
            
            PlayState = PresetsScreen.PlayState.Normal;
            Stop();
        }
        
        private void SetRTPC()
        {
            var volume = Random.Range(SoundData.VolumeLow, SoundData.VolumeHigh);
            AkSoundEngine.SetRTPCValue(GameConfig.Instance.SoundRTPCVolume.Id, volume, SoundGameObject);
        }

        private void Play()
        {
            var presetSwitch = PresetModel.PresetSwitch;
            var layerSwitch = LayerModel.LayerSwitch;
            var soundSwitch = SoundSwitch;

            AkSoundEngine.SetSwitch(presetSwitch.GroupId, presetSwitch.Id, SoundGameObject);
            AkSoundEngine.SetSwitch(layerSwitch.GroupId, layerSwitch.Id, SoundGameObject);
            AkSoundEngine.SetSwitch(soundSwitch.GroupId, soundSwitch.Id, SoundGameObject);

            DurationSound = EstimatedDurationSound = 99999f;
            Update();
            
            AkSoundEngine.PostEvent(GameConfig.Instance.SoundPlayEvent.Id, SoundGameObject, (uint)AkCallbackType.AK_Duration, (cookie, type, info) =>
            {
                var durationInfo = (AkDurationCallbackInfo)info;
                DurationSound = EstimatedDurationSound = durationInfo.fEstimatedDuration / 1000f;

            }, null);
        }

        private void Stop()
        {
            DurationSound = 0;
            EstimatedDurationSound = 0;
            EstimatedDelay = 0;
            _isWaitingDelay = false;
            AkSoundEngine.PostEvent(GameConfig.Instance.SoundStopEvent.Id, SoundGameObject);
        }
    }
}