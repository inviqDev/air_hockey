using UnityEngine;

[RequireComponent(typeof(AICommandSource))]
public sealed class AiStrikerMovement : StrikerMovement
{
    [SerializeField] private AICommandSource aiCommandSource;

    private void Reset()
    {
        if (!aiCommandSource)
            aiCommandSource = GetComponent<AICommandSource>();
    }

    public bool InitializeAiStrikerMovement(BoxCollider2D strikerBoundsCollider)
    {
        if (!aiCommandSource)
            aiCommandSource = GetComponent<AICommandSource>();

        if (!aiCommandSource)
        {
            Debug.LogError($"{nameof(AiStrikerMovement)} on {name} requires a {nameof(AICommandSource)} component.", this);
            return false;
        }

        return base.InitializeStrikerMovement(strikerBoundsCollider);
    }

    private void FixedUpdate()
    {
        var command = aiCommandSource.ReadCommand();
        ExecuteMovementStep(command);
        UpdateMovementLoopState();
    }

    protected override void UpdateMovementLoopState()
    {
        enabled = IsInitialized && IsMovementAllowed && aiCommandSource;
    }
}
