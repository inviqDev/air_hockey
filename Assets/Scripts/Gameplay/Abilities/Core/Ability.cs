public abstract class Ability : IAbility
{
    protected AbilityContext Context { get; private set; }

    public AbilityDefinition Definition { get; private set; }
    public virtual bool IsActive => false;

    public void Initialize(AbilityContext context, AbilityDefinition definition)
    {
        Context = context;
        Definition = definition;
        OnInitialized();
    }

    public virtual void Tick(in AbilityFrameContext frameContext)
    {
    }

    public virtual void ResetState()
    {
    }

    public virtual void Dispose()
    {
    }

    protected virtual void OnInitialized()
    {
    }
}
