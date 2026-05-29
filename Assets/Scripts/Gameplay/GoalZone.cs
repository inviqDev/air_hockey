using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class GoalZone : MonoBehaviour
{
    [SerializeField] private PlayerSide goalSide;
    [SerializeField] private MatchManager matchManager;

    public PlayerSide GoalSide
    {
        get => goalSide;
        set => goalSide = value;
    }

    private void Reset()
    {
        var zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    private void Awake()
    {
        var zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
        ValidateReferences();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (matchManager == null || !matchManager.IsPuck(other.attachedRigidbody))
        {
            return;
        }

        matchManager.HandleGoal(goalSide);
    }

    private void ValidateReferences()
    {
        if (matchManager == null)
        {
            Debug.LogError($"{nameof(GoalZone)} on {name} requires a MatchManager reference.", this);
        }
    }
}
