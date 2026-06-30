using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class StrikerTweenAnimator : MonoBehaviour
{
    [Header("Appear Animation")]
    [SerializeField, Min(0f)] private float appearAnimationDuration = 1f;
    [SerializeField, Min(0f)] private float appearStartScaleMultiplier = 0.001f;
    [SerializeField, Range(0f, 1f)] private float appearStartAlpha = 0.01f;
    [SerializeField] private Ease appearAnimationEase = Ease.OutBack;
    [SerializeField] private Vector3 appearShakeOffset = new(0.05f, 0.05f, 0f);
    [SerializeField, Min(0)] private int appearShakeVibrato = 12;
    [SerializeField, Range(0f, 1f)] private float appearShakeElasticity = 0.5f;
    [SerializeField, Range(0f, 1f)] private float appearShakeStartDelayNormalized = 0.1f;

    [Header("References")]
    [SerializeField] private SpriteRenderer targetSpriteRenderer;

    private Sequence activeSequence;
    
    private Vector3 defaultLocalScale;
    private Vector3 animationLocalPosition;
    private float defaultAlpha = 1f;
    
    private bool hasCachedReferences;
    private bool hasCachedDefaultVisualState;
    private bool readyToPlayAnimation;

    private void Reset()
    {
        if (!targetSpriteRenderer)
            targetSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        TryInitialize();
    }

    public void PrepareAppearAnimation()
    {
        if (!TryInitialize()) return;

        StopActiveAnimation();
        readyToPlayAnimation = true;
        ApplyAppearStartVisualState();
    }

    public void PlayAppearAnimation()
    {
        if (!readyToPlayAnimation) return;
        if (!TryInitialize()) return;

        readyToPlayAnimation = false;
        StopActiveAnimation();
        CaptureAnimationPosition();

        if (appearAnimationDuration <= 0f)
        {
            ApplyVisibleVisualState();
            return;
        }

        ApplyAppearStartVisualState();
        LaunchAppearAnimation();
    }

    public void ResetStrikerVisualState()
    {
        if (!TryInitialize()) return;

        StopActiveAnimation();
        readyToPlayAnimation = false;
        ApplyVisibleVisualState();
    }

    private void OnDestroy()
    {
        StopActiveAnimation();
    }

    private bool TryInitialize()
    {
        var refsCached = TryCacheReferences();
        var defaultVisualStateCached = TryCacheDefaultVisualState();
        return refsCached && defaultVisualStateCached;
    }

    private bool TryCacheReferences()
    {
        if (hasCachedReferences) return true;

        if (!targetSpriteRenderer && !TryGetComponent(out targetSpriteRenderer))
        {
            Debug.LogError($"{nameof(StrikerTweenAnimator)} on {name} requires a {nameof(SpriteRenderer)} component.", this);
            return false;
        }

        hasCachedReferences = true;
        return true;
    }

    private bool TryCacheDefaultVisualState()
    {
        if (hasCachedDefaultVisualState) return true;
        if (!hasCachedReferences) return false;

        defaultLocalScale = transform.localScale;
        defaultAlpha = targetSpriteRenderer.color.a;
        hasCachedDefaultVisualState = true;
        return true;
    }

    private void CaptureAnimationPosition()
    {
        animationLocalPosition = transform.localPosition;
    }

    private void ApplyAppearStartVisualState()
    {
        transform.localScale = defaultLocalScale * appearStartScaleMultiplier;
        SetSpriteAlpha(appearStartAlpha);
    }

    private void ApplyVisibleVisualState()
    {
        transform.localScale = defaultLocalScale;
        SetSpriteAlpha(defaultAlpha);
    }

    private void RestoreAnimationPosition()
    {
        transform.localPosition = animationLocalPosition;
    }

    private void LaunchAppearAnimation()
    {
        var duration = appearAnimationDuration;
        var shakeDelay = duration * Mathf.Clamp01(appearShakeStartDelayNormalized);

        activeSequence = DOTween.Sequence();
        activeSequence.Join(transform.DOScale(defaultLocalScale, duration).SetEase(appearAnimationEase));
        activeSequence.Join(targetSpriteRenderer.DOFade(defaultAlpha, duration).SetEase(appearAnimationEase));
        activeSequence.Join(transform.DOPunchPosition(
                appearShakeOffset,
                duration, 
                Mathf.Max(0, appearShakeVibrato),
                appearShakeElasticity)
            .SetDelay(shakeDelay));
        
        activeSequence.OnComplete(HandleAppearAnimationCompleted);
    }

    private void HandleAppearAnimationCompleted()
    {
        RestoreAnimationPosition();
        ApplyVisibleVisualState();
        StopActiveAnimation();
    }

    private void SetSpriteAlpha(float alpha)
    {
        var color = targetSpriteRenderer.color;
        color.a = alpha;
        targetSpriteRenderer.color = color;
    }

    private void StopActiveAnimation()
    {
        activeSequence?.Kill();
        activeSequence = null;
    }
}
