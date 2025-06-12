// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Base class for a <see cref="ScriptableObject"/> managing any <see cref="EnhancedPoolableObject"/> <see cref="ObjectPool{T}"/>.
    /// </summary>
    public abstract class ScriptableObjectPool : EnhancedScriptableObject, IObjectPoolManager<EnhancedPoolableObject> {
        #region Global Members
        /// <summary>
        /// This scriptable managing <see cref="ObjectPool{T}"/>.
        /// </summary>
        public ObjectPool<EnhancedPoolableObject> Pool {
            get { return pool; }
        }

        /// <summary>
        /// This pool reference template prefab.
        /// </summary>
        public abstract EnhancedPoolableObject Template { get; protected set; }
        #endregion

        #region Pool
        protected readonly ObjectPool<EnhancedPoolableObject> pool = new ObjectPool<EnhancedPoolableObject>();
        protected List<EnhancedPoolableObject> activeObjects       = new List<EnhancedPoolableObject>();

        [NonSerialized] private bool isInitialized = false;

        // -----------------------

        /// <summary>
        /// Get an instance of this scriptable managing <see cref="ObjectPool{T}"/>.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.GetPoolInstance"/>
        public virtual EnhancedPoolableObject GetPoolInstance() {
            Initialize();

            EnhancedPoolableObject _instance = GetInstance_Internal();
            activeObjects.Add(_instance);

            return _instance;
        }

        /// <summary>
        /// Releases a specific instance and sent it back to this scriptable managing pool <see cref="ObjectPool{T}"/>.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.ReleasePoolInstance(T)"/>
        public virtual bool ReleasePoolInstance(EnhancedPoolableObject _instance) {
            Initialize();

            activeObjects.Remove(_instance);
            return ReleaseInstance_Internal(_instance);
        }

        /// <inheritdoc cref="ClearPool(int)"/>
        public virtual void ClearPool(bool _clear, bool _releaseActiveInstances, bool _safeNullCheck = true, int _capacity = 1) {
            if (_releaseActiveInstances) {

                ref List<EnhancedPoolableObject> _span = ref activeObjects;
                for (int i = _span.Count; i-- > 0;) {
                    EnhancedPoolableObject _instance = _span[i];

                    if (_safeNullCheck && (_instance == null)) {
                        _span.RemoveAt(i);
                        continue;
                    }

                    try {
                        _instance.Release();
                    } catch (MissingReferenceException e) {
                        this.LogErrorMessage("Catch Exception => " + e.Message);
                        _span.RemoveAt(i);
                    }
                }
            }

            if (_clear) {
                ClearPool(_capacity);
            }
        }

        /// <summary>
        /// Clears this scriptable managing pool content and destroys all its instances.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.ClearPool(int)"/>
        public virtual void ClearPool(int _capacity = 1) {
            pool.ClearPool(_capacity);
        }

        // -------------------------------------------
        // Manager
        // -------------------------------------------

        /// <inheritdoc cref="IObjectPoolManager{EnhancedPoolableObject}.CreateInstance"/>
        EnhancedPoolableObject IObjectPoolManager<EnhancedPoolableObject>.CreateInstance() {
            return CreateInstance();
        }

        /// <inheritdoc cref="IObjectPoolManager{EnhancedPoolableObject}.DestroyInstance(EnhancedPoolableObject)"/>
        void IObjectPoolManager<EnhancedPoolableObject>.DestroyInstance(EnhancedPoolableObject _instance) {
            DestroyInstance(_instance);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <summary>
        /// Initializes this pool.
        /// </summary>
        protected virtual void Initialize() {
            if (isInitialized)
                return;

            // The created instances from pool.Initialize can lead to double init in some cases - so make sure to set as init before.
            isInitialized = true;

            pool.Initialize(this);
            RegisterActivePool(this);
        }

        /// <summary>
        /// Get an instance from this pool.
        /// </summary>
        protected virtual EnhancedPoolableObject GetInstance_Internal() {
            return pool.GetPoolInstance();
        }

        /// <summary>
        /// Releases an instance from this pool.
        /// </summary>
        protected virtual bool ReleaseInstance_Internal(EnhancedPoolableObject _instance) {
            return pool.ReleasePoolInstance(_instance);
        }

        // -----------------------

        /// <inheritdoc cref="IObjectPoolManager{EnhancedPoolableObject}.CreateInstance"/>
        protected virtual EnhancedPoolableObject CreateInstance() {
            EnhancedPoolableObject instance = Instantiate(Template, Vector3.zero, Quaternion.identity, GameManager.Instance.Transform);
            return instance;
        }

        /// <inheritdoc cref="IObjectPoolManager{EnhancedPoolableObject}.DestroyInstance(EnhancedPoolableObject)"/>
        protected virtual void DestroyInstance(EnhancedPoolableObject instance) {
            try {
                Destroy(instance.gameObject);
            } catch (MissingReferenceException e) {
                this.LogErrorMessage(e.Message);
            }
        }
        #endregion

        #region Utility
        private static List<ScriptableObjectPool> activePools = new List<ScriptableObjectPool>();
        public static Action OnClearAllScriptableObjectPool   = null;

        // -----------------------

        /// <summary>
        /// Clears all active <see cref="ScriptableObjectPool"/>.
        /// </summary>
        public static void ClearAllScriptableObjectPool() {
            ref List<ScriptableObjectPool> _span = ref activePools;
            for (int i = _span.Count; i-- > 0;) {
                _span[i].ClearPool();
            }

            OnClearAllScriptableObjectPool?.Invoke();
        }

        // -------------------------------------------
        // Registration
        // -------------------------------------------

        /// <summary>
        /// Marks a pool as "active" and register it.
        /// </summary>
        protected static void RegisterActivePool(ScriptableObjectPool _pool) {
            activePools.Add(_pool);
        }

        /// <summary>
        /// Marks a pool as "inactive" and unregiser it.
        /// </summary>
        protected static void UnregisterActivePool(ScriptableObjectPool _pool) {
            activePools.Remove(_pool);
        }
        #endregion
    }
}
