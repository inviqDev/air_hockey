using UnityEngine;

public readonly struct MovementCommand
{
    public MovementCommand(Vector2 move, bool dashPressed)
    {
        Move = move;
        DashPressed = dashPressed;
    }

    public Vector2 Move { get; }
    public bool DashPressed { get; }
}