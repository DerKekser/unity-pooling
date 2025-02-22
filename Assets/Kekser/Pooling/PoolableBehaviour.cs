using System.Collections.Generic;
using UnityEngine;

namespace Kekser.Pooling
{
    public abstract class PoolableBehaviour : MonoBehaviour, IPoolable
    {
        private readonly Dictionary<MonoBehaviour, bool> _enabledComponents = new Dictionary<MonoBehaviour, bool>();
        protected virtual bool StoreEnabledComponentState => false;
        
        protected IPool _pool;
        private bool _internalDestroyed;
        
        public void Despawn()
        {
            if (_pool == null)
            {
                Debug.LogWarning("PoolableBehaviour is not despawned while pool is null");
                OnDespawn();
                Destroy(gameObject);
            }
            else
                _pool.Return(this);
        }
        
        public void SetPool(IPool pool)
        {
            _pool = pool;
            gameObject.SetActive(false);
        }

        public void DestroyInternal()
        {
            _internalDestroyed = true;
            Destroy(gameObject);
        }
        
        public void OnSpawnInternal()
        {
            gameObject.SetActive(true);
            //OnSpawn();
            gameObject.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
        }

        public void OnDespawnInternal()
        {
            gameObject.SetActive(false);
            //OnDespawn();
            
            if (StoreEnabledComponentState)
                foreach (KeyValuePair<MonoBehaviour, bool> component in _enabledComponents)
                    component.Key.enabled = component.Value;

            gameObject.SendMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
        }
        
        public virtual void OnSpawn() {}
        public virtual void OnDespawn() {}

        protected virtual void Start()
        {
            if (_pool == null)
            {
                Debug.LogWarning("PoolableBehaviour is not spawned from a pool");
                OnSpawn();
            }

            if (StoreEnabledComponentState)
                foreach (var component in GetComponentsInChildren<MonoBehaviour>())
                    _enabledComponents[component] = component.enabled;
        }

        protected virtual void OnDestroy()
        {
            if (Pooling.IsQuitting || _internalDestroyed)
                return;
            
            Debug.LogWarning("PoolableBehaviour is destroyed. This is not recommended. Use Despawn() instead.");
            _pool?.Return(this);
        }
    }
}