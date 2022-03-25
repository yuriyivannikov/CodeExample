using UnityEngine;

namespace GameClient
{
    public class TimerModel
    {
        public readonly Application Application;

        public bool IsTimerStarted { get; private set; }
        public float LeftTime;

        private float AttenuationVolume
        {
            get
            {
                if (LeftTime <= 0 || Application.ApplicationData.AttenuationValue <= 0)
                    return 1f;
                
                const int secondsPerMinute = 60;
                var attenuationTime = Application.ApplicationData.AttenuationValue * secondsPerMinute;
                if (LeftTime > attenuationTime)
                    return 1f;
                
                if (attenuationTime > _beginLeftTime)
                    return LeftTime / _beginLeftTime;
                
                return LeftTime / attenuationTime;
            }
        }
        
        private float _beginLeftTime;
        
        public TimerModel(Application application)
        {
            Application = application;
        }

        public void Update()
        {
            if (!IsTimerStarted)
                return;
            
            LeftTime -= Time.deltaTime;

            if (LeftTime <= 0)
            {
                TimerEnd();
            }
            else
            {
                if (Application.ApplicationData.AttenuationValue <= 0) 
                    return;
                
                var attenuationVolume = AttenuationVolume;
                if (attenuationVolume < 1)
                {
                    SetVolume(AttenuationVolume);
                }
            }
        }
        
        public void TimerStart()
        {
            if (LeftTime <= 0)
                return;
            
            IsTimerStarted = true;
            _beginLeftTime = LeftTime;
        }

        public void TimerReset()
        {
            IsTimerStarted = false;
            LeftTime = 0;
            SetVolume(1);
        }

        public void TimerEnd()
        {
            TimerReset();
            Application.StopAllSound();
        }

        private static void SetVolume(float value)
        {
            const int multiplier = 100;
            AkSoundEngine.SetRTPCValue(GameConfig.Instance.ApplicationBusRTPCVolume.Id, value * multiplier);
        }
    }
}