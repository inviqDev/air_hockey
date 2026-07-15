using UnityEngine;

[RequireComponent(typeof(SideOwner))]
[RequireComponent(typeof(StrikerTweenAnimator))]
public abstract class StrikerBase : MonoBehaviour, IPoolable
{
    [Header("References")]
    [SerializeField] private SideOwner sideOwner;
    [SerializeField] private StrikerMovement strikerMovement;
    [SerializeField] private StrikerTweenAnimator strikerTweenAnimator;
    [SerializeField] private PlayerAbilityController abilityController;

    protected StrikerMovement Movement => strikerMovement;
    public PlayerAbilityController AbilityController => abilityController;

    private TurnController turnController;
    private IMovable movable;

    private void Reset()
    {
        if (!sideOwner)
            sideOwner = GetComponent<SideOwner>();

        if (!strikerMovement)
            strikerMovement = GetComponent<StrikerMovement>();

        if (!strikerTweenAnimator)
            strikerTweenAnimator = GetComponent<StrikerTweenAnimator>();

        if (!abilityController)
            abilityController = GetComponent<PlayerAbilityController>();
    }

    private void Awake()
    {
        TryCacheReferences();
    }

    public void Initialize(StrikerSetupContext setupContext, TurnController controller)
    {
        if (!TryCacheReferences()) return;

        if (sideOwner)
            sideOwner.Side = setupContext.Side;

        ApplySetup(setupContext);
        if (!TryInitializeMovement()) return;

        ConfigureTurnControllerSubscription(controller);
    }

    public void ResetState(Vector2 position)
    {
        if (!TryCacheReferences()) return;
        if (movable == null) return;

        movable.ResetMovementState(position);
        ResetCustomState();

        if (strikerTweenAnimator)
            strikerTweenAnimator.PlayAppearAnimation();
    }

    protected virtual void ResetCustomState()
    {
    }

    protected virtual void OnBeforeReturnToPool()
    {
    }

    protected abstract bool TryInitializeMovement();
    protected abstract void ApplySetup(StrikerSetupContext setupContext);

    public void OnGetFromPool()
    {
        TryCacheReferences();

        if (strikerTweenAnimator)
            strikerTweenAnimator.PrepareAppearAnimation();
    }

    public void OnMoveToPool()
    {
        OnBeforeReturnToPool();

        if (!TryCacheReferences()) return;

        if (strikerTweenAnimator)
            strikerTweenAnimator.ResetStrikerVisualState();

        SetMovementAllowed(false);
        SetAbilityUsageAllowed(false);
        UnsubscribeFromTurnController();
        turnController = null;
    }

    private void OnDestroy()
    {
        UnsubscribeFromTurnController();
    }

    private bool TryCacheReferences()
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

        movable = strikerMovement;

        if (!strikerTweenAnimator && !TryGetComponent(out strikerTweenAnimator))
        {
            Debug.LogError($"{nameof(StrikerBase)} on {name} requires a {nameof(StrikerTweenAnimator)} component.", this);
            hasAllReferences = false;
        }

        if (!abilityController)
            TryGetComponent(out abilityController);

        return hasAllReferences;
    }

    private void ConfigureTurnControllerSubscription(TurnController newTurnController)
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
        var isGameplayAllowed = turnController && turnController.IsTurnActive;
        SetMovementAllowed(isGameplayAllowed);
        SetAbilityUsageAllowed(isGameplayAllowed);
    }

    private void SetMovementAllowed(bool isAllowed)
    {
        if (!strikerMovement) return;
        if (movable == null) return;

        movable.SetMovementAllowed(isAllowed);
    }

    private void HandleTurnStarted()
    {
        SetMovementAllowed(true);
        SetAbilityUsageAllowed(true);
    }

    private void HandleTurnEnded()
    {
        SetMovementAllowed(false);
        SetAbilityUsageAllowed(false);
    }

    private void SetAbilityUsageAllowed(bool isAllowed)
    {
        if (!abilityController) return;

        abilityController.SetAbilityUsageAllowed(isAllowed);
    }
}
