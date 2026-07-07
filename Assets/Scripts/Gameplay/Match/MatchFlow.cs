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

    public bool ShouldCancelRoundStart(bool hasActiveMatch, int readyParticipantCount, int requiredParticipantCount)
    {
        if (!hasActiveMatch) return false;
        if (CurrentPhase != GamePhase.RoundBreak) return false;
        if (requiredParticipantCount <= 0) return true;

        return readyParticipantCount != requiredParticipantCount;
    }

    public bool TryCompleteRoundStartCountdown(
        bool hasActiveMatch,
        GameOverlay currentOverlay,
        int readyParticipantCount,
        int requiredParticipantCount)
    {
        if (!hasActiveMatch) return false;
        if (CurrentPhase != GamePhase.RoundBreak) return false;
        if (currentOverlay != GameOverlay.None) return false;
        if (requiredParticipantCount <= 0) return false;
        if (readyParticipantCount != requiredParticipantCount) return false;

        return true;
    }

    public bool TryCompleteRoundPreparation(bool didPrepareRound)
    {
        if (CurrentPhase != GamePhase.RoundBreak) return false;
        if (!didPrepareRound) return false;

        TransitionToPhase(GamePhase.TurnActive);
        return true;
    }

    public bool TryEnterGoalPresentation(bool hasActiveMatch)
    {
        if (!hasActiveMatch) return false;
        if (CurrentPhase != GamePhase.TurnActive) return false;

        TransitionToPhase(GamePhase.GoalPresentation);
        return true;
    }

    public bool TryCompleteGoalPresentation(bool hasWinner)
    {
        if (CurrentPhase != GamePhase.GoalPresentation) return false;

        var nextPhase = !hasWinner
            ? GamePhase.RoundBreak
            : GamePhase.MatchComplete;

        TransitionToPhase(nextPhase);
        return true;
    }
}
