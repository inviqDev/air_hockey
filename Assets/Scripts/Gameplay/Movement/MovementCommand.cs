using UnityEngine;

public readonly struct MovementCommand
{
    public MovementCommand(Vector2 move, AbilityActivationTrigger activationTriggers)
    {
        Move = move;
        ActivationTriggers = activationTriggers;
    }

    public Vector2 Move { get; }
    public AbilityActivationTrigger ActivationTriggers { get; }
}
