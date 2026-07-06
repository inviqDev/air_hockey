using System;

public sealed class MatchFlow
{
    public GamePhase CurrentPhase { get; private set; } = GamePhase.NoActiveMatch;

    public event Action<GamePhase, GamePhase> PhaseChanged;

    public void TransitionToPhase(GamePhase nextPhase)
    {
        if (CurrentPhase == nextPhase) return;

        var previousPhase = CurrentPhase;
        CurrentPhase = nextPhase;
        PhaseChanged?.Invoke(previousPhase, CurrentPhase);
    }
}
