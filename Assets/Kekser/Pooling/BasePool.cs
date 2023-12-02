using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kekser.Pooling
{
    public abstract class BasePool : ScriptableObject, IPool
    {
        public abstract void Prewarm();
        public abstract void Prewarm(int count);
        public abstract void Return(IPoolable poolable);
        public abstract void ReturnAll();
        public abstract object GetRaw(Vector3 position = default, Quaternion rotation = default);

        public abstract void OnSpawned(IPoolable poolable);
        public abstract void OnDespawned(IPoolable poolable);
    }
    
    public abstract class BasePool<T> : BasePool, IPool<T> where T : MonoBehaviour, IPoolable
    {
        [SerializeField] private T _prefab;
        [SerializeField] private int _initialSize;
        [SerializeField] private bool _canGrow;
        [SerializeField] private bool _canShrink;
        
        [NonSerialized]
        private bool _isInitialized = false;

        [NonSerialized]
        private List<T> _pool = new List<T>();
        [NonSerialized]
        private List<T> _active = new List<T>();
        [NonSerialized]
        private Dictionary<string, List<T>> _scenePool = new Dictionary<string, List<T>>();

        public sealed override void Prewarm()
        {
            Prewarm(_initialSize);
        }
        
        public sealed override void Prewarm(int count)
        {
            if (_isInitialized)
                return;
            
            Clear();

            SceneManager.sceneUnloaded += OnSceneUnloaded;

            for (int i = 0; i < count; i++)
                Instantiate();
            _isInitialized = true;
        }

        public sealed override object GetRaw(Vector3 position = default, Quaternion rotation = default)
        {
            return Get(position, rotation);
        }
        
        public T Get(Vector3 position = default, Quaternion rotation = default)
        {
            if (!_isInitialized)
                Prewarm();

            T obj = null;
            if (_pool.Count > 0)
                obj = _pool[0];
            else if (_canGrow)
                obj = Instantiate();

            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.transform.localScale = Vector3.one;
                Use(obj);
            }

            OnSpawned(obj);
            return obj;
        }

        public sealed override void Return(IPoolable poolable)
        {
            OnDespawned(poolable);
            Unuse(poolable as T);
            if (_canShrink && _pool.Count > _initialSize)
            {
                _pool.Remove(poolable as T);
                Destroy(poolable as T);
            }
        }

        public sealed override void ReturnAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
                Return(_active[i]);
        }

        public override void OnSpawned(IPoolable poolable)
        {
        }

        public override void OnDespawned(IPoolable poolable)
        {
        }
        
        private void Use(T poolable)
        {
            if (Pooling.IsQuitting)
                return;

            if (!_pool.Contains(poolable))
                return;
            
            poolable.OnSpawnInternal();
            _pool.Remove(poolable);
            _active.Add(poolable);
            
            if (!_scenePool.ContainsKey(SceneManager.GetActiveScene().name))
                _scenePool.Add(SceneManager.GetActiveScene().name, new List<T>() {poolable});
            else
                _scenePool[SceneManager.GetActiveScene().name].Add(poolable);
        }
        
        private void Unuse(T poolable)
        {
            if (Pooling.IsQuitting)
                return;
            
            if (!_active.Contains(poolable))
                return;
            
            poolable.transform.SetParent(null);
            DontDestroyOnLoad(poolable);
            poolable.OnDespawnInternal();
            _active.Remove(poolable);
            _pool.Add(poolable);
            
            if (_scenePool.ContainsKey(SceneManager.GetActiveScene().name))
                _scenePool[SceneManager.GetActiveScene().name].Remove(poolable);
        }

        private int Count()
        {
            return _pool.Count + _active.Count;
        }
        
        private void Clear()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            _isInitialized = false;
            ReturnAll();
            for (int i = 0; i < _pool.Count; i++)
            {
                Destroy(_pool[i]);
            }
            _pool.Clear();
            _active.Clear();
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            if (_scenePool.ContainsKey(scene.name))
            {
                for (int i = _scenePool[scene.name].Count - 1; i >= 0; i--)
                {
                    Return(_scenePool[scene.name][i]);
                }
            }
            
            for (int i = _active.Count -1; i >= 0; i--)
            {
                if (scene.name == _active[i].gameObject.scene.name)
                    Return(_active[i]);
            }
        }
        
        protected T Instantiate()
        {
            T obj = Instantiate(_prefab);
            obj.SetPool(this);
            DontDestroyOnLoad(obj.gameObject);
            _pool.Add(obj);
            obj.name = _prefab.name;
            return obj;
        }

        protected void Destroy(T poolable)
        {
            poolable.DestroyInternal();
        }
    }
}