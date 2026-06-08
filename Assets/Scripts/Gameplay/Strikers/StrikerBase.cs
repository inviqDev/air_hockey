using UnityEngine;

[RequireComponent(typeof(SideOwner))]
[RequireComponent(typeof(StrikerMovement))]
public abstract class StrikerBase : MonoBehaviour
{
    [SerializeField] private SideOwner sideOwner;
    [SerializeField] private StrikerMovement strikerMovement;

    private TurnController turnController;

    private void Reset()
    {
        if (!sideOwner)
            sideOwner = GetComponent<SideOwner>();

        if (!strikerMovement)
            strikerMovement = GetComponent<StrikerMovement>();
    }

    private void Awake()
    {
        CacheReferences();
    }

    public void Initialize(StrikerSetupContext setupContext, TurnController controller)
    {
        if (!CacheReferences()) return;

        if (sideOwner)
            sideOwner.Side = setupContext.Side;

        ApplyStrikerSetup(setupContext);

        var movementCommandSource = GetMovementCommandSource();
        if (movementCommandSource == null)
        {
            Debug.LogError($"{nameof(StrikerBase)} on {name} could not resolve a movement command source.", this);
            return;
        }

        if (!strikerMovement.Initialize(movementCommandSource))
            return;

        ConfigureTurnController(controller);
    }

    public void ResetState(Vector2 position)
    {
        if (!CacheReferences()) return;

        strikerMovement.ResetMovementState(position);

        ResetCustomStrikerState();
    }

    protected virtual void ResetCustomStrikerState()
    {
    }

    protected abstract void ApplyStrikerSetup(StrikerSetupContext setupContext);
    protected abstract IMovementCommandSource GetMovementCommandSource();

    private void OnDestroy()
    {
        UnsubscribeFromTurnController();
    }

    private bool CacheReferences()
    {
        var hasAllReferences = true;

        if (!sideOwner && !TryGetComponent(out sideOwner))
        {
            Debug.LogError($"{nameof(StrikerBase)} on {name} requires a {nameof(SideOwner)} component.", this);
            hasAllReferences = false;
        }

        if (!strikerMovement && !TryGetComponent(out strikerMovement))
        {
            Debug.LogError($"{nameof(StrikerBase)} on {name} requires a {nameof(StrikerMovement)} component.", this);
            hasAllReferences = false;
        }

        return hasAllReferences;
    }

    private void ConfigureTurnController(TurnController newTurnController)
    {
        if (turnController == newTurnController)
        {
            ApplyCurrentTurnState();
            return;
        }

        UnsubscribeFromTurnController();

        turnController = newTurnController;
        SubscribeToTurnController();

        ApplyCurrentTurnState();
    }

    private void SubscribeToTurnController()
    {
        if (!turnController) return;

        turnController.TurnStarted += HandleTurnStarted;
        turnController.TurnEnded += HandleTurnEnded;
    }

    private void UnsubscribeFromTurnController()
    {
        if (!turnController) return;

        turnController.TurnStarted -= HandleTurnStarted;
        turnController.TurnEnded -= HandleTurnEnded;
    }

    private void ApplyCurrentTurnState()
    {
        if (!strikerMovement) return;

        var isMovementAllowed = turnController && turnController.IsTurnActive;
        strikerMovement.SetMovementAllowed(isMovementAllowed);
    }

    private void HandleTurnStarted()
    {
        if (strikerMovement)
            strikerMovement.SetMovementAllowed(true);
    }

    private void HandleTurnEnded()
    {
        if (strikerMovement)
            strikerMovement.SetMovementAllowed(false);
    }
}
