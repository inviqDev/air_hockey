using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class GoalZone : MonoBehaviour
{
    [SerializeField] private PlayerSide goalSide;
    [SerializeField] private GoalController goalController;
    [SerializeField] private PuckRegistry puckRegistry;

    private Collider2D goalCollider;

    private void Reset()
    {
        goalCollider ??= GetComponent<Collider2D>();
        goalCollider.isTrigger = true;
    }

    private void Awake()
    {
        goalCollider ??= GetComponent<Collider2D>();
        goalCollider.isTrigger = true;
        
        ValidateReferences();
    }

    private void OnValidate()
    {
        goalCollider ??= GetComponent<Collider2D>();
        goalCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!goalController || !puckRegistry) return;

        var currentPuck = puckRegistry.CurrentPuck;
        if (!currentPuck) return;

        var attachedBody = other.attachedRigidbody;
        if (attachedBody != currentPuck.PuckRigidbody) return;

        goalController.TryHandleGoal(goalSide);
    }

    private void ValidateReferences()
    {
        if (!goalController)
            Debug.LogError($"{nameof(GoalZone)} on {name} requires a GoalController reference.", this);

        if (!puckRegistry)
            Debug.LogError($"{nameof(GoalZone)} on {name} requires a PuckRegistry reference.", this);
    }
}
