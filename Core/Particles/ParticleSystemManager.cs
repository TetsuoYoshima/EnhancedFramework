// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.GameStates;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Singleton class managing various <see cref="ParticleSystem"/>-related elements, and all associated <see cref="EnhancedParticleSystemPlayer"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Particles/Particle System Manager"), DisallowMultipleComponent]
    public sealed class ParticleSystemManager : EnhancedSingleton<ParticleSystemManager>, IStableUpdate, IGameStateOverrideCallback {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init | UpdateRegistration.Stable;

        #region Global Members
        [Section("Particle Manager")]

        [SerializeField, Enhanced, Required] private Transform poolRoot = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("If true, pauses all particle systems on game pause")]
        [SerializeField] private bool pauseParticlePlayers = true;

        [Tooltip("Whether particle sytems are currently paused or not")]
        [SerializeField, Enhanced, ReadOnly] private bool isPaused = false;

        // -----------------------

        /// <summary>
        /// Are particle systems currently paused?
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
            ref List<EnhancedParticleSystemPlayer> _span = ref particles;

            for (int i = _span.Count; i-- > 0;) {
                _span[i].ParticleUpdate();
            }
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Unregistration.
            GameStateManager.Instance.UnregisterOverrideCallback(this);
        }

        // -------------------------------------------
        // Callback
        // -------------------------------------------

        private void OnStartLoading() {

            // Stop playing all particles.
            ref List<EnhancedParticleSystemPlayer> _span = ref particles;

            for (int i = _span.Count; i-- > 0;) {
                _span[i].Stop(ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            // Clear pools.
            ref List<Pair<ParticleSystemAsset, Transform>> _poolSpan = ref particlePools.collection;

            for (int i = _poolSpan.Count; i-- > 0;) {

                var _pair = _poolSpan[i];
                ParticleSystemAsset _particle = _pair.First;

                if (_particle.ClearOnLoading) {
                    _particle.ClearPool();

                    Destroy(_pair.Second.gameObject);
                    _poolSpan.RemoveAt(i);
                }
            }
        }
        #endregion

        #region Registration
        private static List<EnhancedParticleSystemPlayer> particles = new List<EnhancedParticleSystemPlayer>();

        // -----------------------

        /// <summary>
        /// Registers a specific active <see cref="EnhancedParticleSystemPlayer"/> instance.
        /// </summary>
        /// <param name="_player"><see cref="EnhancedParticleSystemPlayer"/> to register.</param>
        internal static void RegisterPlayer(EnhancedParticleSystemPlayer _player) {
            particles.Add(_player);
        }

        /// <summary>
        /// Unregisters a specific <see cref="EnhancedAudioPlayer"/> instance.
        /// </summary>
        /// <param name="_player"><see cref="EnhancedParticleSystemPlayer"/> to unregister.</param>
        internal static void UnregisterPlayer(EnhancedParticleSystemPlayer _player) {
            particles.RemoveSwap(_player);
        }
        #endregion

        #region Game State
        void IGameStateOverrideCallback.OnGameStateOverride(in GameStateOverride _state) {
            // Pause.
            Pause(_state.IsPaused);
        }
        #endregion

        #region Core
        private readonly PairCollection<ParticleSystemAsset, Transform> particlePools = new PairCollection<ParticleSystemAsset, Transform>();

        // -----------------------

        /// <param name="_position">Position (in world space) where to play this audio.</param>
        /// <inheritdoc cref="Play(ParticleSystemAsset, Transform)"/>
        public ParticleHandler Play(ParticleSystemAsset _particle, Vector3 _position) {
            return _particle.GetPoolInstance().Play(_position);
        }

        /// <summary>
        /// Plays a specific <see cref="ParticleSystemAsset"/>.
        /// </summary>
        /// <param name="_particle">The <see cref="ParticleSystemAsset"/> to play.</param>
        /// <param name="_transform"><see cref="Transform"/> reference for this particle to follow.</param>
        /// <returns><see cref="ParticleHandler"/> wrapper of the current play operation.</returns>
        public ParticleHandler Play(ParticleSystemAsset _particle, Transform _transform) {
            return _particle.GetPoolInstance().Play(_transform);
        }

        // -------------------------------------------
        // Pause
        // -------------------------------------------

        /// <summary>
        /// Pauses / unpauses particle systems.
        /// </summary>
        /// <param name="_pause">Whether to pause or unpause particles.</param>
        [Button(SuperColor.Orange)]
        public void Pause(bool _pause) {
            if (IsPaused == _pause) {
                return;
            }

            if (!pauseParticlePlayers) {
                return;
            }

            // Player pause.
            ref List<EnhancedParticleSystemPlayer> _span = ref particles;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                EnhancedParticleSystemPlayer _particle = _span[i];
                if (!_particle.IgnorePause) {

                    // Pause / Resume.
                    if (_pause) {
                        _particle.Pause();
                    } else {
                        _particle.Play();
                    }
                }
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get the root <see cref="Transform"/> of a specific <see cref="ParticleSystemAsset"/> pool.
        /// </summary>
        /// <param name="_particle"><see cref="ParticleSystemAsset"/> to get the associated pool root.</param>
        /// <returns>Pool root <see cref="Transform"/> of the given <see cref="ParticleSystemAsset"/>.</returns>
        public Transform GetPoolRoot(ParticleSystemAsset _particle) {

            // Particle registration and pool root creation.
            if (!particlePools.TryGetValue(_particle, out Transform _transform)) {
                _transform = new GameObject($"POOL_{_particle.name.RemovePrefix()}").transform;

                _transform.SetParent(poolRoot);
                _transform.ResetLocal();

                particlePools.Add(_particle, _transform);
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
