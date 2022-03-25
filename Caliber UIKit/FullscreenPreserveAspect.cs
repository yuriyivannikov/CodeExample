using System;
using TheraBytes.BetterUi;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenPreserveAspect : MonoBehaviour, IResolutionDependency
{
    private CanvasScaler _canvasScaler;
    private CanvasScaler CanvasScaler
    {
        get
        {
            if(_canvasScaler == null)
                _canvasScaler = GetComponentInParent<CanvasScaler>();
            return _canvasScaler;
        }
    }
    
    private BetterImage _image;
    private BetterImage Image
    {
        get
        {
            if(_image == null)
                _image = GetComponent<BetterImage>();
            return _image;
        }
    }

    private RectTransform _rectTransform;
    private RectTransform RectTransform
    {
        get
        {
            if(_rectTransform == null)
                _rectTransform = transform as RectTransform;
            return _rectTransform;
        }
    }
    
    //private Texture2D fullscreenTexture2D;
    //private Vector2 initialSizeDelta;

    void Awake ()
    {
        //fullscreenTexture2D = GetComponent<BetterImage>().sprite.texture;
        //initialSizeDelta = GetComponent<RectTransform>().sizeDelta;
    }

    private void OnEnable()
    {
        OnResolutionChanged();
    }

    public void OnResolutionChanged()
    {
        if (Image == null || Image.sprite == null)
        {
            Debug.LogError("BetterImage sprite is null");
            return;
        }

        float k = 1;
        if (CanvasScaler != null)
        {
            if (CanvasScaler.matchWidthOrHeight > 0)
                k = CanvasScaler.referenceResolution.y / ResolutionMonitor.CurrentResolution.y;
            else
                k = CanvasScaler.referenceResolution.x / ResolutionMonitor.CurrentResolution.x;
        }

        float width = ResolutionMonitor.CurrentResolution.x * k;
        float height = ResolutionMonitor.CurrentResolution.y * k;
        
        float spriteWidth = Image.sprite.texture.width;
        float spriteHeight = Image.sprite.texture.height;
        
        float texRatio = spriteWidth / spriteHeight;
        float rectRatio = width / height;
        
        if (rectRatio > texRatio)
        {
            height = width / spriteWidth * spriteHeight;
        }
        else
        {
            width = height / spriteHeight * spriteWidth;
        }
        
        RectTransform.sizeDelta = new Vector2(width, height);
        
        /*
        var rectTransform = GetComponent<RectTransform>();
        var normalAspectRatio = (Single)fullscreenTexture2D.width / fullscreenTexture2D.height;
        var aspectRatio = ResolutionMonitor.CurrentResolution.x / ResolutionMonitor.CurrentResolution.y;

        if (aspectRatio > normalAspectRatio)
        {
            var width = fullscreenTexture2D.height / ResolutionMonitor.CurrentResolution.y * 1.001f * ResolutionMonitor.CurrentResolution.x;
            var height = width / fullscreenTexture2D.width * fullscreenTexture2D.height;

            rectTransform.sizeDelta = new Vector2(width, height);
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(initialSizeDelta.x, initialSizeDelta.y);
        }
        */
    }
}
