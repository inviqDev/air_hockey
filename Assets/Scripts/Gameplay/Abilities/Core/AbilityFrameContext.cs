using UnityEngine;

public readonly struct AbilityFrameContext
{
    public AbilityFrameContext(
        AbilityContext ownerContext,
        Vector2 moveInput,
        float deltaTime,
        AbilityActivationTrigger activationTriggers = AbilityActivationTrigger.None)
    {
        OwnerContext = ownerContext;
        MoveInput = moveInput;
        DeltaTime = deltaTime;
        ActivationTriggers = activationTriggers;
    }

    public AbilityContext OwnerContext { get; }
    public Vector2 MoveInput { get; }
    public float DeltaTime { get; }
    public AbilityActivationTrigger ActivationTriggers { get; }
}
