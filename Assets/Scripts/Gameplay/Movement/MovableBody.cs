using UnityEngine;

public sealed class MovableBody : IMovable
{
    private readonly Rigidbody2D body;

    public MovableBody(Rigidbody2D body)
    {
        this.body = body;
        ConfigureRigidbody();
    }

    public bool IsMovementAllowed { get; private set; }

    public Vector2 Position => body ? body.position : Vector2.zero;

    public Vector2 Velocity
    {
        get
        {
            if (!body) return Vector2.zero;

#if UNITY_6000_0_OR_NEWER
            return body.linearVelocity;
#else
            return body.velocity;
#endif
        }
    }

    public void SetMovementAllowed(bool isAllowed)
    {
        IsMovementAllowed = isAllowed;

        if (!IsMovementAllowed)
            StopMovement();
    }

    public void ResetMovementState(Vector2 position)
    {
        if (!body) return;

        SetVelocity(Vector2.zero);
        body.angularVelocity = 0f;
        body.position = position;
        body.rotation = 0f;
    }

    public void StopMovement()
    {
        SetVelocity(Vector2.zero);
    }

    public void ApplyVelocity(Vector2 velocity)
    {
        SetVelocity(velocity);
    }

    private void ConfigureRigidbody()
    {
        if (!body) return;

        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void SetVelocity(Vector2 velocity)
    {
        if (!body) return;

#if UNITY_6000_0_OR_NEWER
        body.linearVelocity = velocity;
#else
        body.velocity = velocity;
#endif
    }
}
