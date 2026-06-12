using UnityEngine;

[RequireComponent(typeof(SideOwner))]
public abstract class StrikerMovement : MovableBody
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Dash")]
    [SerializeField, Min(0f)] private float dashSpeed = 16f;
    [SerializeField, Min(0f)] private float dashDuration = 0.12f;
    [SerializeField, Min(0f)] private float dashCooldown = 0.35f;

    private SideOwner sideOwner;
    private DashAbility dashAbility;

    protected bool IsDashActive => dashAbility != null && dashAbility.IsDashing;

    protected bool InitializeStrikerMovement()
    {
        if (!InitializeMovableBody()) return false;

        if (dashAbility == null)
            dashAbility = CreateDashAbility();

        UpdateMovementLoopState();
        return true;
    }

    protected void ExecuteMovementStep(MovementCommand command)
    {
        var velocity = CalculateVelocity(command);
        ApplyVelocity(velocity);
    }

    protected override void HandleMovementReset()
    {
        if (dashAbility != null)
            dashAbility.ResetState();
    }

    private Vector2 CalculateVelocity(MovementCommand command)
    {
        var moveVelocity = command.Move * moveSpeed;
        var dashVelocity = dashAbility != null
            ? dashAbility.Step(command.DashPressed, command.Move, sideOwner.Side, Time.fixedDeltaTime)
            : Vector2.zero;

        return moveVelocity + dashVelocity;
    }

    protected override bool CacheAdditionalReferences()
    {
        var hasAllReferences = true;

        if (!sideOwner && !TryGetComponent(out sideOwner))
        {
            Debug.LogError($"{nameof(StrikerMovement)} on {name} requires a {nameof(SideOwner)} component.", this);
            hasAllReferences = false;
        }

        return hasAllReferences;
    }

    private DashAbility CreateDashAbility()
    {
        var dash = new DashAbility(dashSpeed, dashDuration, dashCooldown);
        dash.ResetState();

        return dash;
    }
}
