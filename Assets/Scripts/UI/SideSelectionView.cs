using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class SideSelectionView : MenuViewBase
{
    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.2f;
    [SerializeField] private Ease fadeInEase = Ease.OutSine;
    [SerializeField] private Ease fadeOutEase = Ease.InSine;

    [Header("Buttons")]
    [SerializeField] private Button leftSideButton;
    [SerializeField] private Button rightSideButton;
    [SerializeField] private Button backButton;

    public event Action<PlayerSide> SideSelected;
    public event Action BackButtonClicked;

    private Tween fadeTween;

    private void OnEnable()
    {
        AddButtonListeners();
        SetInteractable(true);
    }

    private void OnDisable()
    {
        StopFadeTween();
        RemoveButtonListeners();
        SetInteractable(false);
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    protected override void HandleBeforeShow()
    {
        SetInteractable(true);
    }

    protected override void HandleBeforeHide()
    {
        SetInteractable(false);
    }

    protected override void HandleAfterInitialize()
    {
        ValidateReferences();
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
            HandleFadeCompleted(onComplete));
    }

    protected override void PlayHideAnimation(Action onComplete)
    {
        StopFadeTween();
        fadeTween = MenuAnimationsHelper.PlayCanvasGroupFade(
            ResolvedCanvasGroup,
            1f,
            0f,
            fadeOutDuration,
            fadeOutEase,
            HandleFadeCompleted(onComplete));
    }

    private void SelectLeftSide()
    {
        SideSelected?.Invoke(PlayerSide.Left);
    }

    private void SelectRightSide()
    {
        SideSelected?.Invoke(PlayerSide.Right);
    }

    private void AddButtonListeners()
    {
        if (leftSideButton)
        {
            leftSideButton.onClick.RemoveListener(SelectLeftSide);
            leftSideButton.onClick.AddListener(SelectLeftSide);
        }

        if (rightSideButton)
        {
            rightSideButton.onClick.RemoveListener(SelectRightSide);
            rightSideButton.onClick.AddListener(SelectRightSide);
        }

        if (backButton)
        {
            backButton.onClick.RemoveListener(HandleBackClicked);
            backButton.onClick.AddListener(HandleBackClicked);
        }
    }

    private void RemoveButtonListeners()
    {
        if (leftSideButton)
            leftSideButton.onClick.RemoveListener(SelectLeftSide);

        if (rightSideButton)
            rightSideButton.onClick.RemoveListener(SelectRightSide);

        if (backButton)
            backButton.onClick.RemoveListener(HandleBackClicked);
    }

    private void SetInteractable(bool interactable)
    {
        if (leftSideButton)
            leftSideButton.interactable = interactable;

        if (rightSideButton)
            rightSideButton.interactable = interactable;

        if (backButton)
            backButton.interactable = interactable;
    }

    private void HandleBackClicked()
    {
        BackButtonClicked?.Invoke();
    }

    private void StopFadeTween()
    {
        fadeTween?.Kill();
        fadeTween = null;
    }

    private Action HandleFadeCompleted(Action onComplete)
    {
        return () =>
        {
            fadeTween = null;
            onComplete?.Invoke();
        };
    }

    private void ValidateReferences()
    {
        if (!leftSideButton)
            Debug.LogError($"{nameof(SideSelectionView)} requires a left side button reference.", this);

        if (!rightSideButton)
            Debug.LogError($"{nameof(SideSelectionView)} requires a right side button reference.", this);

        if (!backButton)
            Debug.LogError($"{nameof(SideSelectionView)} requires a back button reference.", this);
    }
}
