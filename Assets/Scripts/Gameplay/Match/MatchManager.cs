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
    private bool hasPreparedTurnState;
    private bool lastPreparedTurnCanStart;

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

    private void Update()
    {
        if (!HasActiveMatch) return;
        if (!turnController) return;
        if (turnController.IsTurnActive) return;

        var canStartTurn = roundController && roundController.HasAllGameItemsSpawned;
        if (hasPreparedTurnState && lastPreparedTurnCanStart == canStartTurn) return;

        lastPreparedTurnCanStart = canStartTurn;
        hasPreparedTurnState = true;
        turnController.RefreshTurnPreparation(PrepareCurrentTurn);
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
        hasPreparedTurnState = false;
        turnController.PrepareTurn(PrepareCurrentTurn);
    }

    private bool PrepareCurrentTurn()
    {
        var canStartTurn = roundController && roundController.PrepareTurn();
        lastPreparedTurnCanStart = canStartTurn;
        hasPreparedTurnState = true;
        
        if (uiManager)
            uiManager.ClearGoalPopUpText();

        return canStartTurn;
    }

    private void HandleGoalResult(GoalResult result)
    {
        if (turnController)
            turnController.EndTurn();

        if (uiManager)
            uiManager.PlayGoalInfo(result);

        if (!result.HasWinner)
        {
            turnController?.PrepareTurnAfterGoalDelay(PrepareCurrentTurn);
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
            uiManager.ShowStartGameStateImmediately(this);
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

        hasPreparedTurnState = false;
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
        hasPreparedTurnState = false;
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

        var canStartTurn = roundController && roundController.RespawnTurnItems(currentConfiguration);
        lastPreparedTurnCanStart = canStartTurn;
        hasPreparedTurnState = true;
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
