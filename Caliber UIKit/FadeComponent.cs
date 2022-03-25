using System;
using System.Collections;
using TheraBytes.BetterUi;
using UnityEngine;

public class FadeComponent : MonoBehaviour
{
    [SerializeField]
    private BetterImage _backgroundImage;

    private void Awake()
    {
        var transparentColor = new Color(_backgroundImage.color.r, _backgroundImage.color.g, _backgroundImage.color.b, 0f);
        _backgroundImage.color = transparentColor;
    }

    public void StartFade(float fadeInDuration, float fadeOutDuration, Sprite sprite, Action fadeInCompleteAction = null, Action fadeOutCompleteAction = null)
    {
        var coroutine = FadeCoroutine(fadeInDuration, fadeOutDuration, sprite, fadeInCompleteAction, fadeOutCompleteAction);
        StartCoroutine(coroutine);
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    private IEnumerator FadeCoroutine(float fadeInDuration, float fadeOutDuration, Sprite sprite, Action fadeInCompleteAction, Action fadeOutCompleteAction)
    {
        _backgroundImage.sprite = sprite;
        if (sprite != null)
        {
            _backgroundImage.color = Color.white;
        }
//        _backgroundImage.DOFade(1f, fadeInDuration);

        yield return new WaitForSeconds(fadeInDuration);

        fadeInCompleteAction?.Invoke();

//        _backgroundImage.DOFade(0f, fadeOutDuration);

        yield return new WaitForSeconds(fadeOutDuration);

        fadeOutCompleteAction?.Invoke();

        Destroy(gameObject);
    }
}