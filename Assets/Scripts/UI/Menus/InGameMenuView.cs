using DG.Tweening;
using UnityEngine;

public sealed class InGameMenuView : MenuViewBase
{
    [Header("Appear Animation")]
    [SerializeField, Min(0f)] private float appearDelaySeconds;
    [SerializeField, Min(0.01f)] private float showDurationSeconds = 3f;
    [SerializeField, Min(0.01f)] private float hideDurationSeconds = 2f;
    [SerializeField] private Ease fadeEase = Ease.InBack;

    [Header("Scale Animation")]
    [SerializeField] private bool animateScale = true;
    [SerializeField] private Vector3 hiddenScale = new(0.65f, 0.65f, 1f);
    [SerializeField] private Vector3 visibleScale = Vector3.one;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    private Tween visibilityTween;

    private void OnDisable()
    {
        StopAnimation();
    }

    [ContextMenu("Show")]
    public new void Show()
    {
        base.Show();
    }

    [ContextMenu("Hide Immediately")]
    public new void HideImmediately()
    {
        base.HideImmediately();
    }

    protected override void HandleBeforeShow()
    {
        StopAnimation();
        SetCanvasVisibleState(0f, true);

        if (!animateScale) return;

        transform.localScale = hiddenScale;
    }

    protected override void HandleBeforeHide()
    {
        StopAnimation();
    }

    protected override void HandleAfterShow()
    {
        if (!animateScale) return;

        transform.localScale = visibleScale;
    }

    protected override void HandleAfterHide()
    {
        if (!animateScale) return;

        transform.localScale = hiddenScale;
    }

    protected override void PlayShowAnimation(System.Action onComplete)
    {
        if (!ResolvedCanvasGroup)
        {
            onComplete?.Invoke();
            return;
        }

        var duration = Mathf.Max(0.01f, showDurationSeconds);
        var sequence = DOTween.Sequence().SetUpdate(true);
        sequence.AppendInterval(Mathf.Max(0f, appearDelaySeconds));
        sequence.Append(ResolvedCanvasGroup.DOFade(1f, duration).SetEase(fadeEase));

        if (animateScale)
            sequence.Join(transform.DOScale(visibleScale, duration).SetEase(scaleEase));

        visibilityTween = sequence.OnComplete(() =>
        {
            visibilityTween = null;
            onComplete?.Invoke();
        });
    }

    protected override void PlayHideAnimation(System.Action onComplete)
    {
        if (!ResolvedCanvasGroup)
        {
            onComplete?.Invoke();
            return;
        }

        var duration = Mathf.Max(0.01f, hideDurationSeconds);
        var sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(ResolvedCanvasGroup.DOFade(0f, duration).SetEase(fadeEase));

        if (animateScale)
            sequence.Join(transform.DOScale(hiddenScale, duration).SetEase(scaleEase));

        visibilityTween = sequence.OnComplete(() =>
        {
            visibilityTween = null;
            onComplete?.Invoke();
        });
    }

    private void StopAnimation()
    {
        visibilityTween?.Kill();
        visibilityTween = null;
    }
}
