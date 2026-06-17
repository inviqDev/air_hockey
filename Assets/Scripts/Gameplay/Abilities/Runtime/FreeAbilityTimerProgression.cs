using UnityEngine;

public sealed class FreeAbilityTimerProgression
{
    private readonly Timer freeAbilityTimer = new();

    private float initialTimerDuration = 1f;
    private float durationMultiplier = 1f;
    private float nextTimerDuration = 1f;

    public string TimerCurrentValueText => freeAbilityTimer.GetFormattedMinutesSeconds();

    public void ConfigureFreeAbilityTimer(float initialDurationSeconds, float multiplier)
    {
        initialTimerDuration = Mathf.Max(1f, initialDurationSeconds);
        durationMultiplier = Mathf.Max(1f, multiplier);
        nextTimerDuration = initialTimerDuration;
        ResetFreeAbilityTimer();
    }

    public void ResetProgression()
    {
        nextTimerDuration = initialTimerDuration;
        ResetFreeAbilityTimer();
    }

    public void StartTurnProgression()
    {
        freeAbilityTimer.Start();
    }

    public void StopTurnProgression()
    {
        freeAbilityTimer.Stop();
    }

    public bool Tick(float deltaTime)
    {
        if (!freeAbilityTimer.IsRunning) return false;

        var completed = freeAbilityTimer.Tick(deltaTime);
        if (!completed) return false;

        RestartFreeAbilityTimerWithNextDuration();
        return true;
    }

    private void ResetFreeAbilityTimer()
    {
        freeAbilityTimer.Stop();
        freeAbilityTimer.SetDecremental(nextTimerDuration);
        freeAbilityTimer.Reset();
    }

    private void RestartFreeAbilityTimerWithNextDuration()
    {
        nextTimerDuration *= durationMultiplier;
        freeAbilityTimer.SetDecremental(nextTimerDuration);
        freeAbilityTimer.Restart();
    }
}
