using System;
using System.Security.Cryptography;
using TheraBytes.BetterUi;
using UnityEngine;
using UnityEngine.UI;

public class PreserveAspectPanel : MonoBehaviour, IResolutionDependency
{
    [SerializeField]
    private CanvasScaler _canvasScaler;
    [SerializeField]
    private float _aspectRatio = 1.0f;

    [SerializeField]
    private bool _stretch = false;

    [SerializeField]
    private Vector2 _size = new Vector2();
    [SerializeField]
    private RectOffset _padding = new RectOffset();

    private RectTransform _rectTransform;

    void Awake ()
    {
        _rectTransform = GetComponent<RectTransform>();
        OnResolutionChanged();
    }

    void OnValidate()
    {
        OnResolutionChanged();
    }

    public void OnResolutionChanged ()
    {
        float canvasAspectRatio = 1.0f;
        if (_canvasScaler != null)
        {
            canvasAspectRatio = _canvasScaler.referenceResolution.y / ResolutionMonitor.CurrentResolution.y;
        }

        float width = (ResolutionMonitor.CurrentResolution.x * canvasAspectRatio) - _padding.left - _padding.right;
        float height = (ResolutionMonitor.CurrentResolution.y * canvasAspectRatio) - _padding.top - _padding.bottom;
        float aspectRatio = width / height;
        
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        if (aspectRatio > _aspectRatio)
        {
            _rectTransform.localScale = new Vector3(_size.y / height, _size.y / height, 1f);
            if (_stretch)
            {
                _rectTransform.sizeDelta = new Vector2(_size.x, _size.y / (_size.y / height));
            }
            //_rectTransform.sizeDelta = new Vector2(height * _aspectRatio, height);
        }
        else
        {
            _rectTransform.localScale = new Vector3(width / _size.x, width / _size.x, 1f);
            if (_stretch)
            {
                _rectTransform.sizeDelta = new Vector2(_size.x, _size.y / (width / _size.x));
            }
            // _rectTransform.sizeDelta = new Vector2(width, width / _aspectRatio);
        }
    }
}
