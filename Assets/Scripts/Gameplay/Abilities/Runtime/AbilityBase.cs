public abstract class AbilityBase : IAbility
{
    protected AbilityBase(AbilityConfig config)
    {
        Config = config;
    }

    public AbilityConfig Config { get; }
    public string Id => Config != null ? Config.Id : string.Empty;
    public bool CanActivate => !IsDisposed && Config != null && CanActivateCore();

    protected bool IsDisposed { get; private set; }

    public void Activate()
    {
        if (!CanActivate) return;

        ActivateCore();
    }

    public virtual void Tick(float deltaTime)
    {
    }

    public void Cancel()
    {
        if (IsDisposed) return;

        CancelCore();
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        DisposeCore();
        IsDisposed = true;
    }

    protected abstract bool CanActivateCore();
    protected abstract void ActivateCore();

    protected virtual void CancelCore()
    {
    }

    protected virtual void DisposeCore()
    {
    }
}
