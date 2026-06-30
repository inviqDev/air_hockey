using UnityEngine;

public sealed class Timer
{
    public enum TimerMode
    {
        Incremental,
        Decremental
    }

    public TimerMode Mode { get; private set; }
    public float StartValueSeconds { get; private set; }
    public float CurrentSeconds { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsFinished => Mode == TimerMode.Decremental && CurrentSeconds <= 0f;

    public void SetIncremental(float startValueSeconds = 0f)
    {
        Mode = TimerMode.Incremental;
        StartValueSeconds = Mathf.Max(0f, startValueSeconds);
        CurrentSeconds = StartValueSeconds;
        IsRunning = false;
    }

    public void SetDecremental(float startValueSeconds)
    {
        Mode = TimerMode.Decremental;
        StartValueSeconds = Mathf.Max(0f, startValueSeconds);
        CurrentSeconds = StartValueSeconds;
        IsRunning = false;
    }

    public void Start()
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void Reset()
    {
        CurrentSeconds = StartValueSeconds;
    }

    public void Restart()
    {
        Reset();
        Start();
    }

    public bool Tick(float deltaTime)
    {
        if (!IsRunning) return false;
        if (deltaTime <= 0f) return false;

        if (Mode == TimerMode.Incremental)
        {
            CurrentSeconds += deltaTime;
            return false;
        }

        var hadTimeRemaining = CurrentSeconds > 0f;
        CurrentSeconds = Mathf.Max(0f, CurrentSeconds - deltaTime);

        if (!hadTimeRemaining || CurrentSeconds > 0f) return false;

        IsRunning = false;
        return true;
    }

    public string GetFormattedMinutesSeconds()
    {
        return FormatMinutesSeconds(CurrentSeconds, Mode);
    }

    public static string FormatMinutesSeconds(float seconds, TimerMode mode)
    {
        var clampedSeconds = Mathf.Max(0f, seconds);
        var roundedSeconds = mode == TimerMode.Decremental
            ? Mathf.CeilToInt(clampedSeconds)
            : Mathf.FloorToInt(clampedSeconds);

        var minutes = roundedSeconds / 60;
        var remainingSeconds = roundedSeconds % 60;

        return $"{minutes}:{remainingSeconds:00}";
    }
}
