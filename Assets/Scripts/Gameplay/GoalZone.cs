using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class GoalZone : MonoBehaviour
{
    [SerializeField] private PlayerSide goalSide;
    [SerializeField] private MatchManager matchManager;
    
    private Collider2D _collider;

    public PlayerSide GoalSide
    {
        get => goalSide;
        set => goalSide = value;
    }

    private void Reset()
    {
        _collider ??= GetComponent<Collider2D>();
        _collider.isTrigger = true;
    }

    private void Awake()
    {
        _collider ??= GetComponent<Collider2D>();
        _collider.isTrigger = true;
        
        ValidateReferences();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!matchManager || !matchManager.IsPuck(other.attachedRigidbody)) return;

        matchManager.HandleGoal(goalSide);
    }

    private void ValidateReferences()
    {
        if (!matchManager)
        {
            Debug.LogError($"{nameof(GoalZone)} on {name} requires a MatchManager reference.", this);
        }
    }
}
