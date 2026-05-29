using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MatchUIView : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI leftScoreText;
    [SerializeField] private TextMeshProUGUI rightScoreText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI goalInfoText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button restartButton;

    [Header("Goal Info Animation")]
    [SerializeField] private Vector2 goalInfoStartAnchoredPosition = Vector2.zero;
    [SerializeField] private float goalInfoMoveUpDistance = 220f;
    [SerializeField] private float goalInfoAnimationSeconds = 2.5f;
    [SerializeField] private Ease goalInfoMoveEase = Ease.OutSine;
    [SerializeField] private Ease goalInfoFadeEase = Ease.InExpo;

    private MatchManager matchManager;
    private Sequence goalInfoSequence;

    public void Initialize(MatchManager manager)
    {
        matchManager = manager;
        ValidateReferences();

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartMatch);
            restartButton.onClick.AddListener(RestartMatch);
        }

        if (readyButton != null)
        {
            readyButton.onClick.RemoveListener(BeginTurnCountdown);
            readyButton.onClick.AddListener(BeginTurnCountdown);
        }

        HideGoalInfoImmediately();
    }

    public void SetScores(int leftScore, int rightScore)
    {
        if (leftScoreText != null)
        {
            leftScoreText.text = leftScore.ToString();
        }

        if (rightScoreText != null)
        {
            rightScoreText.text = rightScore.ToString();
        }
    }

    public void SetCountdownText(string message)
    {
        if (countdownText != null)
        {
            countdownText.text = message;
        }
    }

    public void SetGoalInfoText(string message)
    {
        StopGoalInfoAnimation();

        if (goalInfoText != null)
        {
            goalInfoText.text = message;
            goalInfoText.gameObject.SetActive(!string.IsNullOrEmpty(message));
            SetGoalInfoAlpha(1f);
            goalInfoText.rectTransform.anchoredPosition = goalInfoStartAnchoredPosition;
        }
    }

    public void PlayGoalInfo(string message)
    {
        if (goalInfoText == null)
        {
            return;
        }

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

    public void ShowReadyButton(bool show)
    {
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(show);
            readyButton.interactable = show;
        }
    }

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void OnDisable()
    {
        StopGoalInfoAnimation();
    }

    private void RestartMatch()
    {
        matchManager?.RestartMatch();
    }

    private void BeginTurnCountdown()
    {
        matchManager?.BeginTurnCountdown();
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
        if (goalInfoText == null)
        {
            return;
        }

        goalInfoText.text = string.Empty;
        goalInfoText.rectTransform.anchoredPosition = goalInfoStartAnchoredPosition;
        SetGoalInfoAlpha(0f);
        goalInfoText.gameObject.SetActive(false);
    }

    private void StopGoalInfoAnimation()
    {
        if (goalInfoSequence == null)
        {
            return;
        }

        goalInfoSequence.Kill();
        goalInfoSequence = null;
    }

    private void SetGoalInfoAlpha(float alpha)
    {
        if (goalInfoText == null)
        {
            return;
        }

        var color = goalInfoText.color;
        color.a = alpha;
        goalInfoText.color = color;
    }

    private void ValidateReferences()
    {
        if (canvas == null)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a Canvas reference.", this);
        }

        if (leftScoreText == null)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a LeftScoreText reference.", this);
        }

        if (rightScoreText == null)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a RightScoreText reference.", this);
        }

        if (countdownText == null)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a CountdownText reference.", this);
        }

        if (goalInfoText == null)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a GoalInfoText reference.", this);
        }

        if (readyButton == null)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a ReadyButton reference.", this);
        }

        if (restartButton == null)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a RestartButton reference.", this);
        }
    }
}
