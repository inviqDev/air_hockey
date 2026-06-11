using UnityEngine;

[CreateAssetMenu(menuName = "Air Hockey/Abilities/Puck Scale", fileName = "PuckScaleAbility")]
public sealed class PuckScaleAbilityDefinition : AbilityDefinition
{
    [SerializeField, Min(0f)] private float scaleMultiplier = 0.5f;
    [SerializeField, Min(0.01f)] private float effectDuration = 4f;
    [SerializeField, Min(0f)] private float cooldown = 6f;

    public float ScaleMultiplier => scaleMultiplier;
    public float EffectDuration => effectDuration;
    public float Cooldown => cooldown;

    public override IAbility CreateAbility()
    {
        return new PuckScaleAbility(this);
    }
}
