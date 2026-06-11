public abstract class Effect : IEffect
{
    public virtual bool IsFinished => false;

    public virtual void StartEffect(EffectContext context)
    {
    }

    public virtual void TickEffect(EffectContext context, float deltaTime)
    {
    }

    public virtual void EndEffect(EffectContext context)
    {
    }
}
