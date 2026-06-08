using UnityEngine;

[RequireComponent(typeof(SideOwner))]
[RequireComponent(typeof(MovementMotor2D))]
public abstract class StrikerBase : MonoBehaviour
{
    [SerializeField] private SideOwner sideOwner;
    [SerializeField] private MovementMotor2D movementMotor;

    private TurnController turnController;

    private void Reset()
    {
        if (!sideOwner)
            sideOwner = GetComponent<SideOwner>();

        if (!movementMotor)
            movementMotor = GetComponent<MovementMotor2D>();
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

        ConfigureTurnController(controller);

        ApplyStrikerSetup(setupContext);
    }

    public void ResetState(Vector2 position)
    {
        if (!CacheReferences()) return;

        movementMotor.ResetMovementState(position);

        ResetCustomStrikerState();
    }

    protected virtual void ResetCustomStrikerState()
    {
    }

    protected abstract void ApplyStrikerSetup(StrikerSetupContext setupContext);

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

        if (!movementMotor && !TryGetComponent(out movementMotor))
        {
            Debug.LogError($"{nameof(StrikerBase)} on {name} requires a {nameof(MovementMotor2D)} component.", this);
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
        if (!movementMotor) return;

        var isMovementAllowed = turnController && turnController.IsTurnActive;
        movementMotor.SetMovementAllowed(isMovementAllowed);
    }

    private void HandleTurnStarted()
    {
        if (movementMotor)
            movementMotor.SetMovementAllowed(true);
    }

    private void HandleTurnEnded()
    {
        if (movementMotor)
            movementMotor.SetMovementAllowed(false);
    }
}
