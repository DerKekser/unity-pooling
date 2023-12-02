namespace Kekser.Pooling
{
    public interface IPoolable
    {
        public void SetPool(IPool pool);
        public void DestroyInternal();
        public void OnSpawnInternal();
        public void OnDespawnInternal();
    }
}