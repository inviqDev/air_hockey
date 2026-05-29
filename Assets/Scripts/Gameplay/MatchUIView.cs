using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MatchUIView : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI leftScoreText;
    [SerializeField] private TextMeshProUGUI rightScoreText;
    [SerializeField] private TextMeshProUGUI matchStateText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button restartButton;

    private MatchManager matchManager;

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

    public void SetStateText(string message)
    {
        if (matchStateText != null)
        {
            matchStateText.text = message;
        }
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

    private void RestartMatch()
    {
        matchManager?.RestartMatch();
    }

    private void BeginTurnCountdown()
    {
        matchManager?.BeginTurnCountdown();
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

        if (matchStateText == null)
        {
            Debug.LogError($"{nameof(MatchUIView)} on {name} requires a MatchStateText reference.", this);
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
