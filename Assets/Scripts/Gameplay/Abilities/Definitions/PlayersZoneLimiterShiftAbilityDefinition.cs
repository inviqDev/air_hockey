using UnityEngine;

[CreateAssetMenu(menuName = "Air Hockey/Abilities/Players Zone Limiter Shift", fileName = "PlayersZoneLimiterShiftAbility")]
public sealed class PlayersZoneLimiterShiftAbilityDefinition : AbilityDefinition
{
    [SerializeField, Min(0f)] private float shiftDistance = 0.75f;
    [SerializeField, Min(0.01f)] private float effectDuration = 4f;
    [SerializeField, Min(0f)] private float cooldown = 6f;

    public float ShiftDistance => shiftDistance;
    public float EffectDuration => effectDuration;
    public float Cooldown => cooldown;

    public override IAbility CreateAbility()
    {
        return new PlayersZoneLimiterShiftAbility(this);
    }
}
