using System;
using UnityEngine;

public sealed class TurnController : MonoBehaviour
{
    [SerializeField] private TurnStartView turnStartView;

    public bool IsTurnActive { get; private set; }
    public event Action CountdownCompleted;
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
            turnStartView.CountdownCompleted += HandleCountdownCompleted;
            turnStartView.RespawnItemsRequested += HandleRespawnItemsRequested;
        }
    }

    private void OnDisable()
    {
        if (turnStartView)
        {
            turnStartView.CountdownCompleted -= HandleCountdownCompleted;
            turnStartView.RespawnItemsRequested -= HandleRespawnItemsRequested;
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void PrepareTurn(Func<bool> beforeShowTurnPreparation)
    {
        EndTurn();
        var canStartTurn = beforeShowTurnPreparation == null || beforeShowTurnPreparation();

        if (turnStartView)
            turnStartView.ShowTurnPreparation(canStartTurn);
    }

    public void EndTurn()
    {
        var wasTurnActive = IsTurnActive;
        IsTurnActive = false;

        if (wasTurnActive)
            TurnEnded?.Invoke();

        if (turnStartView)
            turnStartView.Cancel();
    }

    public void ShowTurnPreparation(bool canStartTurn)
    {
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

    public void ActivatePreparedTurn()
    {
        if (IsTurnActive) return;

        IsTurnActive = true;
        TurnStarted?.Invoke();
    }

    private void HandleCountdownCompleted()
    {
        CountdownCompleted?.Invoke();
    }

    private void HandleRespawnItemsRequested()
    {
        RespawnItemsRequested?.Invoke();
    }

    private void ValidateReferences()
    {
        if (!turnStartView)
            Debug.LogError($"{nameof(TurnController)} requires a TurnStartView reference.", this);
    }
}
