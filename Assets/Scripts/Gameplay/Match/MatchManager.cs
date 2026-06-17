using UnityEngine;
using System.Collections;

public sealed class MatchManager : MonoBehaviour
{
    private enum MatchFlowState
    {
        Inactive,
        TurnPreparation,
        TurnActive,
        RoundBreak,
        MatchComplete
    }

    [Header("References")]
    [SerializeField] private RoundController roundController;
    [SerializeField] private TurnController turnController;
    [SerializeField] private ServeManager serveManager;
    [SerializeField] private GoalController goalController;
    [SerializeField] private ScoreKeeper scoreKeeper;
    [SerializeField] private AbilitySelectionCoordinator abilitySelectionCoordinator;

    public int LeftScore => goalController ? goalController.LeftScore : 0;
    public int RightScore => goalController ? goalController.RightScore : 0;
    public bool IsTurnActive => turnController && turnController.IsTurnActive;
    public bool HasActiveMatch { get; private set; }

    private UIManager uiManager;

    private MatchConfiguration currentConfiguration;
    private bool hasCurrentConfiguration;
    private bool hasPreparedTurnState;
    private bool lastPreparedTurnCanStart;
    private MatchFlowState matchFlowState = MatchFlowState.Inactive;
    private Coroutine roundBreakDelayRoutine;

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
        if (matchFlowState != MatchFlowState.TurnPreparation) return;
        if (turnController.IsTurnActive) return;

        var canStartTurn = roundController && roundController.HasAllRoundItemsActive;
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

        StopRoundBreakDelayRoutine();
        matchFlowState = MatchFlowState.TurnPreparation;
        hasPreparedTurnState = false;
        turnController.PrepareTurn(PrepareCurrentTurn);
    }

    private bool PrepareCurrentTurn()
    {
        var canStartTurn = roundController && roundController.ResetRoundItemsForTurn();
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
            EnterRoundBreak();
            return;
        }

        if (roundController)
            roundController.ReturnRoundItemsToPool();

        StopRoundBreakDelayRoutine();
        matchFlowState = MatchFlowState.MatchComplete;
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
        matchFlowState = MatchFlowState.Inactive;

        if (uiManager)
            uiManager.ShowMatchState(this);

        PrepareNextTurn();
    }

    private void ResetCurrentMatchProgress()
    {
        StopRoundBreakDelayRoutine();

        if (goalController)
            goalController.StartGoalLockoutPeriod();

        if (scoreKeeper)
            scoreKeeper.ResetScores();

        if (serveManager)
            serveManager.ResetMatch();

        if (turnController)
            turnController.EndTurn();

        if (abilitySelectionCoordinator)
            abilitySelectionCoordinator.ResetProgression();

        hasPreparedTurnState = false;
        matchFlowState = MatchFlowState.Inactive;
    }

    private bool SpawnConfiguredMatch(MatchConfiguration configuration)
    {
        if (!roundController) return false;

        roundController.ReturnRoundItemsToPool();
        return roundController.ActivateRoundItems(configuration);
    }

    private void StopCurrentMatch()
    {
        if (!HasActiveMatch) return;
        ResetCurrentMatchProgress();

        if (roundController)
            roundController.ReturnRoundItemsToPool();

        HasActiveMatch = false;
        hasPreparedTurnState = false;
        matchFlowState = MatchFlowState.Inactive;
    }

    private void SubscribeToGameFlow()
    {
        if (goalController)
            goalController.GoalResolved += HandleGoalResult;

        if (uiManager)
            uiManager.MatchConfigurationSelected += HandleMatchConfigurationSelected;

        if (turnController)
        {
            turnController.TurnStarted += HandleTurnStarted;
            turnController.RespawnItemsRequested += HandleRespawnItemsRequested;
        }

        var inGameMenu = uiManager ? uiManager.InGameMenu : null;

        if (inGameMenu)
        {
            inGameMenu.RestartClicked += HandleRestartClicked;
            inGameMenu.MainMenuClicked += HandleMainMenuClicked;
            inGameMenu.PauseStateChanged += HandlePauseStateChanged;
            HandlePauseStateChanged(inGameMenu.IsPaused);
        }
    }

    private void UnsubscribeFromGameFlow()
    {
        if (goalController)
            goalController.GoalResolved -= HandleGoalResult;

        if (uiManager)
            uiManager.MatchConfigurationSelected -= HandleMatchConfigurationSelected;

        if (turnController)
        {
            turnController.TurnStarted -= HandleTurnStarted;
            turnController.RespawnItemsRequested -= HandleRespawnItemsRequested;
        }

        var inGameMenu = uiManager ? uiManager.InGameMenu : null;
        if (inGameMenu)
        {
            inGameMenu.RestartClicked -= HandleRestartClicked;
            inGameMenu.MainMenuClicked -= HandleMainMenuClicked;
            inGameMenu.PauseStateChanged -= HandlePauseStateChanged;
        }
    }

    private void HandlePauseStateChanged(bool isPaused)
    {
        if (roundController)
            roundController.SetAbilityPauseState(isPaused);
    }

    private void HandleRespawnItemsRequested()
    {
        if (!HasActiveMatch) return;
        if (!hasCurrentConfiguration) return;

        var canStartTurn = roundController && roundController.RebuildRoundItemsForTurn(currentConfiguration);
        lastPreparedTurnCanStart = canStartTurn;
        hasPreparedTurnState = true;

        if (uiManager)
            uiManager.ClearGoalPopUpText();

        if (turnController)
            turnController.ShowTurnPreparation(canStartTurn);
    }

    private void HandleTurnStarted()
    {
        matchFlowState = MatchFlowState.TurnActive;
    }

    private void EnterRoundBreak()
    {
        StopRoundBreakDelayRoutine();
        matchFlowState = MatchFlowState.RoundBreak;
        roundBreakDelayRoutine = StartCoroutine(ReleaseRoundBreakAfterDelayRoutine());
    }

    private IEnumerator ReleaseRoundBreakAfterDelayRoutine()
    {
        var delaySeconds = turnController ? turnController.GoalDelayBeforeNextTurnSeconds : 0f;

        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        roundBreakDelayRoutine = null;

        if (!HasActiveMatch) yield break;
        if (matchFlowState != MatchFlowState.RoundBreak) yield break;

        PrepareNextTurn();
    }

    private void StopRoundBreakDelayRoutine()
    {
        if (roundBreakDelayRoutine == null) return;

        StopCoroutine(roundBreakDelayRoutine);
        roundBreakDelayRoutine = null;
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

        if (!abilitySelectionCoordinator)
            Debug.LogError($"{nameof(MatchManager)} requires an {nameof(AbilitySelectionCoordinator)} reference.", this);
    }
}
