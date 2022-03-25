using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

namespace GameClient.Purchasing
{
    [RequireComponent(typeof(IAPButton))]
    public class IAPButtonExtended : MonoBehaviour
    {
        [SerializeField] 
        private IAPButton _iapButton;
        
        [SerializeField] 
        private TextMeshProUGUI _titleText;
        
        [SerializeField]
        private TextMeshProUGUI _descriptionText;
        
        [SerializeField]
        private TextMeshProUGUI _priceText;

        private void OnEnable()
        {
            if (_iapButton.buttonType == IAPButton.ButtonType.Purchase)
            {
                if (CodelessIAPStoreListener.initializationComplete) 
                {
                    UpdateText();
                }
            }
        }

        private void UpdateText()
        {
            var product = CodelessIAPStoreListener.Instance.GetProduct(_iapButton.productId);
            if (product != null)
            {
                if (_titleText != null)
                {
                    _titleText.text = product.metadata.localizedTitle;
                }

                if (_descriptionText != null)
                {
                    _descriptionText.text = product.metadata.localizedDescription;
                }
                
                if (_priceText != null)
                {
                    _priceText.text = product.metadata.localizedPriceString;
                }
            }
        }
    }
}