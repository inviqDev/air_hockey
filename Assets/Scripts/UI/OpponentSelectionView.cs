using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class OpponentSelectionView : MenuViewBase
{
    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.2f;
    [SerializeField] private Ease fadeInEase = Ease.OutSine;
    [SerializeField] private Ease fadeOutEase = Ease.InSine;

    [Header("Buttons")]
    [SerializeField] private Button aiOpponentButton;
    [SerializeField] private Button secondPlayerButton;
    [SerializeField] private Button backButton;

    public event Action<PlayerTwoControlType> PlayerTwoControlTypeSelected;
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

    private void SelectAiOpponent()
    {
        PlayerTwoControlTypeSelected?.Invoke(PlayerTwoControlType.Ai);
    }

    private void SelectSecondPlayer()
    {
        PlayerTwoControlTypeSelected?.Invoke(PlayerTwoControlType.Human);
    }

    private void AddButtonListeners()
    {
        if (aiOpponentButton)
        {
            aiOpponentButton.onClick.RemoveListener(SelectAiOpponent);
            aiOpponentButton.onClick.AddListener(SelectAiOpponent);
        }

        if (secondPlayerButton)
        {
            secondPlayerButton.onClick.RemoveListener(SelectSecondPlayer);
            secondPlayerButton.onClick.AddListener(SelectSecondPlayer);
        }

        if (backButton)
        {
            backButton.onClick.RemoveListener(HandleBackClicked);
            backButton.onClick.AddListener(HandleBackClicked);
        }
    }

    private void RemoveButtonListeners()
    {
        if (aiOpponentButton)
            aiOpponentButton.onClick.RemoveListener(SelectAiOpponent);

        if (secondPlayerButton)
            secondPlayerButton.onClick.RemoveListener(SelectSecondPlayer);

        if (backButton)
            backButton.onClick.RemoveListener(HandleBackClicked);
    }

    private void SetInteractable(bool interactable)
    {
        if (aiOpponentButton)
            aiOpponentButton.interactable = interactable;

        if (secondPlayerButton)
            secondPlayerButton.interactable = interactable;

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
        if (!aiOpponentButton)
            Debug.LogError($"{nameof(OpponentSelectionView)} requires an AI opponent button reference.", this);

        if (!secondPlayerButton)
            Debug.LogError($"{nameof(OpponentSelectionView)} requires a second player button reference.", this);

        if (!backButton)
            Debug.LogError($"{nameof(OpponentSelectionView)} requires a back button reference.", this);
    }
}
