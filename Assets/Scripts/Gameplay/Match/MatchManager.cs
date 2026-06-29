using System;
using UnityEngine;
using System.Collections;

public enum GamePhase
{
    NoActiveMatch,
    TurnPreparation,
    TurnActive,
    RoundBreak,
    MatchComplete
}

public enum GameOverlay
{
    None,
    Pause,
    Settings
}

public sealed class MatchManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoundController roundController;
    [SerializeField] private TurnController turnController;
    [SerializeField] private ServeManager serveManager;
    [SerializeField] private GoalController goalController;
    [SerializeField] private ScoreKeeper scoreKeeper;
    [SerializeField] private ParticipantPreparationCoordinator participantPreparationCoordinator;

    public int LeftScore => goalController ? goalController.LeftScore : 0;
    public int RightScore => goalController ? goalController.RightScore : 0;
    public bool IsTurnActive => turnController && turnController.IsTurnActive;
    public bool IsRoundBreakActive => CurrentPhase == GamePhase.RoundBreak;
    public bool IsAbilityMenuInteractionAllowed => HasActiveMatch && CurrentPhase == GamePhase.RoundBreak && CurrentOverlay == GameOverlay.None;
    public bool AreBothParticipantsReady => leftParticipantReady && rightParticipantReady;
    public bool HasActiveMatch { get; private set; }
    public GamePhase CurrentPhase { get; private set; } = GamePhase.NoActiveMatch;
    public GameOverlay CurrentOverlay { get; private set; } = GameOverlay.None;

    public event Action<GamePhase, GamePhase> PhaseChanged;
    public event Action<GameOverlay, GameOverlay> OverlayChanged;
    public event Action<PlayerSide, bool> ParticipantReadyStatusChanged;

    private UIManager uiManager;

    private MatchConfiguration currentConfiguration;
    private GameOverlay overlayToRestoreAfterSettings = GameOverlay.None;
    
    private bool hasCurrentConfiguration;
    private bool hasPreparedTurnState;
    private bool lastPreparedTurnCanStart;
    
    private bool leftParticipantReady;
    private bool rightParticipantReady;

    private Coroutine roundBreakDelayRoutine;
    
    private bool isInitialized;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnEnable()
    {
        if (!isInitialized) return;
        if (participantPreparationCoordinator)
            participantPreparationCoordinator.Activate();

        SubscribeToGameFlow();
        ApplyResolvedPlayerInputMode();
    }

    private void OnDisable()
    {
        if (!isInitialized) return;
        UnsubscribeFromGameFlow();

        if (participantPreparationCoordinator)
            participantPreparationCoordinator.Deactivate();
    }

    private void Update()
    {
        if (!HasActiveMatch) return;
        if (!turnController) return;
        if (CurrentPhase != GamePhase.TurnPreparation) return;
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

        if (participantPreparationCoordinator)
        {
            participantPreparationCoordinator.Initialize(this);
            participantPreparationCoordinator.Activate();
            RefreshAbilitySelectionBindings();
        }

        SubscribeToGameFlow();
        ApplyPlayerInputMode(PlayerInputMode.Disabled);
        isInitialized = true;
    }
    
    public bool IsParticipantReady(PlayerSide side)
    {
        return side switch
        {
            PlayerSide.Left => leftParticipantReady,
            PlayerSide.Right => rightParticipantReady,
            _ => false
        };
    }

    public bool TrySetParticipantReady(PlayerSide side, bool requestedReadyState)
    {
        if (!CanChangeParticipantReadyState()) return false;
        if (!TryGetParticipantReadyState(side, out var currentReadyState)) return false;
        if (currentReadyState == requestedReadyState) return false;

        SetParticipantReadyState(side, requestedReadyState);
        return true;
    }

    private void PrepareNextTurn()
    {
        if (!turnController) return;

        StopRoundBreakDelayRoutine();
        TransitionPhase(GamePhase.TurnPreparation);
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

        RefreshAbilitySelectionBindings();
        StopRoundBreakDelayRoutine();
        ResetParticipantReadyState();
        TransitionPhase(GamePhase.MatchComplete);
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
        RefreshAbilitySelectionBindings();
        ApplyPlayerInputMode(PlayerInputMode.Disabled);

        HasActiveMatch = true;
        overlayToRestoreAfterSettings = GameOverlay.None;

        if (uiManager)
            uiManager.ShowMatchState(this);

        PrepareNextTurn();
    }

    private void ResetCurrentMatchProgress()
    {
        StopRoundBreakDelayRoutine();
        ResetParticipantReadyState();
        SetOverlay(GameOverlay.None);
        ApplyPlayerInputMode(PlayerInputMode.Disabled);

        if (goalController)
            goalController.StartGoalLockoutPeriod();

        if (scoreKeeper)
            scoreKeeper.ResetScores();

        if (serveManager)
            serveManager.ResetMatch();

        if (turnController)
            turnController.EndTurn();

        if (participantPreparationCoordinator)
            participantPreparationCoordinator.ResetProgression();

        hasPreparedTurnState = false;
    }

    private bool SpawnConfiguredMatch(MatchConfiguration configuration)
    {
        if (!roundController) return false;

        roundController.ReturnRoundItemsToPool();
        return roundController.ActivateRoundItems(configuration);
    }

    private void StopCurrentMatch()
    {
        if (!HasActiveMatch && CurrentPhase == GamePhase.NoActiveMatch) return;
        ResetCurrentMatchProgress();

        if (roundController)
            roundController.ReturnRoundItemsToPool();

        RefreshAbilitySelectionBindings();

        HasActiveMatch = false;
        hasPreparedTurnState = false;
        TransitionPhase(GamePhase.NoActiveMatch);
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
            inGameMenu.PauseToggleRequested += HandlePauseToggleRequested;
            inGameMenu.SettingsOpenRequested += HandleSettingsOpenRequested;
            inGameMenu.SettingsCloseRequested += HandleSettingsCloseRequested;
            ApplyOverlayEffects();
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
            inGameMenu.PauseToggleRequested -= HandlePauseToggleRequested;
            inGameMenu.SettingsOpenRequested -= HandleSettingsOpenRequested;
            inGameMenu.SettingsCloseRequested -= HandleSettingsCloseRequested;
        }
    }

    private void HandlePauseToggleRequested()
    {
        if (CurrentOverlay == GameOverlay.Settings) return;

        var nextOverlay = CurrentOverlay == GameOverlay.Pause
            ? GameOverlay.None
            : GameOverlay.Pause;

        SetOverlay(nextOverlay);
    }

    private void HandleSettingsOpenRequested()
    {
        if (CurrentOverlay == GameOverlay.Settings) return;

        overlayToRestoreAfterSettings = CurrentOverlay == GameOverlay.Pause
            ? GameOverlay.Pause
            : GameOverlay.None;

        SetOverlay(GameOverlay.Settings);
    }

    private void HandleSettingsCloseRequested()
    {
        if (CurrentOverlay != GameOverlay.Settings) return;

        SetOverlay(overlayToRestoreAfterSettings);
    }

    private void HandleRespawnItemsRequested()
    {
        if (!HasActiveMatch) return;
        if (!hasCurrentConfiguration) return;

        var canStartTurn = roundController && roundController.RebuildRoundItemsForTurn(currentConfiguration);
        RefreshAbilitySelectionBindings();
        ApplyResolvedPlayerInputMode();
        lastPreparedTurnCanStart = canStartTurn;
        hasPreparedTurnState = true;

        if (uiManager)
            uiManager.ClearGoalPopUpText();

        if (turnController)
            turnController.ShowTurnPreparation(canStartTurn);
    }

    private void HandleTurnStarted()
    {
        TransitionPhase(GamePhase.TurnActive);
    }

    private void EnterRoundBreak()
    {
        StopRoundBreakDelayRoutine();
        ResetParticipantReadyState();
        TransitionPhase(GamePhase.RoundBreak);
        roundBreakDelayRoutine = StartCoroutine(ReleaseRoundBreakAfterDelayRoutine());
    }

    private IEnumerator ReleaseRoundBreakAfterDelayRoutine()
    {
        var delaySeconds = turnController ? turnController.GoalDelayBeforeNextTurnSeconds : 0f;

        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        roundBreakDelayRoutine = null;

        if (!HasActiveMatch) yield break;
        if (CurrentPhase != GamePhase.RoundBreak) yield break;

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

        if (!participantPreparationCoordinator)
            Debug.LogError($"{nameof(MatchManager)} requires a {nameof(ParticipantPreparationCoordinator)} reference.", this);
    }

    private void SetOverlay(GameOverlay nextOverlay)
    {
        if (CurrentOverlay == nextOverlay)
        {
            ApplyOverlayEffects();
            ApplyResolvedPlayerInputMode();
            return;
        }

        var previousOverlay = CurrentOverlay;
        CurrentOverlay = nextOverlay;
        ApplyOverlayEffects();
        ApplyResolvedPlayerInputMode();
        OverlayChanged?.Invoke(previousOverlay, CurrentOverlay);
    }

    private void TransitionPhase(GamePhase nextPhase)
    {
        if (CurrentPhase == nextPhase) return;

        var previousPhase = CurrentPhase;
        if (previousPhase == GamePhase.RoundBreak && nextPhase != GamePhase.RoundBreak)
            ResetParticipantReadyState();

        CurrentPhase = nextPhase;
        ApplyResolvedPlayerInputMode();
        PhaseChanged?.Invoke(previousPhase, CurrentPhase);
    }

    private void ApplyOverlayEffects()
    {
        var inGameMenu = uiManager ? uiManager.InGameMenu : null;
        if (inGameMenu)
            inGameMenu.ApplyOverlayState(CurrentOverlay);

        if (roundController)
            roundController.SetAbilityPauseState(CurrentOverlay != GameOverlay.None);
    }

    private void ApplyResolvedPlayerInputMode()
    {
        ApplyPlayerInputMode(ResolvePlayerInputMode());
    }

    private void ApplyPlayerInputMode(PlayerInputMode inputMode)
    {
        if (!roundController) return;
        roundController.ApplyPlayerInputMode(inputMode);
    }

    private void RefreshAbilitySelectionBindings()
    {
        if (!participantPreparationCoordinator) return;

        if (!roundController)
        {
            participantPreparationCoordinator.ClearParticipantAbilityControllers();
            return;
        }

        participantPreparationCoordinator.BindParticipantAbilityController(PlayerSide.Left, roundController.GetAbilityController(PlayerSide.Left));
        participantPreparationCoordinator.BindParticipantAbilityController(PlayerSide.Right, roundController.GetAbilityController(PlayerSide.Right));
    }

    private PlayerInputMode ResolvePlayerInputMode()
    {
        if (CurrentOverlay != GameOverlay.None)
            return PlayerInputMode.Disabled;

        return CurrentPhase switch
        {
            GamePhase.TurnActive => PlayerInputMode.Gameplay,
            GamePhase.RoundBreak => PlayerInputMode.Intermission,
            GamePhase.NoActiveMatch => PlayerInputMode.Disabled,
            GamePhase.TurnPreparation => PlayerInputMode.Disabled,
            GamePhase.MatchComplete => PlayerInputMode.Disabled,
            _ => PlayerInputMode.Disabled
        };
    }

    private bool CanChangeParticipantReadyState()
    {
        return HasActiveMatch &&
               CurrentPhase == GamePhase.RoundBreak &&
               CurrentOverlay == GameOverlay.None;
    }

    private bool TryGetParticipantReadyState(PlayerSide side, out bool currentReadyState)
    {
        switch (side)
        {
            case PlayerSide.Left:
                currentReadyState = leftParticipantReady;
                return true;

            case PlayerSide.Right:
                currentReadyState = rightParticipantReady;
                return true;

            default:
                Debug.LogError($"{nameof(MatchManager)} received " +
                               $"unsupported {nameof(PlayerSide)} value: {side}.", this);
                currentReadyState = false;
                return false;
        }
    }

    private void ResetParticipantReadyState()
    {
        SetParticipantReadyState(PlayerSide.Left, false);
        SetParticipantReadyState(PlayerSide.Right, false);
    }

    private void SetParticipantReadyState(PlayerSide side, bool requestedReadyState)
    {
        switch (side)
        {
            case PlayerSide.Left:
                if (leftParticipantReady == requestedReadyState) return;
                leftParticipantReady = requestedReadyState;
                break;

            case PlayerSide.Right:
                if (rightParticipantReady == requestedReadyState) return;
                rightParticipantReady = requestedReadyState;
                break;

            default:
                return;
        }

        ParticipantReadyStatusChanged?.Invoke(side, requestedReadyState);
    }
}
