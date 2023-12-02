using UnityEngine;

namespace Kekser.Pooling
{
    public interface IPool
    {
        public void Prewarm();
        public void Prewarm(int count);
        public void Return(IPoolable poolable);
        public void ReturnAll();
        
        public object GetRaw(Vector3 position = default, Quaternion rotation = default);

        void OnSpawned(IPoolable poolable);
        void OnDespawned(IPoolable poolable);
    }
    
    public interface IPool<out T> : IPool where T : IPoolable
    {
        public T Get(Vector3 position = default, Quaternion rotation = default);
    }
}