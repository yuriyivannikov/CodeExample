using UnityEngine;

namespace GameClient
{
    public static class Utils
    {
        public static GameObject CreateSoundGameObject(string name, Transform parent, bool isAkGameObj = false)
        {
            var soundGameObject = new GameObject(name);
            soundGameObject.transform.SetParent(parent);

            if (isAkGameObj)
            {
                var component = soundGameObject.AddComponent<AkGameObj>();
                component.isEnvironmentAware = false;
            }

            return soundGameObject;
        }
    }
}