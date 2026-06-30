using UnityEngine;

public sealed class FreeAbilityTimer
{
    private readonly Timer freeAbilityTimer = new();

    private float initialTimerDuration = 1f;
    private float durationMultiplier = 1f;
    private float nextTimerDuration = 1f;

    public string TimerCurrentValueText => freeAbilityTimer.GetFormattedMinutesSeconds();
    public int TimerCurrentDisplaySeconds => GetDisplaySeconds();

    public void ConfigureFreeAbilityTimer(float initialDurationSeconds, float multiplier)
    {
        initialTimerDuration = Mathf.Max(1f, initialDurationSeconds);
        durationMultiplier = Mathf.Max(1f, multiplier);
        nextTimerDuration = initialTimerDuration;
        ResetFreeAbilityTimer();
    }

    public void ResetTimer()
    {
        nextTimerDuration = initialTimerDuration;
        ResetFreeAbilityTimer();
    }

    public void StartTurnTimer()
    {
        freeAbilityTimer.Start();
    }

    public void StopTurnTimer()
    {
        freeAbilityTimer.Stop();
    }

    public bool Tick(float deltaTime)
    {
        if (!freeAbilityTimer.IsRunning) return false;

        var completed = freeAbilityTimer.Tick(deltaTime);
        if (!completed) return false;

        RestartTimerWithIncreasedDuration();
        return true;
    }

    private void ResetFreeAbilityTimer()
    {
        freeAbilityTimer.Stop();
        freeAbilityTimer.SetDecremental(nextTimerDuration);
        freeAbilityTimer.Reset();
    }

    private void RestartTimerWithIncreasedDuration()
    {
        nextTimerDuration *= durationMultiplier;
        freeAbilityTimer.SetDecremental(nextTimerDuration);
        freeAbilityTimer.Restart();
    }

    private int GetDisplaySeconds()
    {
        var clampedSeconds = Mathf.Max(0f, freeAbilityTimer.CurrentSeconds);
        return freeAbilityTimer.Mode == Timer.TimerMode.Decremental
            ? Mathf.CeilToInt(clampedSeconds)
            : Mathf.FloorToInt(clampedSeconds);
    }
}
