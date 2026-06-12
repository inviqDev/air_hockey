using DG.Tweening;
using TMPro;
using UnityEngine;

public sealed class MatchUIView : MenuViewBase
{
    [SerializeField] private TextMeshProUGUI leftScoreText;
    [SerializeField] private TextMeshProUGUI rightScoreText;
    [SerializeField] private TextMeshProUGUI goalInfoText;

    [Header("Turn Timer")]
    [SerializeField] private TurnController turnController;
    [SerializeField] private TextMeshProUGUI turnTimerText;

    [Header("Goal Info Animation")]
    [SerializeField] private Vector2 goalInfoStartAnchoredPosition = Vector2.zero;
    [SerializeField] private float goalInfoMoveUpDistance = 220f;
    [SerializeField] private float goalInfoAnimationSeconds = 2.5f;
    [SerializeField] private Ease goalInfoMoveEase = Ease.OutSine;
    [SerializeField] private Ease goalInfoFadeEase = Ease.InExpo;

    private Sequence goalInfoSequence;
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
        {
            leftScoreText.text = leftScore.ToString();
        }

        if (rightScoreText)
        {
            rightScoreText.text = rightScore.ToString();
        }
    }

    public void SetGoalInfoText(string message)
    {
        StopGoalInfoAnimation();

        if (!goalInfoText) return;
        
        goalInfoText.text = message;
        goalInfoText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        SetGoalInfoAlpha(1f);
        goalInfoText.rectTransform.anchoredPosition = goalInfoStartAnchoredPosition;
    }

    public void PlayGoalInfo(string message)
    {
        if (!goalInfoText) return;

        StopGoalInfoAnimation();

        var duration = Mathf.Max(0.01f, goalInfoAnimationSeconds);
        var goalInfoRect = goalInfoText.rectTransform;
        var endPosition = goalInfoStartAnchoredPosition + Vector2.up * goalInfoMoveUpDistance;

        goalInfoText.text = message;
        goalInfoText.gameObject.SetActive(true);
        SetGoalInfoAlpha(1f);
        goalInfoRect.anchoredPosition = goalInfoStartAnchoredPosition;

        goalInfoSequence = DOTween.Sequence();
        goalInfoSequence.Join(goalInfoRect.DOAnchorPos(endPosition, duration).SetEase(goalInfoMoveEase));
        goalInfoSequence.Join(goalInfoText.DOFade(0f, duration).SetEase(goalInfoFadeEase));
        goalInfoSequence.OnComplete(CompleteGoalInfoAnimation);
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
        StopGoalInfoAnimation();
    }

    protected override void HandleAfterInitialize()
    {
        ConfigureTurnTimer();
        ValidateReferences();
        HideGoalInfoImmediately();
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

    private void HideGoalInfoImmediately()
    {
        StopGoalInfoAnimation();
        HideGoalInfoElement();
    }

    private void CompleteGoalInfoAnimation()
    {
        goalInfoSequence = null;
        HideGoalInfoElement();
    }

    private void HideGoalInfoElement()
    {
        if (!goalInfoText) return;

        goalInfoText.text = string.Empty;
        goalInfoText.rectTransform.anchoredPosition = goalInfoStartAnchoredPosition;
        SetGoalInfoAlpha(0f);
        goalInfoText.gameObject.SetActive(false);
    }

    private void StopGoalInfoAnimation()
    {
        if (goalInfoSequence == null) return;

        goalInfoSequence.Kill();
        goalInfoSequence = null;
    }

    private void SetGoalInfoAlpha(float alpha)
    {
        if (!goalInfoText) return;

        var color = goalInfoText.color;
        color.a = alpha;
        goalInfoText.color = color;
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
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a LeftScoreText reference.", this);
        }

        if (!rightScoreText)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a RightScoreText reference.", this);
        }

        if (!goalInfoText)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a GoalInfoText reference.", this);
        }
    }
}
