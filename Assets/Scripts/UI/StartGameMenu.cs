using System;
using UnityEngine;

public sealed class StartGameMenu : MenuViewBase
{
    [Header("References")]
    [SerializeField] private StartGameMenuView startGameMenu;
    [SerializeField] private OpponentSelectionView opponentSelection;
    [SerializeField] private SideSelectionView sideSelection;

    public event Action<MatchConfiguration> MatchConfigurationSelected;

    private PlayerTwoControlType selectedPlayerTwoControlType;

    protected override void Awake()
    {
        base.Awake();
        ValidateReferences();
        ShowStartGameMenu();
    }

    private void OnEnable()
    {
        if (startGameMenu)
        {
            startGameMenu.StartGameClicked += ShowOpponentSelection;
            startGameMenu.ExitGameClicked += GameGame;
        }

        if (opponentSelection)
            opponentSelection.PlayerTwoControlTypeSelected += SelectMode;

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
            startGameMenu.ExitGameClicked -= GameGame;
        }

        if (opponentSelection)
            opponentSelection.PlayerTwoControlTypeSelected -= SelectMode;

        if (sideSelection)
        {
            sideSelection.SideSelected -= SelectSide;
            sideSelection.BackButtonClicked -= ShowOpponentSelection;
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    protected override void HandleBeforeShow()
    {
        ShowStartGameMenu();
    }

    private void SelectMode(PlayerTwoControlType playerTwoControlType)
    {
        selectedPlayerTwoControlType = playerTwoControlType;
        startGameMenu?.Hide();
        opponentSelection?.Hide();
        sideSelection?.Show();
    }

    private void ShowStartGameMenu()
    {
        sideSelection?.HideImmediately();
        opponentSelection?.HideImmediately();
        startGameMenu?.Show();
    }

    private void ShowOpponentSelection()
    {
        startGameMenu?.Hide();
        sideSelection?.Hide();
        opponentSelection?.Show();
    }

    private void SelectSide(PlayerSide playerOneSide)
    {
        Hide();

        MatchConfigurationSelected?.Invoke(new MatchConfiguration(selectedPlayerTwoControlType, playerOneSide));
    }

    private static void GameGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
