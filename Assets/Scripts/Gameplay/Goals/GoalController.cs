using System;
using UnityEngine;

public sealed class GoalController : MonoBehaviour
{
    [SerializeField] private ScoreKeeper scoreKeeper;
    [SerializeField] private ServeManager serveManager;
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

    public bool TryHandleGoal(PlayerSide goalSide)
    {
        if (!scoreKeeper || !serveManager) return false;
        if (Time.time < nextGoalAllowedTime) return false;

        StartGoalLockoutPeriod();

        var result = scoreKeeper.RegisterGoal(goalSide);
        serveManager?.SetNextAttacker(goalSide);
        GoalResolved?.Invoke(result);

        return true;
    }

    public void StartGoalLockoutPeriod()
    {
        nextGoalAllowedTime = Time.time + goalLockoutSeconds;
    }

    private void ValidateReferences()
    {
        if (!scoreKeeper)
            Debug.LogError($"{nameof(GoalController)} requires a ScoreKeeper reference.", this);

        if (!serveManager)
            Debug.LogError($"{nameof(GoalController)} requires a ServeManager reference.", this);
    }
}
