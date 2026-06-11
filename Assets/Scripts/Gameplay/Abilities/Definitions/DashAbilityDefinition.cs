using UnityEngine;

[CreateAssetMenu(menuName = "Air Hockey/Abilities/Dash", fileName = "DashAbility")]
public sealed class DashAbilityDefinition : AbilityDefinition
{
    [SerializeField, Min(0f)] private float dashSpeed = 16f;
    [SerializeField, Min(0f)] private float dashDuration = 0.12f;
    [SerializeField, Min(0f)] private float cooldown = 0.35f;

    public float DashSpeed => dashSpeed;
    public float DashDuration => dashDuration;
    public float Cooldown => cooldown;

    public override IAbility CreateAbility()
    {
        return new DashAbility(this);
    }
}
