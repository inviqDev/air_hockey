using System;
using System.Collections;
using UnityEngine;

public sealed class TurnFlowController : MonoBehaviour
{
    [SerializeField] private TurnStartCountdown turnStartCountdown;
    [SerializeField] private TurnTimer turnTimer;
    [SerializeField] private float goalDelayBeforeNextTurnSeconds = 3f;

    private Coroutine goalDelayRoutine;

    public static TurnFlowController Instance { get; private set; }
    public bool IsTurnActive { get; private set; }

    private void Awake()
    {
        Instance = this;
        ValidateReferences();
    }

    private void OnEnable()
    {
        if (turnStartCountdown != null)
        {
            turnStartCountdown.CountdownCompleted += StartTurn;
        }
    }

    private void OnDisable()
    {
        if (turnStartCountdown != null)
        {
            turnStartCountdown.CountdownCompleted -= StartTurn;
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void PrepareTurn(Action beforeStartButton)
    {
        StopGoalDelayRoutine();
        EndTurn();
        beforeStartButton?.Invoke();
        turnStartCountdown?.ShowStartButton();
    }

    public void PrepareTurnAfterGoalDelay(Action beforeStartButton)
    {
        StopGoalDelayRoutine();
        EndTurn();
        goalDelayRoutine = StartCoroutine(PrepareTurnAfterGoalDelayRoutine(beforeStartButton));
    }

    public void EndTurn()
    {
        IsTurnActive = false;
        turnStartCountdown?.Cancel();
        turnTimer?.StopAndReset();
    }

    private IEnumerator PrepareTurnAfterGoalDelayRoutine(Action beforeStartButton)
    {
        var delaySeconds = Mathf.Max(0f, goalDelayBeforeNextTurnSeconds);
        yield return new WaitForSeconds(delaySeconds);

        goalDelayRoutine = null;
        beforeStartButton?.Invoke();
        turnStartCountdown?.ShowStartButton();
    }

    private void StartTurn()
    {
        if (IsTurnActive)
        {
            return;
        }

        IsTurnActive = true;
        turnTimer?.StartTimer();
    }

    private void StopGoalDelayRoutine()
    {
        if (goalDelayRoutine == null)
        {
            return;
        }

        StopCoroutine(goalDelayRoutine);
        goalDelayRoutine = null;
    }

    private void ValidateReferences()
    {
        if (turnStartCountdown == null)
        {
            Debug.LogError($"{nameof(TurnFlowController)} requires a TurnStartCountdown reference.", this);
        }

        if (turnTimer == null)
        {
            Debug.LogError($"{nameof(TurnFlowController)} requires a TurnTimer reference.", this);
        }
    }
}
