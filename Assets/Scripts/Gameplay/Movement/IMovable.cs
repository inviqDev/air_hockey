using UnityEngine;

public interface IMovable
{
    bool IsMovementAllowed { get; }
    Vector2 Position { get; }
    Vector2 Velocity { get; }

    void SetMovementAllowed(bool isAllowed);
    void ResetMovementState(Vector2 position);
    void StopMovement();
}
