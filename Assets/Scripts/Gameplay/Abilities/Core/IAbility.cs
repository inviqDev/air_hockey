public interface IAbility
{
    AbilityDefinition Definition { get; }
    bool IsActive { get; }

    void Initialize(AbilityContext context, AbilityDefinition definition);
    void Tick(in AbilityFrameContext frameContext);
    void ResetState();
    void Dispose();
}
