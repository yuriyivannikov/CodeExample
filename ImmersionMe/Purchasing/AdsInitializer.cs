using UnityEngine;
using UnityEngine.Advertisements;

namespace GameClient.Purchasing
{
    public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
    {
        
        [SerializeField] private string _androidGameId = "3638755";
        [SerializeField] private string _iOsGameId = "3638754";
        [SerializeField] private bool _testMode = true;
        
        private string _gameId;

        private void Awake()
        {
            InitializeAds();
        }

        public void InitializeAds()
        {
            _gameId = (UnityEngine.Application.platform == RuntimePlatform.IPhonePlayer) ? _iOsGameId : _androidGameId;
            Advertisement.Initialize(_gameId, _testMode);
        }

        
        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads initialization complete.");
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        }
    }
}