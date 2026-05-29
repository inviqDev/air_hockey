using System.Collections;
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
    [SerializeField] private float puckStartDistanceFromAttacker = 1.25f;

    [Header("Match")]
    [SerializeField] private int winningScore = 7;
    [SerializeField] private float goalLockoutSeconds = 0.25f;
    [SerializeField] private int turnCountdownSeconds = 3;

    private float nextGoalAllowedTime;
    private Coroutine turnCountdownRoutine;
    private bool hasChosenFirstAttacker;
    private PlayerSide nextAttackingSide;

    public static MatchManager Instance { get; private set; }
    public int LeftScore { get; private set; }
    public int RightScore { get; private set; }
    public bool IsTurnActive { get; private set; }

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
        }

        PrepareNextTurn(string.Empty);
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

        nextAttackingSide = goalSide;
        hasChosenFirstAttacker = true;

        UpdateViewAfterGoal(scoringSide);
    }

    public void RestartMatch()
    {
        LeftScore = 0;
        RightScore = 0;
        hasChosenFirstAttacker = false;
        nextGoalAllowedTime = Time.time + goalLockoutSeconds;

        if (matchView != null)
        {
            matchView.SetScores(LeftScore, RightScore);
        }

        PrepareNextTurn(string.Empty);
    }

    public void BeginTurnCountdown()
    {
        if (IsTurnActive || turnCountdownRoutine != null)
        {
            return;
        }

        turnCountdownRoutine = StartCoroutine(TurnCountdownRoutine());
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
        ResetBody(leftStriker, leftStrikerStartPosition);
        ResetBody(rightStriker, rightStrikerStartPosition);

        ResetBody(puck, GetPuckStartPosition(GetNextAttackingSide()));
    }

    private void PrepareNextTurn(string stateMessage)
    {
        if (turnCountdownRoutine != null)
        {
            StopCoroutine(turnCountdownRoutine);
            turnCountdownRoutine = null;
        }

        IsTurnActive = false;
        ResetRound();

        if (matchView != null)
        {
            matchView.SetStateText(stateMessage);
            matchView.ShowReadyButton(true);
        }
    }

    private IEnumerator TurnCountdownRoutine()
    {
        if (matchView != null)
        {
            matchView.ShowReadyButton(false);
        }

        var countdownSeconds = Mathf.Max(1, turnCountdownSeconds);

        for (var secondsRemaining = countdownSeconds; secondsRemaining > 0; secondsRemaining--)
        {
            if (matchView != null)
            {
                matchView.SetStateText(secondsRemaining.ToString());
            }

            yield return new WaitForSeconds(1f);
        }

        IsTurnActive = true;
        turnCountdownRoutine = null;

        if (matchView != null)
        {
            matchView.SetStateText(string.Empty);
        }
    }

    private PlayerSide GetNextAttackingSide()
    {
        if (!hasChosenFirstAttacker)
        {
            nextAttackingSide = Random.value < 0.5f ? PlayerSide.Left : PlayerSide.Right;
            hasChosenFirstAttacker = true;
        }

        return nextAttackingSide;
    }

    private Vector2 GetPuckStartPosition(PlayerSide attackingSide)
    {
        var attackerPosition = attackingSide == PlayerSide.Left
            ? leftStrikerStartPosition
            : rightStrikerStartPosition;
        var distanceToCenter = Vector2.Distance(attackerPosition, puckStartPosition);
        var offsetFromAttacker = Mathf.Min(puckStartDistanceFromAttacker, distanceToCenter * 0.5f);

        return Vector2.MoveTowards(attackerPosition, puckStartPosition, offsetFromAttacker);
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
        matchView?.SetScores(LeftScore, RightScore);

        if (winningScore > 0 && (LeftScore >= winningScore || RightScore >= winningScore))
        {
            IsTurnActive = false;
            matchView?.ShowReadyButton(false);
            matchView?.SetStateText($"{scoringSide} wins");
            return;
        }

        PrepareNextTurn($"Goal! {scoringSide} scores");
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
