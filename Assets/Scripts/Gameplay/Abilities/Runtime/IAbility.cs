public interface IAbility
{
    string Id { get; }
    bool CanActivate { get; }

    void Activate();
    void Tick(float deltaTime);
    void Dispose();
}
