using TMPro;
using UnityEngine;

public sealed class TurnTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private bool isRunning;
    private float elapsedSeconds;

    private void Awake()
    {
        ValidateReferences();
        ResetTimer();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void Update()
    {
        if (!isRunning) return;

        elapsedSeconds += Time.deltaTime;
        UpdateText();
    }

    public void StartTimer()
    {
        elapsedSeconds = 0f;
        isRunning = true;
        UpdateText();
    }

    public void StopAndReset()
    {
        isRunning = false;
        ResetTimer();
    }

    private void ResetTimer()
    {
        elapsedSeconds = 0f;
        UpdateText();
    }

    private void UpdateText()
    {
        if (timerText)
            timerText.text = elapsedSeconds.ToString("0.00");
    }

    private void ValidateReferences()
    {
        if (!timerText)
            Debug.LogError($"{nameof(TurnTimer)} on {name} requires a TimerText reference.", this);
    }
}
