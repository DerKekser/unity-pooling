using UnityEngine;

namespace Game.Scripts.Helper.Pooling
{
    public interface IPoolable
    {
        public void SetPool(IPool pool);
        public void DestroyInternal();
        public void OnSpawnInternal();
        public void OnDespawnInternal();
    }
}