using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class SureExitView : MenuViewBase
{
    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.35f;
    [SerializeField] private Ease fadeInEase = Ease.OutSine;
    [SerializeField] private Ease fadeOutEase = Ease.InSine;

    [Header("Exit Flow")]
    [SerializeField, Min(0.01f)] private float exitTransitionDuration = 2f;
    [SerializeField] private RectTransform yesButtonRoot;
    [SerializeField] private RectTransform noButtonRoot;

    public event Action ExitCancelled;

    private Tween fadeTween;
    private bool shouldQuitAfterHide;
    private Button yesButton;
    private Button noButton;

    private void OnEnable()
    {
        ResolveButtons();

        if (yesButton)
            yesButton.onClick.AddListener(HandleYesClicked);

        if (noButton)
            noButton.onClick.AddListener(HandleNoClicked);
    }

    private void OnDisable()
    {
        if (yesButton)
            yesButton.onClick.RemoveListener(HandleYesClicked);

        if (noButton)
            noButton.onClick.RemoveListener(HandleNoClicked);

        StopFadeTween();
    }

    private void OnValidate()
    {
        ResolveButtons();
    }

    protected override void HandleAfterInitialize()
    {
        ResolveButtons();
        ValidateReferences();
    }

    public void ShowConfirmation()
    {
        shouldQuitAfterHide = false;
        Show();
    }

    public void ConfirmExit()
    {
        shouldQuitAfterHide = true;
        Hide();
    }

    public void CancelExit()
    {
        shouldQuitAfterHide = false;
        StopFadeTween();
        HideImmediately();
    }

    protected override void PlayShowAnimation(Action onComplete)
    {
        StopFadeTween();
        fadeTween = MenuAnimationsHelper.PlayCanvasGroupFade(
            ResolvedCanvasGroup,
            0f,
            1f,
            fadeInDuration,
            fadeInEase,
            HandleFadeCompleted(onComplete),
            true);
    }

    protected override void PlayHideAnimation(Action onComplete)
    {
        StopFadeTween();

        var hideDuration = shouldQuitAfterHide ? exitTransitionDuration : fadeOutDuration;

        fadeTween = MenuAnimationsHelper.PlayCanvasGroupFade(
            ResolvedCanvasGroup,
            1f,
            0f,
            hideDuration,
            fadeOutEase,
            HandleFadeCompleted(onComplete),
            true);
    }

    protected override void HandleAfterHide()
    {
        if (!shouldQuitAfterHide) return;

        shouldQuitAfterHide = false;
        QuitApplication();
    }

    private void StopFadeTween()
    {
        fadeTween?.Kill();
        fadeTween = null;
    }

    private void ResolveButtons()
    {
        yesButton = ResolveButton(yesButtonRoot);
        noButton = ResolveButton(noButtonRoot);
    }

    private void HandleYesClicked()
    {
        ConfirmExit();
    }

    private void HandleNoClicked()
    {
        CancelExit();
        ExitCancelled?.Invoke();
    }

    private Action HandleFadeCompleted(Action onComplete)
    {
        return () =>
        {
            fadeTween = null;
            onComplete?.Invoke();
        };
    }

    private static void QuitApplication()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ValidateReferences()
    {
        if (!yesButtonRoot)
            Debug.LogError($"{nameof(SureExitView)} requires a Yes button root reference.", this);

        if (!noButtonRoot)
            Debug.LogError($"{nameof(SureExitView)} requires a No button root reference.", this);

        if (!yesButton)
            Debug.LogError($"{nameof(SureExitView)} could not resolve the Yes button component.", this);

        if (!noButton)
            Debug.LogError($"{nameof(SureExitView)} could not resolve the No button component.", this);
    }

    private static Button ResolveButton(Component buttonRoot)
    {
        if (!buttonRoot) return null;
        return buttonRoot.GetComponent<Button>();
    }
}
