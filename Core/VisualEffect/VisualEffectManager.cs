// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.GameStates;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Singleton class managing various <see cref="VisualEffect"/>-related elements, and all associated <see cref="VisualEffectPlayer"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Visual Effects/Visual Effect Manager"), DisallowMultipleComponent]
    public sealed class VisualEffectManager : EnhancedSingleton<VisualEffectManager>, IStableUpdate, IGameStateOverrideCallback {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init | UpdateRegistration.Stable;

        #region Global Members
        [Section("Visual Effect Manager")]

        [Tooltip("Pooling-related reference parent Transform")]
        [SerializeField, Enhanced, Required] private Transform poolRoot = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("If true, pauses all visual effects on game pause")]
        [SerializeField] private bool pauseVisualEffects = true;

        [Tooltip("Whether visual effects are currently paused or not")]
        [SerializeField, Enhanced, ReadOnly] private bool isPaused = false;

        // -----------------------

        /// <summary>
        /// Are visual effects currently paused?
        /// </summary>
        public bool IsPaused {
            get { return isPaused; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Registration.
            GameStateManager.Instance.RegisterOverrideCallback(this);
        }

        protected override void OnInit() {
            base.OnInit();

            // Initialization.
            EnhancedSceneManager.OnStartLoading += OnStartLoading;
        }

        void IStableUpdate.Update() {
            // Particles update.
            ref List<VisualEffectPlayer> _span = ref visualEffects;

            for (int i = _span.Count; i-- > 0;) {
                _span[i].EffectUpdate();
            }
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Unregistration.
            GameStateManager.Instance.UnregisterOverrideCallback(this);
        }

        // -----------------------

        private void OnStartLoading() {
            // Stop playing all effects.
            ref List<VisualEffectPlayer> _span = ref visualEffects;

            for (int i = _span.Count; i-- > 0;) {
                _span[i].Stop(ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            // Clear pools.
            ref List<Pair<VisualEffectAsset, Transform>> _poolSpan = ref visualEffectPools.collection;
            
            for (int i = _poolSpan.Count; i-- > 0;) {

                var _pair = _poolSpan[i];
                VisualEffectAsset _particle = _pair.First;

                if (_particle.ClearOnLoading) {
                    _particle.ClearPool();

                    Destroy(_pair.Second.gameObject);
                    _poolSpan.RemoveAt(i);
                }
            }
        }
        #endregion

        #region Registration
        private static List<VisualEffectPlayer> visualEffects = new List<VisualEffectPlayer>();

        // -----------------------

        /// <summary>
        /// Registers a specific active <see cref="VisualEffectPlayer"/> instance.
        /// </summary>
        /// <param name="_player"><see cref="VisualEffectPlayer"/> to register.</param>
        internal static void RegisterPlayer(VisualEffectPlayer _player) {
            visualEffects.Add(_player);
        }

        /// <summary>
        /// Unregisters a specific <see cref="VisualEffectPlayer"/> instance.
        /// </summary>
        /// <param name="_player"><see cref="VisualEffectPlayer"/> to unregister.</param>
        internal static void UnregisterPlayer(VisualEffectPlayer _player) {
            visualEffects.RemoveSwap(_player);
        }
        #endregion

        #region Game State
        void IGameStateOverrideCallback.OnGameStateOverride(in GameStateOverride _state) {
            // Pause.
            Pause(_state.IsPaused);
        }
        #endregion

        #region Core
        private readonly PairCollection<VisualEffectAsset, Transform> visualEffectPools = new PairCollection<VisualEffectAsset, Transform>();

        // -----------------------

        /// <param name="_position">Position (in world space) where to play this effect.</param>
        /// <inheritdoc cref="Play(VisualEffectAsset, Transform)"/>
        public VisualEffectHandler Play(VisualEffectAsset _particle, Vector3 _position) {
            return _particle.GetPoolInstance().Play(_position);
        }

        /// <summary>
        /// Plays a specific <see cref="VisualEffectAsset"/>.
        /// </summary>
        /// <param name="_particle">The <see cref="VisualEffectAsset"/> to play.</param>
        /// <param name="_transform"><see cref="Transform"/> reference for this effect to follow.</param>
        /// <returns><see cref="ParticleHandler"/> wrapper of the current play operation.</returns>
        public VisualEffectHandler Play(VisualEffectAsset _particle, Transform _transform) {
            return _particle.GetPoolInstance().Play(_transform);
        }

        // -------------------------------------------
        // Pause
        // -------------------------------------------

        /// <summary>
        /// Pauses / unpauses visual effects.
        /// </summary>
        /// <param name="_pause">Whether to pause or unpause effects.</param>
        [Button(SuperColor.Orange)]
        public void Pause(bool _pause) {
            if (IsPaused == _pause) {
                return;
            }

            if (!pauseVisualEffects) {
                return;
            }

            // Player pause.
            ref List<VisualEffectPlayer> _span = ref visualEffects;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                VisualEffectPlayer _effect = _span[i];
                if (!_effect.IgnorePause) {

                    // Pause / Resume.
                    if (_pause) {
                        _effect.Pause();
                    } else {
                        _effect.Play();
                    }
                }
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get the root <see cref="Transform"/> of a specific <see cref="VisualEffectAsset"/> pool.
        /// </summary>
        /// <param name="_effect"><see cref="VisualEffectAsset"/> to get the associated pool root.</param>
        /// <returns>Pool root <see cref="Transform"/> of the given <see cref="VisualEffectAsset"/>.</returns>
        public Transform GetPoolRoot(VisualEffectAsset _effect) {
            // Particle registration and pool root creation.
            if (!visualEffectPools.TryGetValue(_effect, out Transform _transform)) {
                _transform = new GameObject($"POOL_{_effect.name.RemovePrefix()}").transform;

                _transform.SetParent(poolRoot);
                _transform.ResetLocal();

                visualEffectPools.Add(_effect, _transform);
            }

            return _transform;
        }
        #endregion

        #region Logger
        public override Color GetLogMessageColor(LogType _type) {
            return SuperColor.Turquoise.Get();
        }
        #endregion
    }
}
