using System;

public sealed class MatchFlow
{
    public GamePhase CurrentPhase { get; private set; } = GamePhase.NoActiveMatch;

    public event Action<GamePhase, GamePhase> PhaseChanged;
    public event Action RoundStartRequested;

    public void TransitionToPhase(GamePhase nextPhase)
    {
        if (CurrentPhase == nextPhase) return;

        var previousPhase = CurrentPhase;
        CurrentPhase = nextPhase;
        PhaseChanged?.Invoke(previousPhase, CurrentPhase);
    }

    public bool CanParticipantRequestReady(bool hasActiveMatch, bool isValidParticipant, GameOverlay currentOverlay)
    {
        var roundBreakActive = IsParticipantInRoundBreak(hasActiveMatch, isValidParticipant);
        return roundBreakActive && currentOverlay == GameOverlay.None;
    }

    public bool ShouldShowParticipantReady(bool hasActiveMatch, bool isValidParticipant, GameOverlay currentOverlay)
    {
        var roundBreakActive = IsParticipantInRoundBreak(hasActiveMatch, isValidParticipant);
        return roundBreakActive && currentOverlay != GameOverlay.Settings;
    }

    private bool IsParticipantInRoundBreak(bool hasActiveMatch, bool isValidParticipant)
    {
        return hasActiveMatch && isValidParticipant && CurrentPhase == GamePhase.RoundBreak;
    }

    public void RequestRoundStartIfReady(
        bool hasActiveMatch, GameOverlay currentOverlay, int readyParticipantCount, int requiredParticipantCount)
    {
        if (!hasActiveMatch) return;
        if (CurrentPhase != GamePhase.RoundBreak) return;
        if (currentOverlay != GameOverlay.None) return;
        if (requiredParticipantCount <= 0) return;
        if (readyParticipantCount != requiredParticipantCount) return;

        RoundStartRequested?.Invoke();
    }
}
