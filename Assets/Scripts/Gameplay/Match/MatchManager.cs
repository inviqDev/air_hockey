using System;
using System.Collections.Generic;
using UnityEngine;

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

    public int LeftScore => scoreKeeper ? scoreKeeper.LeftScore : 0;
    public int RightScore => scoreKeeper ? scoreKeeper.RightScore : 0;
    public bool IsTurnActive => turnController && turnController.IsTurnActive;
    public bool IsRoundBreakActive => CurrentPhase == GamePhase.RoundBreak;
    public bool HasActiveMatch { get; private set; }
    public GamePhase CurrentPhase => matchFlow.CurrentPhase;
    public GameOverlay CurrentOverlay { get; private set; } = GameOverlay.None;
    public MatchResult CompletedMatchResult => completedMatchResult;

    public event Action<GamePhase, GamePhase> PhaseChanged;
    public event Action<GameOverlay, GameOverlay> OverlayChanged;
    public event Action<ParticipantId, bool> ParticipantReadyStatusChanged;

    private UIManager uiManager;

    private MatchConfiguration currentConfiguration;
    private ParticipantRoster currentParticipantRoster;
    private MatchResult completedMatchResult;
    private GameOverlay overlayToRestoreAfterSettings = GameOverlay.None;

    private Guid currentMatchSessionId = Guid.Empty;

    private bool hasCurrentConfiguration;
    private bool hasPreparedTurnState;
    private bool lastPreparedTurnCanStart;
    private bool isMatchFlowRoundStartPending;

    private readonly HashSet<ParticipantId> readyParticipants = new();
    private readonly MatchFlow matchFlow = new();

    private bool isInitialized;

    private void Awake()
    {
        matchFlow.PhaseChanged += HandleMatchFlowPhaseChanged;
        matchFlow.RoundStartRequested += HandleRoundStartRequested;
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
        if (CurrentPhase != GamePhase.RoundPreparation) return;
        if (isMatchFlowRoundStartPending) return;
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
            RefreshParticipantWorldBindings();
        }

        SubscribeToGameFlow();
        ApplyPlayerInputMode(PlayerInputMode.Disabled);
        isInitialized = true;
    }

    public bool IsParticipantReady(ParticipantId participantId)
    {
        return readyParticipants.Contains(participantId);
    }

    public bool TrySetParticipantReady(ParticipantId participantId, bool requestedReadyState)
    {
        if (!CanParticipantRequestReady(participantId)) return false;
        if (!TryGetParticipantReadyState(participantId, out var currentReadyState)) return false;
        if (currentReadyState == requestedReadyState) return false;

        SetParticipantReadyState(participantId, requestedReadyState);

        if (requestedReadyState)
            RequestRoundStart();
        else
            CancelRoundStartIfReadyStateChanged();

        return true;
    }

    public bool CanParticipantOpenAbilityMenu(ParticipantId participantId)
    {
        var canRequestReady = CanParticipantRequestReady(participantId);
        var hasNotReadyStatus = !IsParticipantReady(participantId);

        return canRequestReady && hasNotReadyStatus;
    }

    private bool CanParticipantRequestReady(ParticipantId participantId)
    {
        var isValid = IsValidParticipant(participantId);
        return matchFlow.CanParticipantRequestReady(HasActiveMatch, isValid, CurrentOverlay);
    }

    public bool ShouldShowParticipantReady(ParticipantId participantId)
    {
        var isValid = IsValidParticipant(participantId);
        return matchFlow.ShouldShowParticipantReady(HasActiveMatch, isValid, CurrentOverlay);
    }

    private void PrepareNextTurn()
    {
        if (!turnController) return;

        isMatchFlowRoundStartPending = false;
        TransitionPhase(GamePhase.RoundPreparation);
        hasPreparedTurnState = false;
        turnController.PrepareTurn(PrepareCurrentTurn);
    }

    private bool PrepareCurrentTurn()
    {
        var canStartTurn = roundController && roundController.ResetRoundItemsForTurn();
        lastPreparedTurnCanStart = canStartTurn;
        hasPreparedTurnState = true;

        if (uiManager)
            uiManager.ClearGoalPresentationText();

        return canStartTurn;
    }

    private void HandleGoalResult(GoalResult result)
    {
        if (turnController)
            turnController.EndTurn();

        if (!matchFlow.TryEnterGoalPresentation(HasActiveMatch)) return;

        var capturedMatchSessionId = currentMatchSessionId;

        if (uiManager)
        {
            uiManager.PlayGoalPresentation(result, () => CompleteGoalPresentationForMatchSession(result, capturedMatchSessionId));
            return;
        }

        CompleteGoalPresentationForMatchSession(result, capturedMatchSessionId);
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
        ClearCompletedMatchResult();
        SetNewMatchSessionId();
        ConfigureParticipants(configuration);
        ResetCurrentMatchProgress();
        SpawnConfiguredMatch(configuration);
        RefreshParticipantWorldBindings();
        ApplyPlayerInputMode(PlayerInputMode.Disabled);

        HasActiveMatch = true;
        overlayToRestoreAfterSettings = GameOverlay.None;

        if (uiManager)
            uiManager.ShowMatchState(this);

        PrepareNextTurn();
    }

    private void ResetCurrentMatchProgress()
    {
        ResetParticipantReadyState();
        SetOverlay(GameOverlay.None);
        ApplyPlayerInputMode(PlayerInputMode.Disabled);

        if (goalController)
            goalController.StartGoalLockoutPeriod();

        if (scoreKeeper)
        {
            if (!hasCurrentConfiguration)
                throw new InvalidOperationException($"{nameof(MatchManager)} cannot reset score without a current match configuration.");

            scoreKeeper.ConfigureForMatch(currentConfiguration);
            scoreKeeper.ResetScores();
        }

        if (serveManager)
            serveManager.ResetMatch();

        if (turnController)
            turnController.EndTurn();

        if (participantPreparationCoordinator)
            participantPreparationCoordinator.ResetProgression();

        hasPreparedTurnState = false;
        isMatchFlowRoundStartPending = false;
    }

    private bool SpawnConfiguredMatch(MatchConfiguration configuration)
    {
        if (!roundController) return false;
        if (currentParticipantRoster == null) return false;

        ApplyPlayerInputMode(PlayerInputMode.Disabled);
        ClearParticipantWorldBindings();
        roundController.ReturnRoundItemsToPoolForFullMatch();
        return roundController.ActivateRoundItems(configuration);
    }

    private void StopCurrentMatch()
    {
        if (!HasActiveMatch && CurrentPhase == GamePhase.NoActiveMatch) return;
        ClearCurrentMatchSessionId();
        ResetCurrentMatchProgress();

        ClearParticipantWorldBindings();

        if (roundController)
            roundController.ReturnRoundItemsToPoolForFullMatch();

        ClearParticipantCollection();

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
            turnController.CountdownCompleted += HandleTurnCountdownCompleted;
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
            turnController.CountdownCompleted -= HandleTurnCountdownCompleted;
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
        if (currentParticipantRoster == null) return;

        ApplyPlayerInputMode(PlayerInputMode.Disabled);
        ClearParticipantWorldBindings();
        var canStartTurn = roundController && roundController.RebuildRoundItemsForTurn(currentConfiguration);
        RefreshParticipantWorldBindings();
        ApplyResolvedPlayerInputMode();
        lastPreparedTurnCanStart = canStartTurn;
        hasPreparedTurnState = true;

        if (uiManager)
            uiManager.ClearGoalPresentationText();

        if (turnController)
            turnController.ShowTurnPreparation(canStartTurn);
    }

    private void EnterRoundBreak()
    {
        ResetParticipantReadyState();
        TransitionPhase(GamePhase.RoundBreak);
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
        matchFlow.TransitionToPhase(nextPhase);
    }

    private void HandleMatchFlowPhaseChanged(GamePhase previousPhase, GamePhase currentPhase)
    {
        if (previousPhase == GamePhase.RoundBreak && currentPhase != GamePhase.RoundBreak)
            ResetParticipantReadyState();

        ApplyResolvedPlayerInputMode();
        PhaseChanged?.Invoke(previousPhase, currentPhase);
    }

    private void HandleRoundStartRequested()
    {
        if (!turnController) return;

        isMatchFlowRoundStartPending = true;
        hasPreparedTurnState = false;

        if (uiManager)
            uiManager.ClearGoalPresentationText();

        turnController.ShowTurnPreparation(true);
    }

    private void HandleTurnCountdownCompleted()
    {
        if (!turnController) return;

        if (isMatchFlowRoundStartPending)
        {
            if (currentParticipantRoster == null) return;
            if (!matchFlow.TryCompleteRoundStartCountdown(
                    HasActiveMatch, CurrentOverlay, readyParticipants.Count, currentParticipantRoster.Count))
            {
                return;
            }

            var canStartTurn = PrepareCurrentTurn();
            if (!matchFlow.TryCompleteRoundPreparation(canStartTurn))
            {
                turnController.ShowTurnPreparation(canStartTurn);
                return;
            }

            isMatchFlowRoundStartPending = false;
            turnController.ActivatePreparedTurn();
            return;
        }

        TransitionPhase(GamePhase.RoundActive);
        turnController.ActivatePreparedTurn();
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
        if (participantPreparationCoordinator)
            participantPreparationCoordinator.ApplyInputMode(inputMode);
    }

    private void RefreshParticipantWorldBindings()
    {
        if (!participantPreparationCoordinator) return;

        if (!roundController || currentParticipantRoster == null)
        {
            ClearParticipantWorldBindings();
            return;
        }

        foreach (var participant in currentParticipantRoster.Participants)
        {
            var participantId = participant.ParticipantId;
            var participantSlotId = currentConfiguration.SlotAssignments.GetSlotForParticipant(participantId);
            var abilityController = roundController.GetAbilityController(participantSlotId);
            participantPreparationCoordinator.BindParticipantAbilityController(participantId, abilityController);

            if (participant.IsHuman)
            {
                if (!roundController.TryGetHumanGameplayTargets(
                        participantSlotId,
                        out var movement,
                        out var gameplayAbilityController))
                {
                    throw new InvalidOperationException(
                        $"{nameof(MatchManager)} could not resolve gameplay targets for human participant " +
                        $"{participantId} assigned to arena slot {participantSlotId}.");
                }

                participantPreparationCoordinator.BindParticipantGameplayInputTargets(
                    participantId, movement, gameplayAbilityController);
                continue;
            }

            participantPreparationCoordinator.ClearParticipantGameplayInputTargets(participantId);
        }
    }

    private void ClearParticipantWorldBindings()
    {
        if (!participantPreparationCoordinator) return;

        participantPreparationCoordinator.ClearParticipantGameplayInputTargets();
        participantPreparationCoordinator.ClearParticipantAbilityControllers();
    }

    private PlayerInputMode ResolvePlayerInputMode()
    {
        if (CurrentOverlay != GameOverlay.None)
            return PlayerInputMode.Disabled;

        return CurrentPhase switch
        {
            GamePhase.RoundActive => PlayerInputMode.Gameplay,
            GamePhase.RoundBreak => PlayerInputMode.Preparation,
            GamePhase.NoActiveMatch => PlayerInputMode.Disabled,
            GamePhase.RoundPreparation => PlayerInputMode.Disabled,
            GamePhase.GoalPresentation => PlayerInputMode.Disabled,
            GamePhase.MatchComplete => PlayerInputMode.Disabled,
            _ => PlayerInputMode.Disabled
        };
    }

    private void CompleteGoalPresentationForMatchSession(GoalResult result, Guid capturedMatchSessionId)
    {
        if (capturedMatchSessionId != currentMatchSessionId) return;
        if (!HasActiveMatch) return;
        if (CurrentPhase != GamePhase.GoalPresentation) return;

        if (!matchFlow.TryCompleteGoalPresentation(result.HasWinner)) return;
        if (!result.HasWinner)
        {
            ResetParticipantReadyState();
            return;
        }

        CaptureCompletedMatchResult(result);

        ApplyPlayerInputMode(PlayerInputMode.Disabled);
        ClearParticipantWorldBindings();

        if (roundController)
            roundController.ReturnRoundItemsToPoolForFullMatch();

        ResetParticipantReadyState();
        HasActiveMatch = false;
        ClearParticipantCollection();
    }

    private void SetNewMatchSessionId()
    {
        currentMatchSessionId = Guid.NewGuid();
    }

    private void ClearCurrentMatchSessionId()
    {
        currentMatchSessionId = Guid.Empty;
    }

    private void CaptureCompletedMatchResult(GoalResult result)
    {
        if (!scoreKeeper)
            throw new InvalidOperationException($"{nameof(MatchManager)} cannot create match result without a ScoreKeeper reference.");

        completedMatchResult = scoreKeeper.CreateMatchResult(result.ScoringParticipantId);
    }

    private void ClearCompletedMatchResult()
    {
        completedMatchResult = null;
    }

    private bool IsValidParticipant(ParticipantId participantId)
    {
        return currentParticipantRoster != null && currentParticipantRoster.ContainsParticipant(participantId);
    }

    private void RequestRoundStart()
    {
        if (currentParticipantRoster == null) return;
        matchFlow.RequestRoundStartIfReady(HasActiveMatch, CurrentOverlay, readyParticipants.Count, currentParticipantRoster.Count);
    }

    private void CancelRoundStartIfReadyStateChanged()
    {
        if (!isMatchFlowRoundStartPending) return;
        if (currentParticipantRoster == null) return;
        if (!matchFlow.ShouldCancelRoundStart(HasActiveMatch, readyParticipants.Count, currentParticipantRoster.Count)) return;

        isMatchFlowRoundStartPending = false;

        if (turnController)
            turnController.EndTurn();
    }

    private bool TryGetParticipantReadyState(ParticipantId participantId, out bool currentReadyState)
    {
        if (!IsValidParticipant(participantId))
        {
            currentReadyState = false;
            return false;
        }

        currentReadyState = readyParticipants.Contains(participantId);
        return true;
    }

    private void ResetParticipantReadyState()
    {
        if (readyParticipants.Count == 0) return;

        var previousReadyParticipants = new ParticipantId[readyParticipants.Count];
        readyParticipants.CopyTo(previousReadyParticipants);
        readyParticipants.Clear();

        foreach (var participantId in previousReadyParticipants)
        {
            ParticipantReadyStatusChanged?.Invoke(participantId, false);
        }
    }

    private void SetParticipantReadyState(ParticipantId participantId, bool requestedReadyState)
    {
        var didChange = requestedReadyState
            ? readyParticipants.Add(participantId)
            : readyParticipants.Remove(participantId);

        if (!didChange) return;

        ParticipantReadyStatusChanged?.Invoke(participantId, requestedReadyState);
    }

    private void ConfigureParticipants(MatchConfiguration configuration)
    {
        currentParticipantRoster = configuration.Roster;

        if (participantPreparationCoordinator)
            participantPreparationCoordinator.ConfigureParticipants(configuration);
    }

    private void ClearParticipantCollection()
    {
        if (participantPreparationCoordinator)
            participantPreparationCoordinator.ClearParticipants();

        currentParticipantRoster = null;
    }
}
