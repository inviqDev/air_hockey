using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "Puck Scale Config", menuName = "Abilities/Puck Scale Ability")]
public sealed class PuckScaleAbilityConfig : AbilityConfig
{
    [Header("Puck Scale")]
    [SerializeField, Min(0.01f)] private float downscaledScaleMultiplier = 0.5f;
    [SerializeField, Min(0f)] private float animationDuration = 0.2f;
    [SerializeField] private Ease animationEase = Ease.OutQuad;

    public float DownscaledScaleMultiplier => downscaledScaleMultiplier;
    public float AnimationDuration => animationDuration;
    public Ease AnimationEase => animationEase;
}
