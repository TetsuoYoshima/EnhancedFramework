// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Self-managed singleton instance class.
    /// <br/> The generic object type should be set as the same as this class type.
    /// <para/>
    /// Note that there should always be exactly one instance of this class at a time.
    /// <br/> Every other instance will automatically be destroyed,
    /// unless another behaviour is specified using <see cref="OnNonSingletonInstance"/>.
    /// <para/>
    /// </summary>
    /// <typeparam name="T">This object singleton type.</typeparam>
    public abstract class EnhancedSingleton<T> : EnhancedBehaviour where T : EnhancedSingleton<T> {
        #region Singleton Instance
        /// <summary>
        /// Called whenever a new singleton instance of this class is assigned.
        /// </summary>
        public static event Action<EnhancedSingleton<T>> OnInstanceChanged = null;

        private static T instance = null;

        // -----------------------

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static T Instance {
            get {
                #if UNITY_EDITOR
                if (!Application.isPlaying && (instance == null)) {
                    instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                }
                #endif

                return instance;
            } protected set {
                instance = value;
                OnInstanceChanged?.Invoke(value);
            }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            if (Instance.IsValid() && (instance != this)) {
                OnNonSingletonInstance();   // When a singleton instance already exist, call this method.
            } else {
                Instance = this as T;       // Set this object as singleton instance.
            }

            base.OnBehaviourEnabled();
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            if (Instance == this) {
                Instance = null;
            }
        }

        // -----------------------

        /// <summary>
        /// Called when a new non-singleton instance of this object is enabled.
        /// <para/>
        /// By default, this base method call will log an error and destroy the object. Override its content to specify another behaviour.
        /// </summary>
        protected virtual void OnNonSingletonInstance() {
            this.LogErrorMessage($"Singleton Instance of \"{GetType().Name}\" already exist! Destroying object \"{name}\"");
            Destroy(this);
        }
        #endregion
    }
}
