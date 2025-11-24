// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="Animator"/>-related layer configuration asset.
    /// </summary>
    [CreateAssetMenu(fileName = "ANIL_AnimatorLayer", menuName = FrameworkUtility.MenuPath + "Animation/Animator Layer", order = FrameworkUtility.MenuOrder)]
    public sealed class EnhancedAnimatorLayer : EnhancedScriptableObject {
        #region Global Members
        [Section("Animator Layer")]

        [Tooltip("Identifier name of this layer")]
        [SerializeField, Enhanced, DisplayName("Name")] private string layerName = "Layer";

        [Tooltip("Weight of this layer")]
        [SerializeField, Enhanced, Range(0f, 1f)] private float weight = 1f;

        [Tooltip("Toggles this layer root motion")]
        [SerializeField] private bool rootMotion = false;

        [Space(10f)]

        [Tooltip("All states wrapped in this layer")]
        [SerializeField] private EnhancedAnimatorState[] states = new EnhancedAnimatorState[0];

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Default state of this layer")]
        [SerializeField, Enhanced, Required] private EnhancedAnimatorState defaultState = null;

        [Space(10f, order = 2), Title("Default Transition", "Default transition used to transit to this layer default state", order = 3), Space(5f, order = 4)]

        [Tooltip("Default transition used when exiting to this layer default state")]
        [SerializeField, Enhanced, Block] private EnhancedAnimatorTransitionSettings defaultTransition = new EnhancedAnimatorTransitionSettings();

        // -----------------------

        private int layerIndex = 0;

        // -----------------------

        /// <summary>
        /// Whether root motion is active on this layer or not.
        /// </summary>
        public bool RootMotion {
            get { return rootMotion; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes this layer.
        /// </summary>
        /// <param name="_index">Index of this layer.</param>
        public void Initialize(int _index) {

            layerIndex = _index;

            // States.
            ref EnhancedAnimatorState[] _span = ref states;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _span[i].Initialize(_index);
            }
        }
        #endregion

        #region Animation
        /// <summary>
        /// Plays a specific state on this layer.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> on which to play the transition.</param>
        /// <param name="_stateHash">Hash name of the sate to play.</param>
        /// <param name="_instant">If true, instantly plays the animation.</param>
        /// <returns>True if the given state could be successfully played, false otherwise.</returns>
        public bool Play(Animator _animator, int _stateHash, bool _instant = false) {

            ref EnhancedAnimatorState[] _span = ref states;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                EnhancedAnimatorState _state = _span[i];

                if (_state.Hash == _stateHash) {

                    _state.Play(_animator, layerIndex, _instant);
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc cref="PlayDefault(Animator, float)"/>
        public void PlayDefault(Animator _animator, bool _instant = false) {

            // Instant.
            if (_instant) {
                _animator.Play(defaultState.Hash, layerIndex);
                return;
            }

            PlayDefault(_animator, defaultTransition);
        }

        /// <summary>
        /// Plays the default state of this layer.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> on which to play the state.</param>
        /// <param name="_transitionDuration">Transition duration (in seconds).</param>
        public void PlayDefault(Animator _animator, float _transitionDuration) {
            EnhancedAnimatorTransitionSettings _settings = new EnhancedAnimatorTransitionSettings(_transitionDuration, 0f, false);
            PlayDefault(_animator, _settings);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private void PlayDefault(Animator _animator, EnhancedAnimatorTransitionSettings _settings) {
            EnhancedAnimatorTransition _transition = EnhancedAnimatorTransition.GetCache(_settings, StateTransitionMode.CrossFade);
            _transition.Play(_animator, defaultState, layerIndex);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get a specific <see cref="EnhancedAnimatorState"/> from this layer.
        /// </summary>
        /// <param name="_stateHash">The hash of the state to get.</param>
        /// <param name="_state">The state associated with the given hash.</param>
        /// <returns>True if the associated state could be successfully retrieved, false otherwise.</returns>
        public bool GetState(int _stateHash, out EnhancedAnimatorState _state) {

            ref EnhancedAnimatorState[] _span = ref states;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                EnhancedAnimatorState _temp = _span[i];

                if (_temp.Hash == _stateHash) {

                    _state = _temp;
                    return true;
                }
            }

            _state = null;
            return false;
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        /// <summary>
        /// Creates and setups this layer in an <see cref="AnimatorController"/> at a given layer index.
        /// </summary>
        /// <param name="_animator"><see cref="AnimatorController"/> on which to create this layer.</param>
        /// <param name="_layerIndex">Index of this layer.</param>
        internal void Create(AnimatorController _animator, int _layerIndex) {

            // Layer.
            AnimatorControllerLayer[] _layers = _animator.layers;

            if (_layers.Length == _layerIndex) {

                _animator.AddLayer(layerName);
                _layers = _animator.layers;
            }

            AnimatorControllerLayer _layer = _layers[_layerIndex];

            _layer.defaultWeight = weight;
            _layer.blendingMode  = AnimatorLayerBlendingMode.Override;
            _layer.name          = layerName;

            _animator.layers = _layers;
            layerIndex       = _layerIndex;

            // States.
            EnhancedCollection<AnimatorState> _states = new EnhancedCollection<AnimatorState>();
            GetStates(_layer.stateMachine);

            foreach (EnhancedAnimatorState _state in states) {
                _state.Create(_animator, _layerIndex, _states);
            }

            // Create transitions once we are sure all states were created.
            foreach (EnhancedAnimatorState _state in states) {
                _state.CreateExitTransition(_states, defaultState, defaultTransition);
            }

            // ----- Local Method ----- \\

            void GetStates(AnimatorStateMachine _root) {

                foreach (var _temp in _root.states) {
                    _states.Add(_temp.state);
                }

                foreach (var _subState in _root.stateMachines) {
                    GetStates(_subState.stateMachine);
                }
            }
        }
        #endif
        #endregion
    }
}
