using System;
using DG.Tweening;
using UnityEngine;

public sealed class StartGameMenu : MenuViewBase
{
    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.2f;
    [SerializeField] private Ease fadeInEase = Ease.OutSine;
    [SerializeField] private Ease fadeOutEase = Ease.InSine;

    [Header("References")]
    [SerializeField] private StartGameMenuView startGameMenu;
    [SerializeField] private OpponentSelectionView opponentSelection;
    [SerializeField] private SideSelectionView sideSelection;

    public event Action<MatchConfiguration> MatchConfigurationSelected;

    private PlayerTwoControlType playerTwoControlType;
    private Tween fadeTween;

    private void OnEnable()
    {
        if (startGameMenu)
        {
            startGameMenu.StartGameClicked += ShowOpponentSelection;
            startGameMenu.ExitGameClicked += ExitGame;
        }

        if (opponentSelection)
        {
            opponentSelection.PlayerTwoControlTypeSelected += SelectOpponentMode;
            opponentSelection.BackButtonClicked += ShowStartGameMenu;
        }

        if (sideSelection)
        {
            sideSelection.SideSelected += SelectSide;
            sideSelection.BackButtonClicked += ShowOpponentSelection;
        }
    }

    private void OnDisable()
    {
        if (startGameMenu)
        {
            startGameMenu.StartGameClicked -= ShowOpponentSelection;
            startGameMenu.ExitGameClicked -= ExitGame;
        }

        if (opponentSelection)
        {
            opponentSelection.PlayerTwoControlTypeSelected -= SelectOpponentMode;
            opponentSelection.BackButtonClicked -= ShowStartGameMenu;
        }

        if (sideSelection)
        {
            sideSelection.SideSelected -= SelectSide;
            sideSelection.BackButtonClicked -= ShowOpponentSelection;
        }

        StopFadeTween();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    protected override void HandleBeforeShow()
    {
        ShowStartGameMenu();
    }

    protected override void HandleAfterInitialize()
    {
        ValidateReferences();
        ShowStartGameMenu();
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
        fadeTween = MenuAnimationsHelper.PlayCanvasGroupFade(
            ResolvedCanvasGroup,
            1f,
            0f,
            fadeOutDuration,
            fadeOutEase,
            HandleFadeCompleted(onComplete),
            true);
    }

    private void SelectOpponentMode(PlayerTwoControlType selectedPlayerTwoControlType)
    {
        playerTwoControlType = selectedPlayerTwoControlType;
        ShowOnly(sideSelection);
    }

    private void ShowStartGameMenu()
    {
        ShowOnly(startGameMenu);
    }

    private void ShowOpponentSelection()
    {
        ShowOnly(opponentSelection);
    }

    private void SelectSide(PlayerSide playerOneSide)
    {
        Hide();

        var configuration = new MatchConfiguration(playerTwoControlType, playerOneSide);
        MatchConfigurationSelected?.Invoke(configuration);
    }

    private static void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowOnly(MenuViewBase target)
    {
        SetSubmenuVisibility(startGameMenu, target == startGameMenu);
        SetSubmenuVisibility(opponentSelection, target == opponentSelection);
        SetSubmenuVisibility(sideSelection, target == sideSelection);
    }

    private static void SetSubmenuVisibility(MenuViewBase view, bool shouldShow)
    {
        if (!view) return;

        if (shouldShow)
            view.Show();
        else
            view.HideImmediately();
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
        if (!startGameMenu)
            Debug.LogError($"{nameof(StartGameMenu)} requires a start/exit menu reference.", this);

        if (!opponentSelection)
            Debug.LogError($"{nameof(StartGameMenu)} requires an opponent selection reference.", this);

        if (!sideSelection)
            Debug.LogError($"{nameof(StartGameMenu)} requires a side selection reference.", this);
    }
}
