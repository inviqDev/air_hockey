using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SideOwner))]
[RequireComponent(typeof(AbilityController))]
[RequireComponent(typeof(EffectReceiver))]
public abstract class StrikerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    private readonly List<IMovementVelocityEffect> movementEffects = new();
    private SideOwner sideOwner;
    [SerializeField] private AbilityController abilityController;
    [SerializeField] private EffectReceiver effectReceiver;
    private AbilityContext abilityContext;
    private Rigidbody2D strikerRb;

    private bool isInitialized;
    private bool isMovementAllowed;

    protected bool IsMovementAllowed => isMovementAllowed;
    protected bool IsInitialized => isInitialized;
    public AbilityController AbilityController => abilityController;
    protected bool HasActiveAbilityWork =>
        (abilityController != null && abilityController.HasActiveAbility) ||
        (effectReceiver != null && effectReceiver.HasActiveEffects);

    private void Reset()
    {
        if (!strikerRb)
            strikerRb = GetComponent<Rigidbody2D>();

        if (strikerRb)
            ConfigureBody(strikerRb);
    }

    protected bool InitializeStrikerMovement(StrikerSetupContext setupContext)
    {
        if (!EnsureInitialized()) return false;

        abilityController.Initialize(abilityContext);
        UpdateMovementLoopState();
        return true;
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
        if (!strikerRb) return;

        SetLinearVelocity(strikerRb, Vector2.zero);
        strikerRb.angularVelocity = 0f;
        strikerRb.position = position;
        strikerRb.rotation = 0f;

        if (abilityController != null)
            abilityController.ResetState();

        if (effectReceiver != null)
            effectReceiver.ClearEffects();

        HandleMovementReset();
        UpdateMovementLoopState();
    }

    protected void ExecuteMovementStep(MovementCommand command)
    {
        var velocity = CalculateVelocity(command);
        ApplyVelocity(velocity);
    }

    protected void StopMovement()
    {
        if (strikerRb)
            SetLinearVelocity(strikerRb, Vector2.zero);
    }

    protected abstract void UpdateMovementLoopState();

    protected virtual void HandleMovementStopped()
    {
    }

    protected virtual void HandleMovementReset()
    {
    }

    private void OnDisable()
    {
        StopMovement();
    }

    private Vector2 CalculateVelocity(MovementCommand command)
    {
        var frameContext = new AbilityFrameContext(
            abilityContext,
            command.Move,
            Time.fixedDeltaTime,
            command.ActivationTriggers);

        if (command.ActivationTriggers != AbilityActivationTrigger.None)
            abilityController.RequestAbilityActivation(frameContext);

        var moveVelocity = command.Move * moveSpeed;
        var resolvedVelocity = ApplyMovementEffects(moveVelocity, command.Move);

        abilityController.TickAbilities(frameContext);
        effectReceiver.TickEffects(Time.fixedDeltaTime);

        return resolvedVelocity;
    }

    private Vector2 ApplyMovementEffects(Vector2 baseVelocity, Vector2 moveInput)
    {
        var resolvedVelocity = baseVelocity;
        var movementEffectContext = new MovementVelocityEffectContext(gameObject, transform, moveInput, Time.fixedDeltaTime);

        effectReceiver.GetEffects(movementEffects);
        for (var i = 0; i < movementEffects.Count; i++)
            resolvedVelocity = movementEffects[i].ModifyVelocity(resolvedVelocity, movementEffectContext);

        return resolvedVelocity;
    }

    private void ApplyVelocity(Vector2 velocity)
    {
        SetLinearVelocity(strikerRb, velocity);
    }

    private bool EnsureInitialized()
    {
        if (isInitialized)
            return true;

        if (!CacheReferences()) return false;

        ConfigureBody(strikerRb);
        abilityContext = new AbilityContext(gameObject, transform, effectReceiver);

        isInitialized = true;
        return true;
    }

    private bool CacheReferences()
    {
        var hasAllReferences = true;

        if (!strikerRb && !TryGetComponent(out strikerRb))
        {
            Debug.LogError($"{nameof(StrikerMovement)} on {name} requires a {nameof(Rigidbody2D)} component.", this);
            hasAllReferences = false;
        }

        if (!sideOwner && !TryGetComponent(out sideOwner))
        {
            Debug.LogError($"{nameof(StrikerMovement)} on {name} requires a {nameof(SideOwner)} component.", this);
            hasAllReferences = false;
        }

        if (!abilityController && !TryGetComponent(out abilityController))
            abilityController = gameObject.AddComponent<AbilityController>();

        if (!effectReceiver && !TryGetComponent(out effectReceiver))
            effectReceiver = gameObject.AddComponent<EffectReceiver>();

        return hasAllReferences;
    }

    private static void ConfigureBody(Rigidbody2D targetBody)
    {
        targetBody.bodyType = RigidbodyType2D.Dynamic;
        targetBody.gravityScale = 0f;
        targetBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        targetBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        targetBody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private static void SetLinearVelocity(Rigidbody2D body, Vector2 velocity)
    {
#if UNITY_6000_0_OR_NEWER
        body.linearVelocity = velocity;
#else
        body.velocity = velocity;
#endif
    }
}
