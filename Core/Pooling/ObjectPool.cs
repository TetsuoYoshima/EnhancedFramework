// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;

namespace EnhancedFramework.Core {
	/// <summary>
	/// Base interface to inherit any poolable object from.
	/// </summary>
	public interface IPoolableObject {
        #region Content
        /// <summary>
        /// Called when this object is created.
        /// </summary>
        void OnCreated(IObjectPool _pool);

        /// <summary>
        /// Called when this element is removed from the pool.
        /// </summary>
        void OnRemovedFromPool();

        /// <summary>
        /// Called when this element is returned to the pool.
        /// </summary>
        void OnSentToPool();
        #endregion
    }

    /// <summary>
    /// Base non-generic interface to inherit any <see cref="IPoolableObject"/> pool from.
    /// </summary>
    public interface IObjectPool {
        #region Content
        /// <summary>
        /// Get an object instance from this pool.
        /// </summary>
        /// <returns>The object instance get from the pool.</returns>
        IPoolableObject GetPoolInstance();

        /// <summary>
        /// Releases a specific instance and sent it to the pool.
        /// </summary>
        /// <param name="_instance">Object instance to release and send to the pool.</param>
        /// <returns>True if the instance could be successfully released and sent to the pool, false otherwise.</returns>
        bool ReleasePoolInstance(IPoolableObject _instance);

        /// <summary>
        /// Clears this pool content and destroys all its instances.
        /// </summary>
        /// <param name="_capacity">New capacity of the pool.</param>
        void ClearPool(int _capacity = 1);
        #endregion
    }

    /// <summary>
    /// Base generic interface to inherit any <see cref="IPoolableObject"/> pool from.
    /// </summary>
    public interface IObjectPool<T> : IObjectPool where T : class, IPoolableObject {
        #region Content
        /// <inheritdoc cref="IObjectPool.GetPoolInstance"/>
        new T GetPoolInstance();

        /// <inheritdoc cref="IObjectPool.ReleasePoolInstance"/>
        bool ReleasePoolInstance(T _instance);

        // -------------------------------------------
        // Non-Generic Default Implementation
        // -------------------------------------------

        /// <inheritdoc/>
        IPoolableObject IObjectPool.GetPoolInstance() {
            return GetPoolInstance();
        }

        /// <inheritdoc/>
        bool IObjectPool.ReleasePoolInstance(IPoolableObject _instance) {
            if (_instance is T _objectInstance) {
                return ReleasePoolInstance(_objectInstance);
            }

            return false;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="ObjectPool{T}"/>-related interface, managing a pool and used to create and destroy instances.
    /// </summary>
    /// <typeparam name="T"><see cref="IPoolableObject"/> managed type.</typeparam>
    public interface IObjectPoolManager<T> : IObjectPool<T> where T : class, IPoolableObject {
        #region Content
        /// <summary>
        /// Creates a new object instance.
        /// </summary>
        /// <returns>Created object instance.</returns>
        T CreateInstance();

        /// <summary>
        /// Destroys a specific object instance.
        /// </summary>
        /// <param name="_instance">The instance to destroy.</param>
        void DestroyInstance(T _instance);
        #endregion
    }

    // ===== Core Pool ===== \\

    /// <summary>
    /// Generic pool system used to manage a specific <see cref="IPoolableObject"/> type.
    /// <para/>
    /// Keep in mind to initialize it as soon as possible using <see cref="Initialize(IObjectPoolManager{T})"/>.
    /// </summary>
    [Serializable]
    public sealed class ObjectPool<T> : IObjectPool<T> where T : class, IPoolableObject  {
        #region Global Members
        [SerializeField] private EnhancedCollection<T> pool = new EnhancedCollection<T>();
        private IObjectPoolManager<T> manager = null;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <param name="_capacity">Initial capacity of the pool.</param>
        /// <inheritdoc cref="ObjectPool{T}"/>
        public ObjectPool(int _capacity = 1) {
            pool = new EnhancedCollection<T>(_capacity);
        }
        #endregion

        #region Initializer
        /// <summary>
        /// Initializes this pool.
        /// <br/> Keep in mind to always initialize it as soon as possible.
        /// </summary>
        /// <param name="_manager">Manager used to create and destroy pool object instances.</param>
        public void Initialize(IObjectPoolManager<T> _manager) {
            if (manager == _manager) {
                return;
            }

            manager = _manager;

            ref List<T> _span = ref pool.collection;
            int _capacity = _span.Capacity;

            for (int i = _span.Count; i < _capacity; i++) {

                T _instance = CreateInstance();

                AssertInstanceInPool(_instance);
                _span.Add(_instance);
            }
        }
        #endregion

        #region Management
        /// <inheritdoc cref="IObjectPool.GetPoolInstance"/>
        public T GetPoolInstance() {
            T _instance;
            int _count = pool.Count;

            if (_count == 0) {
                _instance = CreateInstance();
            } else {
                int _index = _count - 1;

                _instance = pool[_index];
                pool.RemoveAt(_index);
            }           

            _instance.OnRemovedFromPool();
            AssertInstanceInPool(_instance);

            return _instance;
        }

        /// <inheritdoc cref="IObjectPool.ReleasePoolInstance"/>
        public bool ReleasePoolInstance(T _instance) {
            // Ignore it if already in the pool.
            if (pool.IndexOf(_instance) != -1) {
                return false;
            }

            AssertInstanceInPool(_instance);
            pool.Add(_instance);

            _instance.OnSentToPool();

            return true;
        }

        /// <inheritdoc cref="IObjectPool.ClearPool"/>
        public void ClearPool(int _capacity = 1) {
            // Destroy instances.
            ref List<T> _span = ref pool.collection;
            for (int i = _span.Count; i-- > 0;) {
                manager.DestroyInstance(_span[i]);
            }

            // Reset capacity.
            _span.Clear();
            _span.Capacity = _capacity;
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private T CreateInstance() {
            T _instance = manager.CreateInstance();

            _instance.OnCreated(manager);
            _instance.OnSentToPool();

            return _instance;
        }
        #endregion

        #region Utility
        [Conditional("DEVELOPMENT")]
        private void AssertInstanceInPool(T _instance) {
            Assert.IsFalse(pool.Contains(_instance));
        }
        #endregion
    }
}
