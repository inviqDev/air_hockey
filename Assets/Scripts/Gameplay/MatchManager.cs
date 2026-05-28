using UnityEngine;

public sealed class MatchManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D puck;
    [SerializeField] private Rigidbody2D leftStriker;
    [SerializeField] private Rigidbody2D rightStriker;
    [SerializeField] private GoalZone leftGoal;
    [SerializeField] private GoalZone rightGoal;
    [SerializeField] private MatchUIView matchView;

    [Header("Start Positions")]
    [SerializeField] private Vector2 puckStartPosition = Vector2.zero;
    [SerializeField] private Vector2 leftStrikerStartPosition = new(-8f, 0f);
    [SerializeField] private Vector2 rightStrikerStartPosition = new(7.29f, 0f);

    [Header("Match")]
    [SerializeField] private int winningScore = 7;
    [SerializeField] private float goalLockoutSeconds = 0.25f;

    private float nextGoalAllowedTime;

    public static MatchManager Instance { get; private set; }
    public int LeftScore { get; private set; }
    public int RightScore { get; private set; }

    private void Awake()
    {
        Instance = this;
        ValidateReferences();
        ConfigureGoals();
    }

    private void Start()
    {
        if (matchView != null)
        {
            matchView.Initialize(this);
            matchView.SetScores(LeftScore, RightScore);
            matchView.SetStateText(string.Empty);
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public bool IsPuck(Rigidbody2D candidate)
    {
        return candidate != null && candidate == puck;
    }

    public void HandleGoal(PlayerSide goalSide)
    {
        if (Time.time < nextGoalAllowedTime)
        {
            return;
        }

        nextGoalAllowedTime = Time.time + goalLockoutSeconds;

        var scoringSide = goalSide == PlayerSide.Left ? PlayerSide.Right : PlayerSide.Left;

        if (scoringSide == PlayerSide.Left)
        {
            LeftScore++;
        }
        else
        {
            RightScore++;
        }

        ResetRound();
        UpdateViewAfterGoal(scoringSide);
    }

    public void RestartMatch()
    {
        LeftScore = 0;
        RightScore = 0;
        nextGoalAllowedTime = Time.time + goalLockoutSeconds;

        ResetRound();

        if (matchView != null)
        {
            matchView.SetScores(LeftScore, RightScore);
            matchView.SetStateText(string.Empty);
        }
    }

    private void ConfigureGoals()
    {
        if (leftGoal != null)
        {
            leftGoal.GoalSide = PlayerSide.Left;
        }

        if (rightGoal != null)
        {
            rightGoal.GoalSide = PlayerSide.Right;
        }
    }

    private void ResetRound()
    {
        ResetBody(puck, puckStartPosition);
        ResetBody(leftStriker, leftStrikerStartPosition);
        ResetBody(rightStriker, rightStrikerStartPosition);
    }

    private void ResetBody(Rigidbody2D body, Vector2 position)
    {
        if (body == null)
        {
            return;
        }

        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.position = position;
        body.rotation = 0f;
    }

    private void UpdateViewAfterGoal(PlayerSide scoringSide)
    {
        if (matchView == null)
        {
            return;
        }

        matchView.SetScores(LeftScore, RightScore);

        if (winningScore > 0 && (LeftScore >= winningScore || RightScore >= winningScore))
        {
            matchView.SetStateText($"{scoringSide} wins");
            return;
        }

        matchView.SetStateText($"Goal! {scoringSide} scores");
    }

    private void ValidateReferences()
    {
        if (puck == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a puck Rigidbody2D reference.", this);
        }

        if (leftStriker == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a left striker Rigidbody2D reference.", this);
        }

        if (rightStriker == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a right striker Rigidbody2D reference.", this);
        }

        if (leftGoal == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a left GoalZone reference.", this);
        }

        if (rightGoal == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a right GoalZone reference.", this);
        }

        if (matchView == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a MatchUIView reference.", this);
        }
    }
}
