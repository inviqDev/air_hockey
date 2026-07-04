public sealed class ParticipantReadyStatusHandler
{
    private readonly ParticipantId participantId;
    private readonly ParticipantHudView participantHud;
    private readonly ParticipantAbilitySelectionRuntime abilitySelectionRuntime;
    private readonly MatchManager matchManager;

    private PlayerInputReader inputReader;
    private bool isEnabled;

    public ParticipantReadyStatusHandler(
        ParticipantId participantId,
        ParticipantHudView participantHud,
        ParticipantAbilitySelectionRuntime abilitySelectionRuntime,
        MatchManager matchManager)
    {
        this.participantId = participantId;
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

        var isCurrentlyReady = matchManager.IsParticipantReady(participantId);
        matchManager.TrySetParticipantReady(participantId, !isCurrentlyReady);
    }

    private void HandleParticipantReadyStatusChanged(ParticipantId changedParticipantId, bool isReady)
    {
        if (changedParticipantId != participantId) return;
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

        participantHud.SetReady(matchManager.IsParticipantReady(participantId));
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
               matchManager.ShouldShowParticipantReady(participantId) &&
               !abilitySelectionRuntime.IsMenuOpen;
    }
}
