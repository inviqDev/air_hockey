using System;
using System.Collections;
using UnityEngine;

public sealed class TurnController : MonoBehaviour
{
    [SerializeField] private TurnStartView turnStartView;
    [SerializeField] private TurnTimer turnTimer;
    [SerializeField] private float goalDelayBeforeNextTurnSeconds = 3f;
    
    private Coroutine goalDelayRoutine;

    public bool IsTurnActive { get; private set; }
    public event Action TurnStarted;
    public event Action TurnEnded;
    public event Action RespawnItemsRequested;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnEnable()
    {
        if (turnStartView)
        {
            turnStartView.CountdownCompleted += StartTurn;
            turnStartView.RespawnItemsRequested += HandleRespawnItemsRequested;
        }
    }

    private void OnDisable()
    {
        if (turnStartView)
        {
            turnStartView.CountdownCompleted -= StartTurn;
            turnStartView.RespawnItemsRequested -= HandleRespawnItemsRequested;
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void PrepareTurn(Func<bool> beforeShowTurnPreparation)
    {
        StopGoalDelayRoutine();
        EndTurn();
        var canStartTurn = beforeShowTurnPreparation == null || beforeShowTurnPreparation();

        if (turnStartView)
            turnStartView.ShowTurnPreparation(canStartTurn);
    }

    public void PrepareTurnAfterGoalDelay(Func<bool> beforeShowTurnPreparation)
    {
        StopGoalDelayRoutine();
        EndTurn();
        goalDelayRoutine = StartCoroutine(PrepareTurnAfterGoalDelayRoutine(beforeShowTurnPreparation));
    }

    public void EndTurn()
    {
        var wasTurnActive = IsTurnActive;
        IsTurnActive = false;

        if (wasTurnActive)
            TurnEnded?.Invoke();

        if (turnStartView)
            turnStartView.Cancel();

        if (turnTimer)
            turnTimer.StopAndReset();
    }

    public void ShowTurnPreparation(bool canStartTurn)
    {
        StopGoalDelayRoutine();
        EndTurn();

        if (turnStartView)
            turnStartView.ShowTurnPreparation(canStartTurn);
    }

    public void RefreshTurnPreparation(Func<bool> beforeShowTurnPreparation)
    {
        if (IsTurnActive) return;

        var canStartTurn = beforeShowTurnPreparation == null || beforeShowTurnPreparation();

        if (turnStartView)
            turnStartView.ShowTurnPreparation(canStartTurn);
    }

    private IEnumerator PrepareTurnAfterGoalDelayRoutine(Func<bool> beforeShowTurnPreparation)
    {
        var delaySeconds = Mathf.Max(0f, goalDelayBeforeNextTurnSeconds);
        yield return new WaitForSeconds(delaySeconds);

        goalDelayRoutine = null;
        var canStartTurn = beforeShowTurnPreparation == null || beforeShowTurnPreparation();

        if (turnStartView)
            turnStartView.ShowTurnPreparation(canStartTurn);
    }

    private void StartTurn()
    {
        if (IsTurnActive) return;

        IsTurnActive = true;
        TurnStarted?.Invoke();

        if (turnTimer)
            turnTimer.StartTimer();
    }

    private void StopGoalDelayRoutine()
    {
        if (goalDelayRoutine == null) return;

        StopCoroutine(goalDelayRoutine);
        goalDelayRoutine = null;
    }

    private void HandleRespawnItemsRequested()
    {
        RespawnItemsRequested?.Invoke();
    }

    private void ValidateReferences()
    {
        if (!turnStartView)
            Debug.LogError($"{nameof(TurnController)} requires a TurnStartView reference.", this);

        if (!turnTimer)
            Debug.LogError($"{nameof(TurnController)} requires a TurnTimer reference.", this);
    }
}
