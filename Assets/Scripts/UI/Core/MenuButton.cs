using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale")]
    [SerializeField, Min(1f)] private float hoverScale = 1.06f;
    [SerializeField, Min(0.01f)] private float scaleUpDuration = 0.16f;
    [SerializeField, Min(0.01f)] private float scaleDownDuration = 0.12f;

    [Header("Hover Punch")]
    [SerializeField, Min(0.01f)] private float punchDuration = 0.22f;
    [SerializeField, Min(0f)] private float punchStrength = 0.04f;
    [SerializeField, Min(1)] private int punchVibrato = 8;
    [SerializeField, Range(0f, 1f)] private float punchElasticity = 0.8f;

    [Header("Hover Shake")]
    [SerializeField, Min(0.01f)] private float shakeTotalDuration = 0.2f;
    [SerializeField, Min(0f)] private float shakeStrength = 2f;
    [SerializeField, Min(1)] private int shakeLoops = 10;

    [Header("Color")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Color normalColor = new(0.32f, 0.305f, 0.305f, 1f);
    [SerializeField] private Color hoverColor = new(0.4f, 0.38f, 0.38f, 1f);
    [SerializeField, Min(0.01f)] private float colorDuration = 0.12f;

    private Vector3 initialScale;
    private Quaternion initialRotation;
    private Tween scaleTween;
    private Tween punchTween;
    private Tween shakeTween;
    private Tween colorTween;

    private void Awake()
    {
        CacheReferences();
        initialScale = transform.localScale;
        initialRotation = transform.localRotation;
        ApplyNormalState();
    }

    private void OnDisable()
    {
        StopTweens();
        transform.localScale = initialScale;
        transform.localRotation = initialRotation;
        ApplyNormalState();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopTweens();
        transform.localRotation = initialRotation * Quaternion.Euler(0f, 0f, -shakeStrength);

        scaleTween = transform
            .DOScale(initialScale * hoverScale, scaleUpDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(PlayHoverPunch)
            .SetUpdate(true);

        shakeTween = transform
            .DOLocalRotate(new Vector3(0f, 0f, shakeStrength), GetShakeLoopDuration())
            .SetEase(Ease.InOutSine)
            .SetLoops(shakeLoops, LoopType.Yoyo)
            .OnComplete(() => transform.localRotation = initialRotation)
            .SetUpdate(true);

        if (!targetImage) return;

        colorTween = targetImage
            .DOColor(hoverColor, colorDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopTweens();
        transform.localRotation = initialRotation;

        scaleTween = transform
            .DOScale(initialScale, scaleDownDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);

        if (!targetImage) return;

        colorTween = targetImage
            .DOColor(normalColor, colorDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    private void CacheReferences()
    {
        if (!targetImage)
            targetImage = GetComponent<Image>();
    }

    private void ApplyNormalState()
    {
        if (targetImage)
            targetImage.color = normalColor;
    }

    private void StopTweens()
    {
        scaleTween?.Kill();
        scaleTween = null;

        punchTween?.Kill();
        punchTween = null;

        shakeTween?.Kill();
        shakeTween = null;

        colorTween?.Kill();
        colorTween = null;
    }

    private void PlayHoverPunch()
    {
        punchTween = transform
            .DOPunchScale(Vector3.one * punchStrength, punchDuration, punchVibrato, punchElasticity)
            .SetUpdate(true);
    }

    private float GetShakeLoopDuration()
    {
        return shakeLoops <= 0
            ? shakeTotalDuration
            : shakeTotalDuration / shakeLoops;
    }
}
