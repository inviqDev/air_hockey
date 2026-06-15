using UnityEngine;

public sealed class AbilityProgressState
{
    private readonly Timer freeAbilityTimer = new();

    private float initialFreeAbilityDurationSeconds = 1f;
    private float freeAbilityDurationMultiplier = 1f;
    private float nextFreeAbilityDurationSeconds = 1f;

    public int AvailableAbilityPoints { get; private set; }
    public string FreeAbilityTimerText => freeAbilityTimer.GetFormattedMinutesSeconds();

    public void ConfigureFreeAbilityTimer(float initialDurationSeconds, float durationMultiplier)
    {
        initialFreeAbilityDurationSeconds = Mathf.Max(1f, initialDurationSeconds);
        freeAbilityDurationMultiplier = Mathf.Max(1f, durationMultiplier);
        nextFreeAbilityDurationSeconds = initialFreeAbilityDurationSeconds;
        ResetFreeAbilityTimer();
    }

    public void ResetProgression()
    {
        AvailableAbilityPoints = 0;
        nextFreeAbilityDurationSeconds = initialFreeAbilityDurationSeconds;
        ResetFreeAbilityTimer();
    }

    public void StartTurnProgression()
    {
        nextFreeAbilityDurationSeconds = initialFreeAbilityDurationSeconds;
        freeAbilityTimer.SetDecremental(nextFreeAbilityDurationSeconds);
        freeAbilityTimer.Restart();
    }

    public void StopTurnProgression()
    {
        ResetFreeAbilityTimer();
    }

    public bool Tick(float deltaTime)
    {
        if (!freeAbilityTimer.IsRunning) return false;

        var completed = freeAbilityTimer.Tick(deltaTime);
        if (!completed) return false;

        AddAvailableAbilityChoice();
        RestartFreeAbilityTimerWithNextDuration();
        return true;
    }

    public void AddAvailableAbilityChoice(int amount = 1)
    {
        AvailableAbilityPoints += Mathf.Max(0, amount);
    }

    public bool TrySpendAvailableAbilityChoice()
    {
        if (AvailableAbilityPoints <= 0) return false;

        AvailableAbilityPoints--;
        return true;
    }

    private void ResetFreeAbilityTimer()
    {
        freeAbilityTimer.Stop();
        freeAbilityTimer.SetDecremental(nextFreeAbilityDurationSeconds);
        freeAbilityTimer.Reset();
    }

    private void RestartFreeAbilityTimerWithNextDuration()
    {
        nextFreeAbilityDurationSeconds *= freeAbilityDurationMultiplier;
        freeAbilityTimer.SetDecremental(nextFreeAbilityDurationSeconds);
        freeAbilityTimer.Restart();
    }
}
