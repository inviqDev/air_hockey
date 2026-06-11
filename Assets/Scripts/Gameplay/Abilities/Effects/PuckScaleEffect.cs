using UnityEngine;

public sealed class PuckScaleEffect : TimedEffect
{
    private readonly float scaleMultiplier;
    private Vector3 originalScale;

    public PuckScaleEffect(float scaleMultiplier, float duration)
        : base(duration)
    {
        this.scaleMultiplier = scaleMultiplier;
    }

    public override void StartEffect(EffectContext context)
    {
        originalScale = context.TargetTransform.localScale;
        context.TargetTransform.localScale = originalScale * scaleMultiplier;
    }

    public override void EndEffect(EffectContext context)
    {
        context.TargetTransform.localScale = originalScale;
    }
}
