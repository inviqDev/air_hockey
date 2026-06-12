using UnityEngine;

public interface IStrikerMovementOverride
{
    Vector2 CurrentMoveDirection { get; }
    PlayerSide Side { get; }
    bool CanUseMovementOverride { get; }

    bool TryBeginMovementOverride();
    void SetMovementOverrideVelocity(Vector2 velocity);
    void EndMovementOverride();
}
