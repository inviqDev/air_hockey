public abstract class TimedEffect : Effect
{
    private float remainingTime;

    protected TimedEffect(float duration)
    {
        remainingTime = duration;
    }

    public override bool IsFinished => remainingTime <= 0f;

    public override void TickEffect(EffectContext context, float deltaTime)
    {
        remainingTime -= deltaTime;
        OnTickEffect(context, deltaTime);
    }

    protected virtual void OnTickEffect(EffectContext context, float deltaTime)
    {
    }
}
