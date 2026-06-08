using UnityEngine;

[RequireComponent(typeof(SideOwner))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class StrikerBase : MonoBehaviour
{
    [Header("Striker colliders")]
    [SerializeField] private BoxCollider2D strikerBoundsCollider;
    [SerializeField] private Collider2D strikerCollisionCollider;

    [Header("References")]
    [SerializeField] private SideOwner sideOwner;
    [SerializeField] private StrikerMovement strikerMovement;
    
    protected StrikerMovement Movement => strikerMovement;
    protected BoxCollider2D BoundsCollider => strikerBoundsCollider;
    
    private TurnController turnController;

    private void Reset()
    {
        if (!sideOwner)
            sideOwner = GetComponent<SideOwner>();

        if (!strikerMovement)
            strikerMovement = GetComponent<StrikerMovement>();

        if (!strikerBoundsCollider)
            strikerBoundsCollider = GetComponent<BoxCollider2D>();

        if (!strikerCollisionCollider)
            strikerCollisionCollider = GetCollisionCollider();
    }

    private void Awake()
    {
        CacheReferences();
    }

    public void InitializeStriker(StrikerSetupContext setupContext, TurnController controller)
    {
        if (!CacheReferences()) return;

        if (sideOwner)
            sideOwner.Side = setupContext.Side;

        ApplyStrikerSetup(setupContext);
        if (!InitializeStrikerMovement()) return;

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

    protected abstract bool InitializeStrikerMovement();
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

        if (!strikerMovement && !TryGetComponent(out strikerMovement))
        {
            Debug.LogError($"{nameof(StrikerBase)} on {name} requires a {nameof(StrikerMovement)} component.", this);
            hasAllReferences = false;
        }

        if (!strikerBoundsCollider && !TryGetComponent(out strikerBoundsCollider))
        {
            Debug.LogError($"{nameof(StrikerBase)} on {name} requires a {nameof(BoxCollider2D)} component.", this);
            hasAllReferences = false;
        }

        if (!strikerCollisionCollider)
        {
            strikerCollisionCollider = GetCollisionCollider();

            if (!strikerCollisionCollider)
            {
                Debug.LogError($"{nameof(StrikerBase)} on {name} requires a collision {nameof(Collider2D)} component.", this);
                hasAllReferences = false;
            }
        }

        return hasAllReferences;
    }

    private Collider2D GetCollisionCollider()
    {
        foreach (var col in GetComponents<Collider2D>())
        {
            if (col == strikerBoundsCollider) continue;
            return col;
        }

        return null;
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
