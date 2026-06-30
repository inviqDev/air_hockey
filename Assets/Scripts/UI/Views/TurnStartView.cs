using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TurnStartView : MenuViewBase
{
    [Header("Start button settings")] 
    [SerializeField] private GameObject startRoundButtonsRoot;

    [Header("Buttons")] 
    [SerializeField] private Button startTurnButton;
    [SerializeField] private Button respawnItemsButton;

    [Header("DOTween anim settings")] 
    [SerializeField, Range(0.1f, 2f)] private float pulseDuration = 0.75f;

    [SerializeField, Range(0.1f, 2f)] private float fadeToValue = 0.4f;
    [SerializeField] private int countdownSeconds = 3;
    [SerializeField] private float startButtonAppearDelay = 1f;

    [Header("Countdown text field")] 
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI spawnErrorMessageText;

    private Image startButtonImage;
    private Tween startButtonDelayTween;
    private Tween startButtonPulseTween;

    private Coroutine countdownRoutine;
    private bool canStartTurn;
    private int currentCountdownValue;
    private bool shouldResumeCountdownAfterEnable;
    private const string SpawnErrorMessage = "Round setup failed.\nPress Respawn Items to rebuild it.";

    public event Action CountdownCompleted;
    public event Action RespawnItemsRequested;

    private void OnEnable()
    {
        if (startTurnButton)
        {
            startTurnButton.onClick.RemoveListener(BeginCountdown);
            startTurnButton.onClick.AddListener(BeginCountdown);
        }

        if (respawnItemsButton)
        {
            respawnItemsButton.onClick.RemoveListener(RequestRespawnItems);
            respawnItemsButton.onClick.AddListener(RequestRespawnItems);
        }

        ResumeCountdownIfNeeded();
    }

    private void OnDisable()
    {
        PauseCountdownForViewDisable();
        StopStartButtonAnimation();

        if (startTurnButton)
            startTurnButton.onClick.RemoveListener(BeginCountdown);

        if (respawnItemsButton)
            respawnItemsButton.onClick.RemoveListener(RequestRespawnItems);
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    protected override void HandleAfterInitialize()
    {
        ValidateReferences();
        CacheReferences();
        ResolveStartButtonImage();
        SetCountdownText(string.Empty);
        SetStartRoundButtonsVisible(false);
        SetSpawnErrorMessageVisible(false);

        if (startTurnButton)
            startTurnButton.interactable = false;

        if (respawnItemsButton)
            respawnItemsButton.interactable = false;
    }

    public void ShowTurnPreparation(bool isTurnReadyToStart)
    {
        ResetCountdownState();
        ShowImmediately();
        StopStartButtonAnimation();
        SetCountdownText(string.Empty);
        SetSpawnErrorMessageVisible(false);

        if (!startTurnButton) return;

        canStartTurn = isTurnReadyToStart;
        CacheReferences();
        ResolveStartButtonImage();
        startTurnButton.interactable = false;
        SetStartRoundButtonsVisible(false);

        startButtonDelayTween = DOVirtual
            .DelayedCall(Mathf.Max(0f, startButtonAppearDelay), ShowDelayedTurnPreparation);
    }

    public void HideTurnPreparation()
    {
        StopStartButtonAnimation();

        if (!startTurnButton) return;

        SetStartRoundButtonsVisible(false);
        startTurnButton.interactable = false;
        SetSpawnErrorMessageVisible(false);

        if (respawnItemsButton)
            respawnItemsButton.interactable = false;
    }

    public void Cancel()
    {
        ResetCountdownState();
        SetCountdownText(string.Empty);
        HideTurnPreparation();
    }

    private void BeginCountdown()
    {
        if (countdownRoutine != null) return;

        HideTurnPreparation();
        currentCountdownValue = Mathf.Max(1, countdownSeconds);
        shouldResumeCountdownAfterEnable = false;
        countdownRoutine = StartCoroutine(CountdownRoutine(currentCountdownValue));
    }

    private void RequestRespawnItems()
    {
        if (countdownRoutine != null) return;

        RespawnItemsRequested?.Invoke();
    }

    private void ShowDelayedTurnPreparation()
    {
        startButtonDelayTween = null;

        if (!startTurnButton) return;

        CacheReferences();
        ResolveStartButtonImage();
        SetStartRoundButtonsVisible(true);

        startTurnButton.gameObject.SetActive(canStartTurn);
        startTurnButton.interactable = canStartTurn;
        SetSpawnErrorMessageVisible(!canStartTurn);

        if (respawnItemsButton)
        {
            var shouldShowRespawnButton = !canStartTurn;
            respawnItemsButton.gameObject.SetActive(shouldShowRespawnButton);
            respawnItemsButton.interactable = shouldShowRespawnButton;
        }

        if (!canStartTurn) return;
        if (!startButtonImage) return;

        SetStartButtonImageAlpha(1f);

        startButtonPulseTween = startButtonImage
            .DOFade(fadeToValue, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private IEnumerator CountdownRoutine(int startingValue)
    {
        for (var secondsRemaining = startingValue; secondsRemaining > 0; secondsRemaining--)
        {
            currentCountdownValue = secondsRemaining;
            SetCountdownText(secondsRemaining.ToString());
            yield return new WaitForSeconds(1f);
        }

        countdownRoutine = null;
        currentCountdownValue = 0;
        shouldResumeCountdownAfterEnable = false;
        SetCountdownText(string.Empty);
        CountdownCompleted?.Invoke();
    }

    private void StopCountdown()
    {
        if (countdownRoutine == null) return;

        StopCoroutine(countdownRoutine);
        countdownRoutine = null;
    }

    private void PauseCountdownForViewDisable()
    {
        if (countdownRoutine == null) return;

        StopCoroutine(countdownRoutine);
        countdownRoutine = null;
        shouldResumeCountdownAfterEnable = currentCountdownValue > 0;
    }

    private void ResumeCountdownIfNeeded()
    {
        if (!shouldResumeCountdownAfterEnable) return;
        if (currentCountdownValue <= 0) return;
        if (countdownRoutine != null) return;

        shouldResumeCountdownAfterEnable = false;
        SetCountdownText(currentCountdownValue.ToString());
        countdownRoutine = StartCoroutine(CountdownRoutine(currentCountdownValue));
    }

    private void ResetCountdownState()
    {
        StopCountdown();
        currentCountdownValue = 0;
        shouldResumeCountdownAfterEnable = false;
    }

    private void StopStartButtonAnimation()
    {
        startButtonDelayTween?.Kill();
        startButtonDelayTween = null;

        startButtonPulseTween?.Kill();
        startButtonPulseTween = null;

        if (startButtonImage)
            SetStartButtonImageAlpha(1f);
    }

    private void SetCountdownText(string message)
    {
        if (!countdownText) return;

        var shouldShowCountdown = !string.IsNullOrEmpty(message);
        countdownText.gameObject.SetActive(shouldShowCountdown);
        countdownText.text = message;
    }

    private void SetSpawnErrorMessageVisible(bool isVisible)
    {
        if (!spawnErrorMessageText) return;

        spawnErrorMessageText.gameObject.SetActive(isVisible);

        if (isVisible)
            spawnErrorMessageText.text = SpawnErrorMessage;
    }

    private void ResolveStartButtonImage()
    {
        if (!startTurnButton)
        {
            startButtonImage = null;
            return;
        }

        if (!startButtonImage)
            startButtonImage = startTurnButton.GetComponent<Image>();
    }

    private void CacheReferences()
    {
        if (!startRoundButtonsRoot && startTurnButton)
            startRoundButtonsRoot = startTurnButton.transform.parent.gameObject;
    }

    private void SetStartRoundButtonsVisible(bool isVisible)
    {
        if (startRoundButtonsRoot)
            startRoundButtonsRoot.SetActive(isVisible);
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
        if (!startRoundButtonsRoot && startTurnButton)
            startRoundButtonsRoot = startTurnButton.transform.parent.gameObject;

        if (!startRoundButtonsRoot)
            Debug.LogError($"{nameof(TurnStartView)} on {name} requires a StartRoundButtonsRoot reference.", this);

        if (!startTurnButton)
            Debug.LogError($"{nameof(TurnStartView)} on {name} requires a StartTurnButton reference.", this);

        if (!respawnItemsButton)
            Debug.LogError($"{nameof(TurnStartView)} on {name} requires a RespawnItemsButton reference.", this);

        if (!countdownText)
            Debug.LogError($"{nameof(TurnStartView)} on {name} requires a CountdownText reference.", this);

        if (!spawnErrorMessageText)
            Debug.LogError($"{nameof(TurnStartView)} on {name} requires a SpawnErrorMessageText reference.", this);
    }
}
