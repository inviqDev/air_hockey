using UnityEngine;

public readonly struct AbilitySlotData
{
    private readonly bool _canActivate;
    private readonly bool _hasCooldown;
    private readonly float _cooldownDuration;
    private readonly float _cooldownRemaining;

    public AbilitySlotData(
        AbilityConfig config,
        bool canActivate,
        bool hasCooldown,
        float cooldownDuration,
        float cooldownRemaining)
    {
        Config = config;
        _canActivate = canActivate;
        _hasCooldown = hasCooldown;
        _cooldownDuration = cooldownDuration;
        _cooldownRemaining = cooldownRemaining;
    }

    public bool HasAbility => Config != null;
    public bool IsEmpty => !HasAbility;
    public AbilityConfig Config { get; }
    public bool CanActivate => HasAbility && _canActivate;
    public bool HasCooldown => HasAbility && _hasCooldown;
    public float CooldownDuration => HasCooldown ? Mathf.Max(0f, _cooldownDuration) : 0f;
    public float CooldownRemaining => HasCooldown ? Mathf.Max(0f, _cooldownRemaining) : 0f;
    public float CooldownNormalized => CooldownDuration > 0f
        ? Mathf.Clamp01(CooldownRemaining / CooldownDuration)
        : 0f;
}
