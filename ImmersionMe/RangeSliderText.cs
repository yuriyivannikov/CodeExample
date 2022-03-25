using UIKit;
using UnityEngine;

public class RangeSliderText : MonoBehaviour
{
    public UIKitRangeSlider RangeSlider;
    
    public TextStyleComponent LowValueText;
    public TextStyleComponent HighValueText;

    private bool _isInit;

    public void OnEnable()
    {
        if (RangeSlider != null && LowValueText != null && HighValueText != null)
        _isInit = true;
    }

    public void Update()
    {
        if (!_isInit)
            return;
        
        LowValueText.text = RangeSlider.LowValue.ToString();
        HighValueText.text = RangeSlider.HighValue.ToString();
    }
}
