using UnityEngine;

[RequireComponent(typeof(SideOwner))]
[RequireComponent(typeof(StrikerTweenAnimator))]
public abstract class StrikerBase : MonoBehaviour, IPoolable
{
    [Header("References")]
    [SerializeField] private SideOwner sideOwner;
    [SerializeField] private StrikerMovement strikerMovement;
    [SerializeField] private StrikerTweenAnimator strikerTweenAnimator;

    protected StrikerMovement Movement => strikerMovement;
    public AbilityController AbilityController => strikerMovement ? strikerMovement.AbilityController : null;

    private TurnController currentTurnController;

    private void Reset()
    {
        if (!sideOwner)
            sideOwner = GetComponent<SideOwner>();

        if (!strikerMovement)
            strikerMovement = GetComponent<StrikerMovement>();

        if (!strikerTweenAnimator)
            strikerTweenAnimator = GetComponent<StrikerTweenAnimator>();
    }

    private void Awake()
    {
        TryCacheReferences();
    }

    public void InitializeStriker(StrikerSetupContext setupContext, TurnController controller)
    {
        if (!TryCacheReferences()) return;

        if (sideOwner)
            sideOwner.Side = setupContext.Side;

        ApplySetup(setupContext);
        if (!TryInitializeMovement(setupContext)) return;

        ConfigureTurnControllerSubscription(controller);
    }

    public void ResetState(Vector2 position)
    {
        if (!TryCacheReferences()) return;

        strikerMovement.ResetMovementState(position);
        ResetCustomState();

        if (strikerTweenAnimator)
            strikerTweenAnimator.PlayAppearAnimation();
    }

    protected virtual void ResetCustomState()
    {
    }

    protected abstract bool TryInitializeMovement(StrikerSetupContext setupContext);
    protected abstract void ApplySetup(StrikerSetupContext setupContext);

    public void OnGetFromPool()
    {
        TryCacheReferences();

        if (strikerTweenAnimator)
            strikerTweenAnimator.PrepareAppearAnimation();
    }

    public void OnMoveToPool()
    {
        if (!TryCacheReferences()) return;

        if (strikerTweenAnimator)
            strikerTweenAnimator.ResetStrikerVisualState();

        SetMovementAllowed(false);
        UnsubscribeFromTurnController();
        currentTurnController = null;
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

        if (!strikerTweenAnimator && !TryGetComponent(out strikerTweenAnimator))
        {
            Debug.LogError($"{nameof(StrikerBase)} on {name} requires a {nameof(StrikerTweenAnimator)} component.", this);
            hasAllReferences = false;
        }

        return hasAllReferences;
    }

    private void ConfigureTurnControllerSubscription(TurnController newTurnController)
    {
        if (currentTurnController == newTurnController)
        {
            ApplyCurrentTurnState();
            return;
        }

        UnsubscribeFromTurnController();

        currentTurnController = newTurnController;
        SubscribeToTurnController();
        ApplyCurrentTurnState();
    }

    private void SubscribeToTurnController()
    {
        if (!currentTurnController) return;

        currentTurnController.TurnStarted += HandleTurnStarted;
        currentTurnController.TurnEnded += HandleTurnEnded;
    }

    private void UnsubscribeFromTurnController()
    {
        if (!currentTurnController) return;

        currentTurnController.TurnStarted -= HandleTurnStarted;
        currentTurnController.TurnEnded -= HandleTurnEnded;
    }

    private void ApplyCurrentTurnState()
    {
        var isMovementAllowed = currentTurnController && currentTurnController.IsTurnActive;
        SetMovementAllowed(isMovementAllowed);
    }

    private void SetMovementAllowed(bool isAllowed)
    {
        if (!strikerMovement) return;

        strikerMovement.SetMovementAllowed(isAllowed);
    }

    private void HandleTurnStarted()
    {
        SetMovementAllowed(true);
    }

    private void HandleTurnEnded()
    {
        SetMovementAllowed(false);
    }
}
