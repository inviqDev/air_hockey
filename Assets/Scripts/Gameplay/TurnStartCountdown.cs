using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TurnStartCountdown : MonoBehaviour
{
    [SerializeField] private Button startTurnButton;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private int countdownSeconds = 3;

    private Coroutine countdownRoutine;

    public event Action CountdownCompleted;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnEnable()
    {
        if (startTurnButton != null)
        {
            startTurnButton.onClick.RemoveListener(BeginCountdown);
            startTurnButton.onClick.AddListener(BeginCountdown);
        }
    }

    private void OnDisable()
    {
        StopCountdown();

        if (startTurnButton != null)
        {
            startTurnButton.onClick.RemoveListener(BeginCountdown);
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void ShowStartButton()
    {
        StopCountdown();
        SetCountdownText(string.Empty);

        if (startTurnButton != null)
        {
            startTurnButton.gameObject.SetActive(true);
            startTurnButton.interactable = true;
        }
    }

    public void HideStartButton()
    {
        if (startTurnButton != null)
        {
            startTurnButton.gameObject.SetActive(false);
            startTurnButton.interactable = false;
        }
    }

    public void Cancel()
    {
        StopCountdown();
        SetCountdownText(string.Empty);
        HideStartButton();
    }

    private void BeginCountdown()
    {
        if (countdownRoutine != null)
        {
            return;
        }

        HideStartButton();
        countdownRoutine = StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        var seconds = Mathf.Max(1, countdownSeconds);

        for (var secondsRemaining = seconds; secondsRemaining > 0; secondsRemaining--)
        {
            SetCountdownText(secondsRemaining.ToString());
            yield return new WaitForSeconds(1f);
        }

        countdownRoutine = null;
        SetCountdownText(string.Empty);
        CountdownCompleted?.Invoke();
    }

    private void StopCountdown()
    {
        if (countdownRoutine == null)
        {
            return;
        }

        StopCoroutine(countdownRoutine);
        countdownRoutine = null;
    }

    private void SetCountdownText(string message)
    {
        if (countdownText != null)
        {
            countdownText.text = message;
        }
    }

    private void ValidateReferences()
    {
        if (startTurnButton == null)
        {
            Debug.LogError($"{nameof(TurnStartCountdown)} on {name} requires a StartTurnButton reference.", this);
        }

        if (countdownText == null)
        {
            Debug.LogError($"{nameof(TurnStartCountdown)} on {name} requires a CountdownText reference.", this);
        }
    }
}
