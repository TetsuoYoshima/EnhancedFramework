// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

namespace EnhancedFramework.Core {
    // ===== Weight Controller ===== \\

    /// <summary>
    /// Base audio <see cref="WeightController{T}"/> class.
    /// </summary>
    /// <typeparam name="T">Audio controlled object type.</typeparam>
    public sealed class AudioWeightController<T> : WeightController<T>, IPoolableObject where T : IWeightControl {
        #region Global Members
        private int priority = 0;

        // -----------------------

        /// <summary>
        /// Priority of this audio. Audio with a higher priority can override the weight on others.
        /// </summary>
        public int Priority {
            get { return priority; }
        }
        #endregion

        #region Controller
        /// <param name="_priority">Priority of this audio.</param>
        /// <inheritdoc cref="WeightController{T}.Initialize(T, float, bool)"/>
        public void Initialize(T _value, float _weight, int _priority, bool _instant) {
            Initialize(_value, _weight, _instant);
            priority = _priority;
        }
        #endregion

        #region Pool
        void IPoolableObject.OnCreated(IObjectPool _pool) { }

        void IPoolableObject.OnRemovedFromPool() { }

        void IPoolableObject.OnSentToPool() {

            // Security.
            Stop(true);
        }
        #endregion
    }

    // ===== Components ===== \\

    /// <summary>
    /// Base audio-related <see cref="EnhancedActivatorBehaviour"/>.
    /// </summary>
    public abstract class AudioControllerBehaviour : EnhancedActivatorBehaviour {
        #region Trigger
        protected override bool InteractWithTrigger(EnhancedBehaviour _actor) {
            return base.InteractWithTrigger(_actor) && AudioManager.Instance.HasTriggerTags(_actor);
        }
        #endregion
    }

    /// <summary>
    /// Base audio-related <see cref="WeightAreaController"/>.
    /// </summary>
    public abstract class AudioWeightControllerBehaviour : WeightAreaController {
        #region Trigger
        protected override bool InteractWithTrigger(EnhancedBehaviour _actor) {
            return base.InteractWithTrigger(_actor) && AudioManager.Instance.HasTriggerTags(_actor);
        }
        #endregion
    }
}
