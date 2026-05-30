using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class GoalZone : MonoBehaviour
{
    [SerializeField] private PlayerSide goalSide;
    [SerializeField] private GoalController goalController;
    
    public PlayerSide GoalSide
    {
        get => goalSide;
        set => goalSide = value;
    }

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        ResolveGoalController();
        ValidateReferences();
    }

    private void OnValidate()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        goalController?.TryHandleGoal(goalSide, other.attachedRigidbody);
    }

    private void ValidateReferences()
    {
        if (!goalController)
        {
            Debug.LogError($"{nameof(GoalZone)} on {name} requires a GoalController reference.", this);
        }
    }

    private void ResolveGoalController()
    {
        if (goalController) return;

#if UNITY_2023_1_OR_NEWER
        goalController = FindFirstObjectByType<GoalController>();
#else
        goalController = FindObjectOfType<GoalController>();
#endif
    }
}
