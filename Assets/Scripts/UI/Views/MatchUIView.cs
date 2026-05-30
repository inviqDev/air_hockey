using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MatchUIView : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI leftScoreText;
    [SerializeField] private TextMeshProUGUI rightScoreText;
    [SerializeField] private TextMeshProUGUI goalInfoText;
    [SerializeField] private Button restartButton;

    [Header("Goal Info Animation")]
    [SerializeField] private Vector2 goalInfoStartAnchoredPosition = Vector2.zero;
    [SerializeField] private float goalInfoMoveUpDistance = 220f;
    [SerializeField] private float goalInfoAnimationSeconds = 2.5f;
    [SerializeField] private Ease goalInfoMoveEase = Ease.OutSine;
    [SerializeField] private Ease goalInfoFadeEase = Ease.InExpo;

    private Sequence goalInfoSequence;

    public event Action RestartClicked;

    public void Initialize()
    {
        ValidateReferences();

        if (restartButton)
        {
            restartButton.onClick.RemoveListener(RestartMatch);
            restartButton.onClick.AddListener(RestartMatch);
        }

        HideGoalInfoImmediately();
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
        RestartClicked?.Invoke();
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

        if (goalInfoText == null)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a GoalInfoText reference.", this);
        }

        if (restartButton == null)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a RestartButton reference.", this);
        }
    }
}
