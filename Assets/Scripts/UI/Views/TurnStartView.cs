using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TurnStartView : MonoBehaviour
{
    [Header("Start button settings")]
    [SerializeField] private Button startTurnButton;
    [SerializeField, Range(0.1f, 2f)] private float pulseDuration = 0.75f;
    [SerializeField, Range(0.1f, 2f)] private float fadeToValue = 0.4f;
    [SerializeField] private int countdownSeconds = 3;
    [SerializeField] private float startButtonAppearDelay = 2f;
    
    [Header("Countdown text field")]
    [SerializeField] private TextMeshProUGUI countdownText;

    private Coroutine countdownRoutine;
    private Image startButtonImage;
    private Tween startButtonDelayTween;
    private Tween startButtonPulseTween;

    public event Action CountdownCompleted;

    private void Awake()
    {
        ValidateReferences();
        ResolveStartButtonImage();
    }

    private void OnEnable()
    {
        if (!startTurnButton) return;
        
        startTurnButton.onClick.RemoveListener(BeginCountdown);
        startTurnButton.onClick.AddListener(BeginCountdown);
    }

    private void OnDisable()
    {
        StopCountdown();
        StopStartButtonAnimation();

        if (startTurnButton)
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
        StopStartButtonAnimation();
        SetCountdownText(string.Empty);

        if (!startTurnButton) return;
        
        ResolveStartButtonImage();
        startTurnButton.interactable = false;
        startTurnButton.gameObject.SetActive(false);

        startButtonDelayTween = DOVirtual
            .DelayedCall(Mathf.Max(0f, startButtonAppearDelay), ShowDelayedStartButton);
    }

    public void HideStartButton()
    {
        StopStartButtonAnimation();

        if (!startTurnButton) return;
        
        startTurnButton.gameObject.SetActive(false);
        startTurnButton.interactable = false;
    }

    public void Cancel()
    {
        StopCountdown();
        SetCountdownText(string.Empty);
        HideStartButton();
    }

    private void BeginCountdown()
    {
        if (countdownRoutine != null) return;

        HideStartButton();
        countdownRoutine = StartCoroutine(CountdownRoutine());
    }

    private void ShowDelayedStartButton()
    {
        startButtonDelayTween = null;

        if (!startTurnButton) return;

        ResolveStartButtonImage();

        startTurnButton.gameObject.SetActive(true);
        startTurnButton.interactable = true;

        if (!startButtonImage) return;

        SetStartButtonImageAlpha(1f);

        startButtonPulseTween = startButtonImage
            .DOFade(fadeToValue, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
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
        if (countdownRoutine == null) return;

        StopCoroutine(countdownRoutine);
        countdownRoutine = null;
    }

    private void StopStartButtonAnimation()
    {
        startButtonDelayTween?.Kill();
        startButtonDelayTween = null;

        startButtonPulseTween?.Kill();
        startButtonPulseTween = null;

        if (startButtonImage)
        {
            SetStartButtonImageAlpha(1f);
        }
    }

    private void SetCountdownText(string message)
    {
        if (countdownText)
        {
            countdownText.text = message;
        }
    }

    private void ResolveStartButtonImage()
    {
        if (!startTurnButton)
        {
            startButtonImage = null;
            return;
        }

        if (!startButtonImage)
        {
            startButtonImage = startTurnButton.GetComponent<Image>();
        }
    }

    private void SetStartButtonImageAlpha(float alpha)
    {
        if (!startButtonImage) return;

        var color = startButtonImage.color;
        color.a = alpha;
        startButtonImage.color = color;
    }

    private void ValidateReferences()
    {
        if (!startTurnButton)
        {
            Debug.LogError($"{nameof(TurnStartView)} on {name} requires a StartTurnButton reference.", this);
        }

        if (!countdownText)
        {
            Debug.LogError($"{nameof(TurnStartView)} on {name} requires a CountdownText reference.", this);
        }
    }
}