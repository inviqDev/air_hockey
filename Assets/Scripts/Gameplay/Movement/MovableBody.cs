using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class MovableBody : MonoBehaviour, IMovable
{
    private Rigidbody2D body;
    private bool isInitialized;
    private bool isMovementAllowed;

    public bool IsMovementAllowed => isMovementAllowed;
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

    protected bool IsInitialized => isInitialized;
    protected Rigidbody2D Body => body;

    protected virtual void Reset()
    {
        if (!body)
            body = GetComponent<Rigidbody2D>();

        if (body)
            ConfigureBody(body);
    }

    public void SetMovementAllowed(bool isAllowed)
    {
        isMovementAllowed = isAllowed;

        if (!isMovementAllowed)
        {
            HandleMovementStopped();
            StopMovement();
        }

        UpdateMovementLoopState();
    }

    public void ResetMovementState(Vector2 position)
    {
        if (!isInitialized) return;
        if (!body) return;

        SetVelocity(Vector2.zero);
        body.angularVelocity = 0f;
        body.position = position;
        body.rotation = 0f;

        HandleMovementReset();
        UpdateMovementLoopState();
    }

    public void StopMovement()
    {
        SetVelocity(Vector2.zero);
    }

    protected bool InitializeMovableBody()
    {
        if (isInitialized)
            return true;

        if (!CacheReferences()) return false;

        ConfigureBody(body);
        isInitialized = true;
        return true;
    }

    protected void ApplyVelocity(Vector2 velocity)
    {
        SetVelocity(velocity);
    }

    protected abstract void UpdateMovementLoopState();

    protected virtual void HandleMovementStopped()
    {
    }

    protected virtual void HandleMovementReset()
    {
    }

    protected virtual bool CacheAdditionalReferences()
    {
        return true;
    }

    protected virtual void ConfigureBody(Rigidbody2D targetBody)
    {
        targetBody.bodyType = RigidbodyType2D.Dynamic;
        targetBody.gravityScale = 0f;
        targetBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        targetBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        targetBody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnDisable()
    {
        StopMovement();
    }

    private bool CacheReferences()
    {
        var hasAllReferences = true;

        if (!body && !TryGetComponent(out body))
        {
            Debug.LogError($"{nameof(MovableBody)} on {name} requires a {nameof(Rigidbody2D)} component.", this);
            hasAllReferences = false;
        }

        if (!CacheAdditionalReferences())
            hasAllReferences = false;

        return hasAllReferences;
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
