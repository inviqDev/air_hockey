public sealed class ParticipantReadyStatusHandler
{
    private readonly PlayerSide side;
    private readonly ParticipantHudView participantHud;
    private readonly ParticipantAbilitySelectionRuntime abilitySelectionRuntime;
    private readonly MatchManager matchManager;

    private PlayerInputReader inputReader;
    private bool isEnabled;

    public ParticipantReadyStatusHandler(
        PlayerSide side,
        ParticipantHudView participantHud,
        ParticipantAbilitySelectionRuntime abilitySelectionRuntime,
        MatchManager matchManager)
    {
        this.side = side;
        this.participantHud = participantHud;
        this.abilitySelectionRuntime = abilitySelectionRuntime;
        this.matchManager = matchManager;
    }

    public void Enable()
    {
        if (isEnabled) return;

        isEnabled = true;
        abilitySelectionRuntime.MenuOpenStateChanged += HandleMenuOpenStateChanged;
        SubscribeToInputReader();
        SubscribeToMatchManager();
        RefreshPresentation();
    }

    public void Disable()
    {
        if (!isEnabled) return;

        UnsubscribeFromMatchManager();
        UnsubscribeFromInputReader();
        abilitySelectionRuntime.MenuOpenStateChanged -= HandleMenuOpenStateChanged;
        isEnabled = false;
        HidePresentation();
    }

    public void BindInputReader(PlayerInputReader nextInputReader)
    {
        if (inputReader == nextInputReader) return;

        UnsubscribeFromInputReader();
        inputReader = nextInputReader;
        SubscribeToInputReader();
        RefreshPresentation();
    }

    private void SubscribeToInputReader()
    {
        if (!isEnabled || !inputReader) return;
        inputReader.ReadyToggleRequested += HandleReadyToggleRequested;
    }

    private void UnsubscribeFromInputReader()
    {
        if (!inputReader) return;
        inputReader.ReadyToggleRequested -= HandleReadyToggleRequested;
    }

    private void SubscribeToMatchManager()
    {
        if (!isEnabled) return;

        matchManager.ParticipantReadyStatusChanged += HandleParticipantReadyStatusChanged;
        matchManager.PhaseChanged += HandlePhaseChanged;
        matchManager.OverlayChanged += HandleOverlayChanged;
    }

    private void UnsubscribeFromMatchManager()
    {
        matchManager.ParticipantReadyStatusChanged -= HandleParticipantReadyStatusChanged;
        matchManager.PhaseChanged -= HandlePhaseChanged;
        matchManager.OverlayChanged -= HandleOverlayChanged;
    }

    private void HandleReadyToggleRequested()
    {
        if (abilitySelectionRuntime.IsMenuOpen) return;

        var isCurrentlyReady = matchManager.IsParticipantReady(side);
        matchManager.TrySetParticipantReady(side, !isCurrentlyReady);
    }

    private void HandleParticipantReadyStatusChanged(PlayerSide participantSide, bool isReady)
    {
        if (participantSide != side) return;
        RefreshPresentation();
    }

    private void HandlePhaseChanged(GamePhase previousPhase, GamePhase currentPhase)
    {
        RefreshPresentation();
    }

    private void HandleOverlayChanged(GameOverlay previousOverlay, GameOverlay currentOverlay)
    {
        RefreshPresentation();
    }

    private void HandleMenuOpenStateChanged(bool isMenuOpen)
    {
        RefreshPresentation();
    }

    private void RefreshPresentation()
    {
        if (!participantHud) return;

        if (!ShouldShowReadyView())
        {
            HidePresentation();
            return;
        }

        participantHud.SetReady(matchManager.IsParticipantReady(side));
        participantHud.SetReadyVisible(true);
    }

    private void HidePresentation()
    {
        if (!participantHud) return;
        participantHud.SetReadyVisible(false);
    }

    private bool ShouldShowReadyView()
    {
        return isEnabled &&
               matchManager.HasActiveMatch &&
               matchManager.CurrentPhase == GamePhase.RoundBreak &&
               matchManager.CurrentOverlay != GameOverlay.Settings &&
               !abilitySelectionRuntime.IsMenuOpen;
    }
}
