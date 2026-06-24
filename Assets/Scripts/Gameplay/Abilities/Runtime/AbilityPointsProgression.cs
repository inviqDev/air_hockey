using UnityEngine;

public sealed class AbilityPointsProgression
{
    private readonly FreeAbilityTimer timer = new();
    private int availableAbilityPoints;
    
    public int AvailableAbilityPoints => availableAbilityPoints;
    public int FreeAbilityTimerDisplaySeconds => timer.TimerCurrentDisplaySeconds;
    public string FreeAbilityTimerText => timer.TimerCurrentValueText;
    
    public AbilityPointsProgression(float initialDurationSeconds, float durationMultiplier)
    {
        timer.ConfigureFreeAbilityTimer(initialDurationSeconds, durationMultiplier);
    }

    public void ResetPointsProgression()
    {
        availableAbilityPoints = 0;
        timer.ResetTimer();
    }

    public void StartAbilityPointsTurnProgression()
    {
        timer.StartTurnTimer();
    }

    public void StopAbilityPointsTurnProgression()
    {
        timer.StopTurnTimer();
    }

    public bool Tick(float deltaTime)
    {
        var completed = timer.Tick(deltaTime);
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
