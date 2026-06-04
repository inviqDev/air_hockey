using UnityEngine;

public sealed class MatchManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoundController roundController;
    [SerializeField] private TurnController turnController;
    [SerializeField] private ServeManager serveManager;
    [SerializeField] private GoalController goalController;
    [SerializeField] private ScoreKeeper scoreKeeper;

    public int LeftScore => goalController ? goalController.LeftScore : 0;
    public int RightScore => goalController ? goalController.RightScore : 0;
    public bool IsTurnActive => turnController && turnController.IsTurnActive;
    public bool HasActiveMatch { get; private set; }

    private UIManager uiManager;

    private MatchConfiguration currentConfiguration;
    private bool hasCurrentConfiguration;

    private bool isInitialized;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnEnable()
    {
        if (!isInitialized) return;
        SubscribeToGameFlow();
    }

    private void OnDisable()
    {
        if (!isInitialized) return;

        UnsubscribeFromGameFlow();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void InitializeGameStart(UIManager rootUiManager)
    {
        if (isInitialized) return;
        ValidateReferences();
        uiManager = rootUiManager;
        if (!uiManager) return;

        SubscribeToGameFlow();
        isInitialized = true;
    }

    private void PrepareNextTurn()
    {
        if (!turnController) return;
        turnController.PrepareTurn(ResetRoundAndStatus);
    }

    private bool ResetRoundAndStatus()
    {
        if (roundController)
            roundController.ResetRound();
        
        if (uiManager)
            uiManager.ClearGoalPopUpText();

        return roundController && roundController.HasAllGameItemsSpawned;
    }

    private void HandleGoalResult(GoalResult result)
    {
        if (turnController)
            turnController.EndTurn();

        if (uiManager)
            uiManager.PlayGoalInfo(result);

        if (!result.HasWinner)
        {
            turnController?.PrepareTurnAfterGoalDelay(ResetRoundAndStatus);
            return;
        }

        if (roundController)
            roundController.DespawnGameItems();

        HasActiveMatch = false;
    }

    private void HandleMatchConfigurationSelected(MatchConfiguration configuration)
    {
        currentConfiguration = configuration;
        hasCurrentConfiguration = true;
        StartConfiguredMatch(configuration);
    }

    private void HandleRestartClicked()
    {
        if (!hasCurrentConfiguration) return;
        StartConfiguredMatch(currentConfiguration);
    }

    private void HandleMainMenuClicked()
    {
        StopCurrentMatch();

        if (uiManager)
            uiManager.ShowStartGameState(this);
    }

    private void StartConfiguredMatch(MatchConfiguration configuration)
    {
        ResetCurrentMatchProgress();
        SpawnConfiguredMatch(configuration);

        HasActiveMatch = true;

        if (uiManager)
            uiManager.ShowMatchState(this);

        PrepareNextTurn();
    }

    private void ResetCurrentMatchProgress()
    {
        if (goalController)
            goalController.StartGoalLockoutPeriod();

        if (scoreKeeper)
            scoreKeeper.ResetScores();

        if (serveManager)
            serveManager.ResetMatch();

        if (turnController)
            turnController.EndTurn();
    }

    private bool SpawnConfiguredMatch(MatchConfiguration configuration)
    {
        if (!roundController) return false;

        roundController.DespawnGameItems();
        return roundController.SpawnGameItems(configuration);
    }

    private void StopCurrentMatch()
    {
        if (!HasActiveMatch) return;
        ResetCurrentMatchProgress();

        if (roundController)
            roundController.DespawnGameItems();

        HasActiveMatch = false;
    }

    private void SubscribeToGameFlow()
    {
        if (goalController)
            goalController.GoalResolved += HandleGoalResult;

        if (uiManager)
            uiManager.MatchConfigurationSelected += HandleMatchConfigurationSelected;

        if (turnController)
            turnController.RespawnItemsRequested += HandleRespawnItemsRequested;

        var inGameMenu = uiManager ? uiManager.InGameMenu : null;
        if (inGameMenu)
        {
            inGameMenu.RestartClicked += HandleRestartClicked;
            inGameMenu.MainMenuClicked += HandleMainMenuClicked;
        }
    }

    private void UnsubscribeFromGameFlow()
    {
        if (goalController)
            goalController.GoalResolved -= HandleGoalResult;

        if (uiManager)
            uiManager.MatchConfigurationSelected -= HandleMatchConfigurationSelected;

        if (turnController)
            turnController.RespawnItemsRequested -= HandleRespawnItemsRequested;

        var inGameMenu = uiManager ? uiManager.InGameMenu : null;
        if (inGameMenu)
        {
            inGameMenu.RestartClicked -= HandleRestartClicked;
            inGameMenu.MainMenuClicked -= HandleMainMenuClicked;
        }
    }

    private void HandleRespawnItemsRequested()
    {
        if (!HasActiveMatch) return;
        if (!hasCurrentConfiguration) return;

        var canStartTurn = SpawnConfiguredMatch(currentConfiguration);
        uiManager?.ClearGoalPopUpText();
        turnController?.ShowTurnPreparation(canStartTurn);
    }

    private void ValidateReferences()
    {
        if (!goalController)
            Debug.LogError($"{nameof(MatchManager)} requires a GoalController reference.", this);

        if (!roundController)
            Debug.LogError($"{nameof(MatchManager)} requires a RoundController reference.", this);

        if (!scoreKeeper)
            Debug.LogError($"{nameof(MatchManager)} requires a ScoreKeeper reference.", this);

        if (!serveManager)
            Debug.LogError($"{nameof(MatchManager)} requires a ServeManager reference.", this);

        if (!turnController)
            Debug.LogError($"{nameof(MatchManager)} requires a TurnController reference.", this);
    }
}
