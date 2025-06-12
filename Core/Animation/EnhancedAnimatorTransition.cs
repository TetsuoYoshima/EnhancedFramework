// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using UnityEngine;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    // ===== Enum & Wrapper ===== \\

    /// <summary>
    /// <see cref="EnhancedAnimatorTransition"/>-related state transition mode.
    /// </summary>
    public enum StateTransitionMode {
        [Tooltip("No transition - Instantly plays the animation")]
        None = 0,

        [Tooltip("Cross fades from the current animation to the new one")]
        CrossFade = 1,

        [Tooltip("Cross fades from the current animation to the new one at the same time")]
        CrossFadeAtTime = 2,

        [Tooltip("Cross fades from the current animation to the new one at the same inverse time (mirror)")]
        CrossFadeAtInverseTime = 3,
    }

    /// <summary>
    /// <see cref="EnhancedAnimatorTransition"/>-related exit transition mode.
    /// </summary>
    public enum ExitTransitionMode {
        [Tooltip("No transition")]
        None = 0,

        [Tooltip("Transits to the layer default state using its configured transition")]
        Default = 1,

        [Tooltip("Transits to the layer default state using a specific transition duration")]
        DefaultWithDuration = 2,

        [Tooltip("Uses a custom transition on this state exit")]
        Custom = 9,
    }

    /// <summary>
    /// Wrapper for a <see cref="EnhancedAnimatorState"/> transition.
    /// </summary>
    [Serializable]
    public struct EnhancedAnimatorTransitionSettings {
        #region Global Members
        [Tooltip("If true, uses normalized time for both duration and offset. Otherwise, values are in seconds")]
        public bool IsFixedDuration;

        [Space(5f)]

        [Tooltip("Transition duration")]
        [Enhanced, Range(0f, 10f)] public float Duration;

        [Tooltip("Transition animation offset")]
        [Enhanced, Range(-10f, 10f)] public float Offset;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <param name="_duration">Transition duration.</param>
        /// <param name="_offset">Transition animation offset.</param>
        /// <param name="_isFixedDuration">If true, uses normalized time for both duration and offset. Otherwise, values are in seconds.</param>
        /// <inheritdoc cref="EnhancedAnimatorTransitionSettings"/>
        public EnhancedAnimatorTransitionSettings(float _duration, float _offset, bool _isFixedDuration = false) {
            IsFixedDuration = _isFixedDuration;
            Duration        = _duration;
            Offset          = _offset;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get this transition normalized duration.
        /// </summary>
        /// <param name="_animation">Transition destination <see cref="AnimationClip"/>.</param>
        /// <returns>This transition normalized duration (from 0 to 1).</returns>
        public readonly float GetDuration(AnimationClip _animation) {
            return GetNormalizedValue(Duration, _animation);
        }

        /// <summary>
        /// Get this transition normalized offset.
        /// </summary>
        /// <param name="_animation">Transition destination <see cref="AnimationClip"/>.</param>
        /// <returns>This transition normalized offset (from 0 to 1).</returns>
        public readonly float GetOffset(AnimationClip _animation) {
            return GetNormalizedValue(Offset, _animation);
        }

        // -----------------------

        private readonly float GetNormalizedValue(float _value, AnimationClip _animation) {
            if (IsFixedDuration) {
                float _length = _animation.length;
                _value = (_length != 0f) ? (_value / _length) : 0f;
            }

            return _value;
        }
        #endregion
    }

    // ===== Transitions ===== \\

    /// <summary>
    /// Wrapper for a <see cref="EnhancedAnimatorState"/> transition.
    /// </summary>
    [Serializable]
    public sealed class EnhancedAnimatorTransition {
        #region Global Members
        [Tooltip("This transition state behaviour")]
        [SerializeField] private StateTransitionMode mode = StateTransitionMode.CrossFade;
        [SerializeField, Enhanced, Block] private EnhancedAnimatorTransitionSettings settings = new EnhancedAnimatorTransitionSettings(.5f, 0f, false);

        // -----------------------

        /// <summary>
        /// Utility temporary transition used to perform quick operations.
        /// </summary>
        private static readonly EnhancedAnimatorTransition temp = new EnhancedAnimatorTransition() {
            mode     = StateTransitionMode.CrossFade,
            settings = new EnhancedAnimatorTransitionSettings(.5f, 0f, false),
        };

        // -----------------------

        /// <summary>
        /// Defines this animation transition behaviour.
        /// </summary>
        public StateTransitionMode Mode {
            get { return mode; }
        }

        /// <summary>
        /// Settings of this transition.
        /// </summary>
        public EnhancedAnimatorTransitionSettings Settings {
            get { return settings; }
        }

        /// <summary>
        /// Duration of this transition.
        /// </summary>
        public float Duration {
            get { return settings.Duration; }
        }

        /// <summary>
        /// Animation offset of this transition.
        /// </summary>
        public float Offset {
            get { return settings.Offset; }
        }

        /// <summary>
        /// If true, uses time in seconds for duration, offset and mode. Otherwise, values are in normalized time.
        /// </summary>
        public bool IsFixedDuration {
            get { return settings.IsFixedDuration; }
        }
        #endregion

        #region Transition
        /// <summary>
        /// Plays this transition to a specific traget destination <see cref="AnimationClip"/>.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> on which to play this transition.</param>
        /// <param name="_state">State to play.</param>
        /// <param name="_layerIndex">Index of the layer on which to play this transition.</param>
        public void Play(Animator _animator, EnhancedAnimatorState _state, int _layerIndex) {

            AnimatorStateInfo _fromState = _animator.GetCurrentAnimatorStateInfo(_layerIndex);

            bool _canTransitToSelf = _state.CanTransitToSelf;
            int _stateHash = _state.Hash;

            // Already in transition to this state.
            if (!_canTransitToSelf && _animator.GetNextAnimatorStateInfo(_layerIndex).shortNameHash == _stateHash) {
                return;
            }

            // State already playing.
            if (_animator.GetCurrentAnimatorStateInfo(_layerIndex).shortNameHash == _stateHash) {

                if (!_canTransitToSelf && !_animator.IsInTransition(_layerIndex)) {
                    return;
                }

                _fromState = _animator.GetNextAnimatorStateInfo(_layerIndex);
            }

            switch (mode) {

                // Standard cross fade.
                case StateTransitionMode.CrossFade:

                    if (IsFixedDuration) {
                        _animator.CrossFadeInFixedTime(_stateHash, Duration, _layerIndex, Offset);
                    } else {
                        _animator.CrossFade(_stateHash, Duration, _layerIndex, Offset);
                    }

                    break;

                // Cross fade at time.
                case StateTransitionMode.CrossFadeAtTime:

                    if (IsFixedDuration) {

                        float _time = Offset + (_fromState.normalizedTime * _state.Animation.length);
                        _animator.CrossFadeInFixedTime(_stateHash, Duration, _layerIndex, Offset + _time);

                    } else {
                        _animator.CrossFade(_stateHash, Duration, _layerIndex, Offset + _fromState.normalizedTime);
                    }

                    break;

                // Cross fade at inverse time.
                case StateTransitionMode.CrossFadeAtInverseTime:

                    float _normalizedTime = 1f - _fromState.normalizedTime;

                    if (IsFixedDuration) {

                        float _time = Offset + (_normalizedTime * _state.Animation.length);
                        _animator.CrossFadeInFixedTime(_stateHash, Duration, _layerIndex, Offset + _time);

                    } else {
                        _animator.CrossFade(_stateHash, Duration, _layerIndex, Offset + _normalizedTime);
                    }

                    break;

                // Play instant.
                case StateTransitionMode.None:
                default:

                    if (IsFixedDuration) {
                        _animator.PlayInFixedTime(_stateHash, _layerIndex, Offset);
                    } else {
                        _animator.Play(_stateHash, _layerIndex, Offset);
                    }

                    break;
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get a cache transition, which can be used to perform quick operations.
        /// </summary>
        /// <param name="_settings">Settings of this transition.</param>
        /// <param name="_mode">Defines this transition behaviour</param>
        /// <returns>Configured cache <see cref="EnhancedAnimatorTransition"/>.</returns>
        public static EnhancedAnimatorTransition GetCache(EnhancedAnimatorTransitionSettings _settings, StateTransitionMode _mode = StateTransitionMode.CrossFade) {

            EnhancedAnimatorTransition _temp = temp;
            _temp.settings = _settings;
            _temp.mode     = _mode;

            return _temp;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="EnhancedAnimatorState"/> from incoming transition wrapper.
    /// </summary>
    [Serializable]
    #pragma warning disable
    public sealed class EnhancedAnimatorStateTransition {
        #region Global Members
        #if UNITY_EDITOR
        [PropertyOrder(-1)]

        [Tooltip("Editor name of this transition")]
        [SerializeField] private string name = "Transition";

        [Tooltip("Editor description of this transition")]
        [SerializeField, Enhanced, EnhancedTextArea(true)] private string description = string.Empty;

        [Space(5f)]
        #endif

        [SerializeField, Enhanced, Block] private EnhancedAnimatorTransition transition = new EnhancedAnimatorTransition();

        [Space(5f)]

        [Tooltip("From coming state of this transition")]
        [SerializeField] private BlockArray<EnhancedAnimatorState> fromStates = new BlockArray<EnhancedAnimatorState>();

        // -----------------------

        /// <summary>
        /// Total amount of states configured in this transition.
        /// </summary>
        public int StateCount {
            get { return fromStates.Count; }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get if this transition is configured for a specific state.
        /// </summary>
        /// <param name="_hash">Hash of the state to check.</param>
        /// <param name="_transition">This object <see cref="EnhancedAnimatorTransition"/>, if containing the given state hash.</param>
        /// <returns>True if this transition contains the given state, false otherwise.</returns>
        public bool Contains(int _hash, out EnhancedAnimatorTransition _transition) {

            ref EnhancedAnimatorState[] _span = ref fromStates.Array;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                if (_span[i].Hash == _hash) {

                    _transition = transition;
                    return true;
                }
            }

            _transition = null;
            return false;
        }

        /// <summary>
        /// Get an <see cref="EnhancedAnimatorState"/> from this transition at a given index.
        /// </summary>
        /// <param name="_index">Index of the state to get.</param>
        /// <returns>The state from this transition at the given index.</returns>
        public EnhancedAnimatorState GetState(int _index) {
            return fromStates[_index];
        }
        #endregion
    }
}
