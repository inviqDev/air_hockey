using System;
using System.Collections.Generic;
using UnityEngine;

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
    public bool HasActiveMatch { get; private set; }
    public GamePhase CurrentPhase { get; private set; } = GamePhase.NoActiveMatch;
    public GameOverlay CurrentOverlay { get; private set; } = GameOverlay.None;

    public event Action<GamePhase, GamePhase> PhaseChanged;
    public event Action<GameOverlay, GameOverlay> OverlayChanged;
    public event Action<ParticipantId, bool> ParticipantReadyStatusChanged;

    private UIManager uiManager;

    private MatchConfiguration currentConfiguration;
    private ParticipantRoster currentParticipants;
    private GameOverlay overlayToRestoreAfterSettings = GameOverlay.None;

    private bool hasCurrentConfiguration;
    private bool hasPreparedTurnState;
    private bool lastPreparedTurnCanStart;

    private readonly HashSet<ParticipantId> readyParticipants = new();

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
        TryPrepareNextTurnWhenAllParticipantsReady();
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
        return IsValidParticipant(participantId) &&
               HasActiveMatch &&
               CurrentPhase == GamePhase.RoundBreak &&
               CurrentOverlay == GameOverlay.None;
    }

    public bool ShouldShowParticipantReady(ParticipantId participantId)
    {
        return IsValidParticipant(participantId) &&
               HasActiveMatch &&
               CurrentPhase == GamePhase.RoundBreak &&
               CurrentOverlay != GameOverlay.Settings;
    }

    private void PrepareNextTurn()
    {
        if (!turnController) return;

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
            roundController.ReturnRoundItemsToPoolForFullMatch();

        RefreshAbilitySelectionBindings();
        ResetParticipantReadyState();
        TransitionPhase(GamePhase.MatchComplete);
        HasActiveMatch = false;
        ClearParticipantCollection();
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
        ConfigureParticipants(configuration);
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
        if (currentParticipants == null) return false;

        roundController.ReturnRoundItemsToPoolForFullMatch();
        return roundController.ActivateRoundItems(configuration);
    }

    private void StopCurrentMatch()
    {
        if (!HasActiveMatch && CurrentPhase == GamePhase.NoActiveMatch) return;
        ResetCurrentMatchProgress();

        if (roundController)
            roundController.ReturnRoundItemsToPoolForFullMatch();

        RefreshAbilitySelectionBindings();
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
        if (currentParticipants == null) return;

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

        if (!roundController || currentParticipants == null)
        {
            participantPreparationCoordinator.ClearParticipantAbilityControllers();
            return;
        }

        foreach (var participant in currentParticipants.Participants)
        {
            var participantId = participant.ParticipantId;
            var participantSlotId = currentConfiguration.SlotAssignments.GetSlotForParticipant(participantId);
            var abilityController = roundController.GetAbilityController(participantSlotId);
            participantPreparationCoordinator.BindParticipantAbilityController(participantId, abilityController);
        }
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

    private bool IsValidParticipant(ParticipantId participantId)
    {
        return currentParticipants != null && currentParticipants.ContainsParticipant(participantId);
    }

    private void TryPrepareNextTurnWhenAllParticipantsReady()
    {
        if (!HasActiveMatch) return;
        if (currentParticipants == null) return;

        if (CurrentPhase != GamePhase.RoundBreak) return;
        if (CurrentOverlay != GameOverlay.None) return;

        var allParticipantsReady = readyParticipants.Count == currentParticipants.Count;
        if (!allParticipantsReady) return;

        PrepareNextTurn();
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
        currentParticipants = configuration.Roster;

        if (participantPreparationCoordinator)
            participantPreparationCoordinator.ConfigureParticipants(configuration);
    }

    private void ClearParticipantCollection()
    {
        if (participantPreparationCoordinator)
            participantPreparationCoordinator.ClearParticipants();

        currentParticipants = null;
    }
}
