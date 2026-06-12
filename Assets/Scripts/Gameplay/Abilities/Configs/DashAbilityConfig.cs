using UnityEngine;

[CreateAssetMenu(fileName = "Dash Config", menuName = "Abilities/Dash Ability")]
public sealed class DashAbilityConfig : AbilityConfig
{
    [Header("Dash")]
    [SerializeField, Min(0f)] private float dashSpeed = 16f;
    [SerializeField, Min(0f)] private float dashDuration = 0.12f;
    [SerializeField, Min(0f)] private float cooldown = 0.35f;

    public float DashSpeed => dashSpeed;
    public float DashDuration => dashDuration;
    public float Cooldown => cooldown;
}
