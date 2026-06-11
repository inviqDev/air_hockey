public interface IEffect
{
    bool IsFinished { get; }

    void StartEffect(EffectContext context);
    void TickEffect(EffectContext context, float deltaTime);
    void EndEffect(EffectContext context);
}
