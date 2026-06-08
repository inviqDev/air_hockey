using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SideOwner))]
[RequireComponent(typeof(DashAbility))]
[RequireComponent(typeof(MovementMotor2D))]
public abstract class StrikerBase : MonoBehaviour
{
    [SerializeField] private SideOwner sideOwner;
    [SerializeField] private DashAbility dashAbility;
    [SerializeField] private MovementMotor2D movementMotor;

    private Rigidbody2D strikerRigidbody;
    private TurnController turnController;

    private void Reset()
    {
        if (!strikerRigidbody)
            strikerRigidbody = GetComponent<Rigidbody2D>();
        
        if (!sideOwner)
            sideOwner = GetComponent<SideOwner>();

        if (!dashAbility)
            dashAbility = GetComponent<DashAbility>();

        if (!movementMotor)
            movementMotor = GetComponent<MovementMotor2D>();
    }

    private void Awake()
    {
        CacheReferences();
    }

    public void Initialize(StrikerSetupContext setupContext, TurnController controller)
    {
        CacheReferences();

        if (sideOwner)
            sideOwner.Side = setupContext.Side;

        ConfigureTurnController(controller);

        ApplyStrikerSetup(setupContext);
    }

    public void ResetState(Vector2 position)
    {
        CacheReferences();

        if (!strikerRigidbody) return;

#if UNITY_6000_0_OR_NEWER
        strikerRigidbody.linearVelocity = Vector2.zero;
#else
        strikerRigidbody.velocity = Vector2.zero;
#endif

        strikerRigidbody.angularVelocity = 0f;
        strikerRigidbody.position = position;
        strikerRigidbody.rotation = 0f;

        if (dashAbility)
            dashAbility.ResetState();

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

    private void CacheReferences()
    {
        if (!sideOwner)
            sideOwner = GetComponent<SideOwner>();

        if (!dashAbility)
            dashAbility = GetComponent<DashAbility>();

        if (!movementMotor)
            movementMotor = GetComponent<MovementMotor2D>();
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
