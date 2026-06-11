using System;
using DG.Tweening;
using UnityEngine;

public static class MenuAnimationsHelper
{
    public static Tween PlayCanvasGroupFade(
        CanvasGroup canvasGroup,
        float fromAlpha,
        float toAlpha,
        float duration,
        Ease ease,
        Action onComplete,
        bool useUnscaledTime = false)
    {
        if (!canvasGroup)
        {
            onComplete?.Invoke();
            return null;
        }

        canvasGroup.alpha = fromAlpha;

        return canvasGroup
            .DOFade(toAlpha, duration)
            .SetEase(ease)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() => onComplete?.Invoke());
    }
}
