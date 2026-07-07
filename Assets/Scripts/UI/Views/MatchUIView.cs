using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public sealed class MatchUIView : MenuViewBase
{
    [SerializeField] private TextMeshProUGUI leftScoreText;
    [SerializeField] private TextMeshProUGUI rightScoreText;
    [SerializeField] private TextMeshProUGUI goalPresentationText;

    [Header("Turn Timer")]
    [SerializeField] private TurnController turnController;
    [SerializeField] private TextMeshProUGUI turnTimerText;

    [Header("Goal Presentation Animation")]
    [SerializeField] private Vector2 startAnchoredPosition = Vector2.zero;
    [SerializeField] private float moveUpDistance = 220f;
    [SerializeField] private float animationSeconds = 2.5f;
    [SerializeField] private Ease moveEase = Ease.OutSine;
    [SerializeField] private Ease fadeEase = Ease.InExpo;

    private Sequence goalPresentationSequence;
    private Action goalPresentationCompletedCallback;

    private readonly Timer turnTimer = new();

    private void Awake()
    {
        ConfigureTurnTimer();
    }

    private void OnEnable()
    {
        SubscribeToTurnEvents();
    }

    public void SetScores(int leftScore, int rightScore)
    {
        if (leftScoreText)
            leftScoreText.text = leftScore.ToString();

        if (rightScoreText)
            rightScoreText.text = rightScore.ToString();
    }

    public void SetGoalPresentationText(string message)
    {
        StopGoalPresentationAnimation();

        if (!goalPresentationText) return;

        goalPresentationText.text = message;
        goalPresentationText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        SetGoalPresentationAlpha(1f);
        goalPresentationText.rectTransform.anchoredPosition = startAnchoredPosition;
    }

    public void PlayGoalPresentation(string message, Action onCompleted = null)
    {
        if (!goalPresentationText)
        {
            onCompleted?.Invoke();
            return;
        }

        StopGoalPresentationAnimation();
        goalPresentationCompletedCallback = onCompleted;

        var duration = Mathf.Max(0.01f, animationSeconds);
        var goalPresentationRect = goalPresentationText.rectTransform;
        var endPosition = startAnchoredPosition + Vector2.up * moveUpDistance;

        goalPresentationText.text = message;
        goalPresentationText.gameObject.SetActive(true);
        SetGoalPresentationAlpha(1f);
        goalPresentationRect.anchoredPosition = startAnchoredPosition;

        goalPresentationSequence = DOTween.Sequence();
        goalPresentationSequence.Join(goalPresentationRect.DOAnchorPos(endPosition, duration).SetEase(moveEase));
        goalPresentationSequence.Join(goalPresentationText.DOFade(0f, duration).SetEase(fadeEase));
        goalPresentationSequence.OnComplete(HandleGoalPresentationAnimationCompleted);
    }

    private void OnValidate()
    {
        ConfigureTurnTimer();
        ValidateReferences();
    }

    private void Update()
    {
        if (!turnTimer.IsRunning) return;

        turnTimer.Tick(Time.deltaTime);
        UpdateTurnTimerText();
    }

    private void OnDisable()
    {
        UnsubscribeFromTurnEvents();
        StopGoalPresentationAnimation();
        StopAndResetTurnTimer();
    }

    protected override void HandleAfterInitialize()
    {
        ConfigureTurnTimer();
        ValidateReferences();
        HideGoalPresentationImmediately();
        UpdateTurnTimerText();
    }

    public void StartTurnTimer()
    {
        ConfigureTurnTimer();
        turnTimer.Restart();
        UpdateTurnTimerText();
    }

    public void RestartTurnTimer()
    {
        StartTurnTimer();
    }

    public void StopAndResetTurnTimer()
    {
        turnTimer.Stop();
        turnTimer.Reset();
        UpdateTurnTimerText();
    }

    public void ResetMatchSessionState()
    {
        StopGoalPresentationAnimation();
        HideGoalPresentationElement();
        StopAndResetTurnTimer();
    }

    private void HideGoalPresentationImmediately()
    {
        StopGoalPresentationAnimation();
        HideGoalPresentationElement();
    }

    private void HandleGoalPresentationAnimationCompleted()
    {
        goalPresentationSequence = null;
        HideGoalPresentationElement();

        var completed = goalPresentationCompletedCallback;
        goalPresentationCompletedCallback = null;
        completed?.Invoke();
    }

    private void HideGoalPresentationElement()
    {
        if (!goalPresentationText) return;

        goalPresentationText.text = string.Empty;
        goalPresentationText.rectTransform.anchoredPosition = startAnchoredPosition;
        SetGoalPresentationAlpha(0f);
        goalPresentationText.gameObject.SetActive(false);
    }

    private void StopGoalPresentationAnimation()
    {
        if (goalPresentationSequence == null) return;

        goalPresentationSequence.Kill();
        goalPresentationSequence = null;
        goalPresentationCompletedCallback = null;
    }

    private void SetGoalPresentationAlpha(float alpha)
    {
        if (!goalPresentationText) return;

        var color = goalPresentationText.color;
        color.a = alpha;
        goalPresentationText.color = color;
    }

    private void ConfigureTurnTimer()
    {
        turnTimer.SetIncremental(0f);
    }

    private void UpdateTurnTimerText()
    {
        if (!turnTimerText) return;

        turnTimerText.text = turnTimer.GetFormattedMinutesSeconds();
    }

    private void SubscribeToTurnEvents()
    {
        if (!turnController) return;

        turnController.TurnStarted += HandleTurnStarted;
        turnController.TurnEnded += HandleTurnEnded;
    }

    private void UnsubscribeFromTurnEvents()
    {
        if (!turnController) return;

        turnController.TurnStarted -= HandleTurnStarted;
        turnController.TurnEnded -= HandleTurnEnded;
    }

    private void HandleTurnStarted()
    {
        StartTurnTimer();
    }

    private void HandleTurnEnded()
    {
        StopAndResetTurnTimer();
    }

    private void ValidateReferences()
    {
        if (!leftScoreText)
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a LeftScoreText reference.", this);

        if (!rightScoreText)
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a RightScoreText reference.", this);

        if (!goalPresentationText)
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a GoalPresentationText reference.", this);
    }
}
