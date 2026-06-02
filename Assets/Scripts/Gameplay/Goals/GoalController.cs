using System;
using UnityEngine;

public sealed class GoalController : MonoBehaviour
{
    [SerializeField] private ScoreKeeper scoreKeeper;
    [SerializeField] private ServeManager serveManager;
    [SerializeField] private PuckRegistry puckRegistry;
    [SerializeField] private float goalLockoutSeconds = 0.25f;

    private float nextGoalAllowedTime;

    public event Action<GoalResult> GoalResolved;

    public int LeftScore => scoreKeeper ? scoreKeeper.LeftScore : 0;
    public int RightScore => scoreKeeper ? scoreKeeper.RightScore : 0;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public bool TryHandleGoal(PlayerSide goalSide, Rigidbody2D candidate)
    {
        if (!scoreKeeper || !serveManager) return false;
        if (!puckRegistry || !puckRegistry.IsPuck(candidate)) return false;
        if (Time.time < nextGoalAllowedTime) return false;

        nextGoalAllowedTime = Time.time + goalLockoutSeconds;

        var result = scoreKeeper.RegisterGoal(goalSide);
        serveManager?.SetNextAttacker(goalSide);
        GoalResolved?.Invoke(result);

        return true;
    }

    public void ResetMatch()
    {
        scoreKeeper?.ResetScores();
        serveManager?.ResetMatch();
        nextGoalAllowedTime = Time.time + goalLockoutSeconds;
    }

    private void ValidateReferences()
    {
        if (!scoreKeeper)
            Debug.LogError($"{nameof(GoalController)} requires a ScoreKeeper reference.", this);

        if (!serveManager)
            Debug.LogError($"{nameof(GoalController)} requires a ServeManager reference.", this);

        if (!puckRegistry)
            Debug.LogError($"{nameof(GoalController)} requires a PuckRegistry reference.", this);
    }
}
