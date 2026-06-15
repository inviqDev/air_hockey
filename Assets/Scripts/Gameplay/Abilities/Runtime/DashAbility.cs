using UnityEngine;

public sealed class DashAbility : AbilityBase, IHasCooldown
{
    public readonly struct Context
    {
        public Context(IStrikerMovementOverride movement)
        {
            Movement = movement;
        }

        public IStrikerMovementOverride Movement { get; }
    }

    private readonly DashAbilityConfig dashConfig;
    private readonly Context context;

    private Vector2 dashDirection;
    private float remainingDashTime;
    private float remainingCooldownTime;
    private bool isActive;

    public DashAbility(DashAbilityConfig config, Context context) : base(config)
    {
        dashConfig = config;
        this.context = context;
    }

    public float CooldownDuration => dashConfig != null ? dashConfig.Cooldown : 0f;
    public float CooldownRemaining => Mathf.Max(0f, remainingCooldownTime);

    public override void Tick(float deltaTime)
    {
        TickCooldown(deltaTime);
        TickActiveDash(deltaTime);
    }

    protected override void ActivateCore()
    {
        var started = context.Movement.TryBeginMovementOverride();
        if (!started) return;

        dashDirection = ResolveDashDirection();
        remainingDashTime = dashConfig.DashDuration;
        remainingCooldownTime = dashConfig.Cooldown;
        isActive = true;
        ApplyDashVelocity();
    }

    protected override void DisposeCore()
    {
        EndDash();
    }

    protected override bool CanActivateCore()
    {
        if (isActive) return false;
        if (remainingCooldownTime > 0f) return false;
        if (!HasMovement()) return false;
        if (!context.Movement.CanUseMovementOverride) return false;

        return CanResolveDashDirection();
    }

    private void TickCooldown(float deltaTime)
    {
        if (remainingCooldownTime <= 0f) return;
        remainingCooldownTime -= deltaTime;
    }

    private void TickActiveDash(float deltaTime)
    {
        if (!isActive) return;

        if (!HasMovement())
        {
            ClearActiveDashState();
            return;
        }

        remainingDashTime -= deltaTime;
        if (remainingDashTime <= 0f)
        {
            EndDash();
            return;
        }

        ApplyDashVelocity();
    }

    private Vector2 ResolveDashDirection()
    {
        var moveDirection = context.Movement.CurrentMoveDirection;
        if (moveDirection.sqrMagnitude > 0.0001f)
            return moveDirection.normalized;

        return Vector2.zero;
    }

    private bool CanResolveDashDirection()
    {
        var moveDirection = context.Movement.CurrentMoveDirection;
        var hasInvalidXDirection = context.Movement.Side == PlayerSide.Left
            ? moveDirection.x < 0f
            : moveDirection.x > 0f;

        if (hasInvalidXDirection) return false;

        return moveDirection.sqrMagnitude > 0.0001f;
    }

    private void ApplyDashVelocity()
    {
        if (!isActive) return;

        context.Movement.SetMovementOverrideVelocity(dashDirection * dashConfig.DashSpeed);
    }

    private void EndDash()
    {
        if (!isActive) return;

        ClearActiveDashState();

        if (HasMovement())
            context.Movement.EndMovementOverride();
    }

    private void ClearActiveDashState()
    {
        isActive = false;
        remainingDashTime = 0f;
        dashDirection = Vector2.zero;
    }

    private bool HasMovement()
    {
        if (context.Movement == null) return false;

        var movementObject = context.Movement as Object;
        if (movementObject == null) return true;

        return movementObject;
    }
}
