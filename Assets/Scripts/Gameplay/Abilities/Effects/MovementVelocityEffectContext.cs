using UnityEngine;

public readonly struct MovementVelocityEffectContext
{
    public MovementVelocityEffectContext(GameObject target, Transform targetTransform, Vector2 moveInput, float deltaTime)
    {
        Target = target;
        TargetTransform = targetTransform;
        MoveInput = moveInput;
        DeltaTime = deltaTime;
    }

    public GameObject Target { get; }
    public Transform TargetTransform { get; }
    public Vector2 MoveInput { get; }
    public float DeltaTime { get; }
}
