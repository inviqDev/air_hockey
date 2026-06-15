using DG.Tweening;
using UnityEngine;

public sealed class PuckScaleController : IPuckScaleController
{
    private const float DefaultScaleMultiplier = 1f;

    private readonly Transform targetTransform;
    private readonly Vector3 defaultLocalScale;

    private Tween activeScaleTween;
    private bool isDownscaled;

    public PuckScaleController(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
        defaultLocalScale = targetTransform ? targetTransform.localScale : Vector3.one;
    }

    public bool CanToggleScale => targetTransform;

    public void ToggleScale(float downscaledScaleMultiplier, float animationDuration, Ease animationEase)
    {
        if (!CanToggleScale) return;

        var targetMultiplier = isDownscaled
            ? DefaultScaleMultiplier
            : Mathf.Max(0.01f, downscaledScaleMultiplier);

        isDownscaled = !isDownscaled;
        ApplyScale(targetMultiplier, animationDuration, animationEase);
    }

    public void ResetScale()
    {
        if (!CanToggleScale) return;

        isDownscaled = false;
        KillActiveTween();
        targetTransform.localScale = defaultLocalScale;
    }

    private void ApplyScale(float targetMultiplier, float animationDuration, Ease animationEase)
    {
        KillActiveTween();

        var targetScale = defaultLocalScale * targetMultiplier;
        if (animationDuration <= 0f)
        {
            targetTransform.localScale = targetScale;
            return;
        }

        activeScaleTween = targetTransform
            .DOScale(targetScale, animationDuration)
            .SetEase(animationEase);
    }

    private void KillActiveTween()
    {
        if (activeScaleTween == null) return;
        
        activeScaleTween?.Kill();
        activeScaleTween = null;
    }
}
