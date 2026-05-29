using UnityEngine;

public sealed class MatchManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MatchUIView matchView;
    [SerializeField] private ScoreKeeper scoreKeeper;
    [SerializeField] private ServeManager serveManager;
    [SerializeField] private RoundResetter roundResetter;
    [SerializeField] private TurnFlowController turnFlow;

    [Header("Match")]
    [SerializeField] private float goalLockoutSeconds = 0.25f;

    private float nextGoalAllowedTime;

    public static MatchManager Instance { get; private set; }
    public int LeftScore => scoreKeeper ? scoreKeeper.LeftScore : 0;
    public int RightScore => scoreKeeper ? scoreKeeper.RightScore : 0;
    public bool IsTurnActive => turnFlow && turnFlow.IsTurnActive;

    private void Awake()
    {
        Instance = this;
        ValidateReferences();
    }

    private void Start()
    {
        if (matchView)
        {
            matchView.Initialize(this);
            matchView.SetScores(LeftScore, RightScore);
        }

        PrepareNextTurn(string.Empty);
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public bool IsPuck(Rigidbody2D candidate)
    {
        return roundResetter != null && roundResetter.IsPuck(candidate);
    }

    public void HandleGoal(PlayerSide goalSide)
    {
        if (Time.time < nextGoalAllowedTime)
        {
            return;
        }

        nextGoalAllowedTime = Time.time + goalLockoutSeconds;

        var result = scoreKeeper.RegisterGoal(goalSide);
        serveManager?.SetNextAttacker(goalSide);
        HandleGoalResult(result);
    }

    public void RestartMatch()
    {
        scoreKeeper?.ResetScores();
        serveManager?.ResetMatch();
        nextGoalAllowedTime = Time.time + goalLockoutSeconds;

        if (matchView != null)
        {
            matchView.SetScores(LeftScore, RightScore);
        }

        PrepareNextTurn(string.Empty);
    }

    private void PrepareNextTurn(string stateMessage)
    {
        turnFlow?.PrepareTurn(() => ResetRoundAndStatus(stateMessage));
    }

    private void ResetRoundAndStatus(string stateMessage)
    {
        roundResetter?.ResetRound();
        matchView?.SetGoalInfoText(stateMessage);
    }

    private void HandleGoalResult(GoalResult result)
    {
        turnFlow?.EndTurn();
        matchView?.SetScores(result.LeftScore, result.RightScore);
        matchView?.PlayGoalInfo(GetGoalInfoMessage(result));

        if (!result.HasWinner)
        {
            turnFlow?.PrepareTurnAfterGoalDelay(() => ResetRoundAndStatus(string.Empty));
        }
    }

    private static string GetGoalInfoMessage(GoalResult result)
    {
        return result.HasWinner
            ? $"{result.ScoringSide} wins"
            : $"Goal! {result.ScoringSide} scores";
    }

    private void ValidateReferences()
    {
        if (matchView == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a MatchUIView reference.", this);
        }

        if (scoreKeeper == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a ScoreKeeper reference.", this);
        }

        if (serveManager == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a ServeManager reference.", this);
        }

        if (roundResetter == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a RoundResetter reference.", this);
        }

        if (turnFlow == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a TurnFlowController reference.", this);
        }
    }
}
