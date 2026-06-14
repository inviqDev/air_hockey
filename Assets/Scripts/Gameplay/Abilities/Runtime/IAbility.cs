public interface IAbility
{
    AbilityConfig Config { get; }
    string Id { get; }
    bool CanActivate { get; }

    void Activate();
    void Tick(float deltaTime);
    void Dispose();
}
