public interface IPoolable
{
    void OnGetFromPool();
    void OnMoveToPool();
}
