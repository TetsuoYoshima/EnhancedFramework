// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.VFX;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="EnhancedAssetFeedback"/> used to play various <see cref="VisualEffect"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "VEA_VisualEffect", menuName = MenuPath + "Visual Effect", order = MenuOrder)]
    public sealed class VisualEffectAsset : EnhancedAssetFeedback, IObjectPoolManager<VisualEffectPlayer> {
        #region Global Members
        [Section("Visual Effect Asset")]

        [Tooltip("Effects wrapped in this asset")]
        [SerializeField] private BlockArray<VisualEffect> visualEffects = new BlockArray<VisualEffect>();

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Reference transform related local offset correction")]
        [SerializeField] private Vector3 localOffset = Vector3.zero;

        [Tooltip("Local scale applied to this FX")]
        [SerializeField] private Vector3 scale = Vector3.one;

        [Space(10f)]

        [Tooltip("If true, ensures that only one instance is playing at a time")]
        [SerializeField] private bool avoidDuplicate = false;

        [Tooltip("If true, keeps this effect playing while the game is paused")]
        [SerializeField] private bool ignorePause = false;

        [Space(5f)]

        [Tooltip("If true, clears this effects pool when performing a loading operation")]
        [SerializeField] private bool clearOnLoading = false;

        // -----------------------

        /// <summary>
        /// Reference <see cref="Transform"/> local offset.
        /// </summary>
        public Vector3 LocalOffset {
            get { return localOffset; }
        }

        public Vector3 Scale {
            get { return scale; }
        }

        /// <summary>
        /// Determines if this effect should keeps playing while performing a scene loading operation.
        /// </summary>
        public bool ClearOnLoading {
            get { return clearOnLoading; }
        }

        /// <summary>
        /// Determines if this effect should keeps playing while performing a scene loading operation.
        /// </summary>
        public bool IgnorePause {
            get { return ignorePause; }
        }

        /// <summary>
        /// Determines if only one instance of this effect should be playing at a time.
        /// </summary>
        public bool AvoidDuplicate {
            get { return avoidDuplicate; }
        }
        #endregion

        #region Behaviour
        [NonSerialized] private VisualEffectHandler handler = default;

        // -----------------------

        /// <inheritdoc cref="PlayEffect(Transform)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VisualEffectHandler PlayEffect(Vector3 _position) {
            return VisualEffectManager.Instance.Play(this, _position);
        }

        /// <summary>
        /// Plays this visual effect.
        /// </summary>
        /// <inheritdoc cref="VisualEffectManager.Play(VisualEffectAsset, Transform)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VisualEffectHandler PlayEffect(Transform _transform) {
            return VisualEffectManager.Instance.Play(this, _transform);
        }

        // -------------------------------------------
        // Handler
        // -------------------------------------------

        /// <summary>
        /// Sets this effect current handler.
        /// </summary>
        /// <param name="_handler">This effect current handler.</param>
        internal void SetHandler(VisualEffectHandler _handler) {
            handler = _handler;
        }

        /// <summary>
        /// Get the last created <see cref="VisualEffectHandler"/> to play this effect.
        /// </summary>
        /// <param name="_handler">Last created <see cref="VisualEffectHandler"/> for this effect.</param>
        /// <returns>True if this effect handler is valid, false otherwise.</returns>
        public bool GetHandler(out VisualEffectHandler _handler) {
            _handler = handler;
            return _handler.IsValid;
        }

        // -------------------------------------------
        // Feedback
        // -------------------------------------------

        protected override void DoPlay(FeedbackPlayOptions _options, Transform _transform, Vector3 _position) {
            // Instant play (delay managed in the player).
            OnPlay(_options, _transform, _position);
        }

        protected override void OnPlay(FeedbackPlayOptions _options, Transform _transform, Vector3 _position) {
            switch (_options) {
                // Play at position.
                case FeedbackPlayOptions.PlayAtPosition:
                    PlayEffect(_position);
                    break;

                // Follow transform.
                case FeedbackPlayOptions.FollowTransform:
                    PlayEffect(_transform);
                    break;

                // Vanilla.
                case FeedbackPlayOptions.None:
                default:
                    this.LogErrorMessage("Visual Effect cannot be played without a proper Transform reference");
                    break;
            }
        }

        protected override void OnStop() {
            handler.Stop();
        }
        #endregion

        #region Pool
        [NonSerialized] private readonly ObjectPool<VisualEffectPlayer> effectPool = new ObjectPool<VisualEffectPlayer>(0);
        [NonSerialized] private bool isInitialize = false;

        // -----------------------

        /// <summary>
        /// Get a <see cref="VisualEffectPlayer"/> instance from this pool.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.GetPoolInstance"/>
        public VisualEffectPlayer GetPoolInstance() {

            if (!isInitialize) {

                effectPool.Initialize(this);
                isInitialize = true;
            }

            return effectPool.GetPoolInstance();
        }

        /// <summary>
        /// Releases a specific <see cref="VisualEffectPlayer"/> instance and sent it back to this pool.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.ReleasePoolInstance(T)"/>
        public bool ReleasePoolInstance(VisualEffectPlayer _player) {
            return effectPool.ReleasePoolInstance(_player);
        }

        /// <summary>
        /// Clears this <see cref="VisualEffectPlayer"/> pool content and destroys all its instances.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.ClearPool(int)"/>
        public void ClearPool(int _capacity = 1) {
            effectPool.ClearPool(_capacity);
        }

        // -------------------------------------------
        // Manager
        // -------------------------------------------

        /// <inheritdoc cref="IObjectPoolManager{VisualEffectPlayer}.CreateInstance"/>
        VisualEffectPlayer IObjectPoolManager<VisualEffectPlayer>.CreateInstance() {
            VisualEffectPlayer _player = new GameObject("VEP_NewEffect").AddComponent<VisualEffectPlayer>();
            Transform _transform = _player.transform;

            VisualEffect _effect = Instantiate(GetEffect(), _transform);
            _effect.transform.ResetLocal();
            _effect.transform.localScale = scale;

            _transform.SetParent(VisualEffectManager.Instance.GetPoolRoot(this));
            _transform.ResetLocal();

            _player.Initialize(this);

            return _player;
        }

        /// <inheritdoc cref="IObjectPoolManager{VisualEffectPlayer}.DestroyInstance(VisualEffectPlayer)"/>
        void IObjectPoolManager<VisualEffectPlayer>.DestroyInstance(VisualEffectPlayer _instance) {
            Destroy(_instance.gameObject);
        }
        #endregion

        #region Utility
        private int lastRandomIndex = -1;

        // -----------------------

        /// <summary>
        /// Get a random effect from this feedback.
        /// </summary>
        public VisualEffect GetEffect() {
            return visualEffects.Array.Random(ref lastRandomIndex);
        }
        #endregion
    }
}
