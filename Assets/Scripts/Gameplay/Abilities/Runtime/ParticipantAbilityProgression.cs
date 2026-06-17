using UnityEngine;

public sealed class ParticipantAbilityProgression
{
    private readonly FreeAbilityTimerProgression progression = new();
    private int availableAbilityPoints;
    
    public int AvailableAbilityPoints => availableAbilityPoints;
    public string FreeAbilityTimerText => progression.TimerCurrentValueText;
    
    public ParticipantAbilityProgression(float initialDurationSeconds, float durationMultiplier)
    {
        progression.ConfigureFreeAbilityTimer(initialDurationSeconds, durationMultiplier);
    }

    public void ResetProgression()
    {
        availableAbilityPoints = 0;
        progression.ResetProgression();
    }

    public void StartTurnProgression()
    {
        progression.StartTurnProgression();
    }

    public void StopTurnProgression()
    {
        progression.StopTurnProgression();
    }

    public bool Tick(float deltaTime)
    {
        var completed = progression.Tick(deltaTime);
        if (completed)
            AddAvailableAbilityPoint();

        return completed;
    }

    private void AddAvailableAbilityPoint(int amount = 1)
    {
        availableAbilityPoints += Mathf.Max(0, amount);
    }

    public bool TrySpendAvailableAbilityPoint()
    {
        if (availableAbilityPoints <= 0) return false;

        availableAbilityPoints--;
        return true;
    }
}
