// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Constraints for a forward movement.
    /// </summary>
    public enum ForwardConstraint {
        None     = 0,

        Forward  = 1,
        Backward = 2,
    }

    /// <summary>
    /// <see cref="Animator"/>-related state configuration asset.
    /// </summary>
    [CreateAssetMenu(fileName = "ANIS_AnimatorState", menuName = FrameworkUtility.MenuPath + "Animation/Animator State", order = FrameworkUtility.MenuOrder)]
    public sealed class EnhancedAnimatorState : EnhancedScriptableObject {
        #region Global Members
        [Section("Animator State")]

        [Tooltip("Identifier name of this state")]
        [SerializeField, Enhanced, DisplayName("Name")] private string stateName = "State";

        [Tooltip("Animation to play on this state")]
        [SerializeField] private AnimationClip animation = null;
         
        [Space(5f)]

        [Tooltip("Speed coefficient of this animation")]
        [SerializeField, Enhanced, Range(-5f, 5f)] private float speed = 1f;

        [Tooltip("Animation cycle offset (in normalized time)")]
        [SerializeField, Enhanced, Range(-10f, 10f)] private float cycleOffset = 0f;

        [Space(5f)]

        [Tooltip("If true, plays this animation in mirror")]
        [SerializeField] private bool mirror = false;

        [Tooltip("If true, enables foot IK")]
        [SerializeField] private bool footIK = false;

        [Tooltip("If true, write animation default values on animated keys")]
        [SerializeField] private bool writeDefaults = true;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("If true, allows the transit to this state while already playing")]
        [SerializeField] private bool canTransitToSelf = false;

        [Space(5f)]

        [Tooltip("All configured transitions in this state")]
        [SerializeField] private EnhancedCollection<EnhancedAnimatorStateTransition> transitions = new EnhancedCollection<EnhancedAnimatorStateTransition>();

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Toggles this state root motion")]
        [SerializeField] private bool rootMotion = false;

        [Space(5f)]

        [Tooltip("Root motion forward movement constraint")]
        [SerializeField, Enhanced, ShowIf(nameof(rootMotion))] private ForwardConstraint motionForwardContraint = ForwardConstraint.None;

        [Tooltip("Root motion influence on the object position")]
        [SerializeField, Enhanced, ShowIf(nameof(rootMotion))] private AxisConstraints motionPositionConstraints = AxisConstraints.X | AxisConstraints.Z;

        [Tooltip("Root motion influence on the object rotation")]
        [SerializeField, Enhanced, ShowIf(nameof(rootMotion))] private AxisConstraints motionRotationConstraints = AxisConstraints.None;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Space(5f, order = 2), Title("Default Transition", order = 3), Space(5f, order = 4)]

        [SerializeField, Enhanced, Block] private EnhancedAnimatorTransition defaultTransition = new EnhancedAnimatorTransition();

        [Space(15f, order = 0), Title("Exit Transition", order = 1), Space(5f, order = 2)]

        [Tooltip("Mode used for this state default exit transition")]
        [SerializeField, Enhanced, DisplayName("Mode")] private ExitTransitionMode exitTransitionMode = ExitTransitionMode.Default;

        // -----------------------

        [Tooltip("Normalized time at which the exit transition take effect")]
        [SerializeField, Enhanced, ShowIf(nameof(HasExitTransition)), Range(0f, 10f)] private float exitTime = .9f;

        [Space(5f)]

        [Tooltip("Default exit transition duration")]
        [SerializeField, Enhanced, ShowIf(nameof(UseDefaultExitWithDuration)), Range(0f, 10f)] private float exitTransitionDuration = .5f;

        [Tooltip("Default exit transition destination state")]
        [SerializeField, Enhanced, ShowIf(nameof(UseCustomExit)), DisplayName("State")] private EnhancedAnimatorState exitState = null;

        [Space(10f)]

        [SerializeField, Enhanced, ShowIf(nameof(UseCustomExit)), Block] private EnhancedAnimatorTransitionSettings exitTransition = new EnhancedAnimatorTransitionSettings();

        // -----------------------

        private int layerIndex = 0;
        private int hash       = 0;

        // -----------------------

        /// <summary>
        /// Simple name hash of this state.
        /// </summary>
        public int Hash {
            get {
                if (hash == 0) {
                    hash = Animator.StringToHash(stateName);
                    //this.LogWarning("State hash value was not correctly configured");
                }

                return hash;
            }
        }

        /// <summary>
        /// The <see cref="AnimationClip"/> of this state.
        /// </summary>
        public AnimationClip Animation {
            get { return animation; }
        }

        /// <summary>
        /// Whether tho allow transitions to this state while already playing.
        /// </summary>
        public bool CanTransitToSelf {
            get { return canTransitToSelf; }
        }

        /// <summary>
        /// Whether root motion is active on this layer or not.
        /// </summary>
        public bool RootMotion {
            get { return rootMotion; }
        }

        /// <summary>
        /// Indicates if this state has a configured exit transition.
        /// </summary>
        public bool HasExitTransition {
            get { return exitTransitionMode != ExitTransitionMode.None; }
        }

        /// <summary>
        /// Indicates if this state exit transition is defined to <see cref="ExitTransitionMode.DefaultWithDuration"/>.
        /// </summary>
        public bool UseDefaultExitWithDuration {
            get { return exitTransitionMode == ExitTransitionMode.DefaultWithDuration; }
        }

        /// <summary>
        /// Indicates if this state exit transition is defined to <see cref="ExitTransitionMode.Custom"/>.
        /// </summary>
        public bool UseCustomExit {
            get { return exitTransitionMode == ExitTransitionMode.Custom; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes this state.
        /// </summary>
        /// <param name="_layerIndex">Index of this state layer.</param>
        public void Initialize(int _layerIndex) {
            layerIndex = _layerIndex;
            hash       = Animator.StringToHash(stateName);
        }
        #endregion

        // --- Logic --- \\

        #region Animation
        /// <inheritdoc cref="Play(Animator, int, bool)"/>
        public void Play(Animator _animator, bool _instant = false) {
            Play(_animator, layerIndex, _instant);
        }

        /// <summary>
        /// Plays a transition from the current <see cref="Animator"/> state to this state.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> on which to play the transition.</param>
        /// <param name="_layerIndex">Index of this state layer.</param>
        /// <param name="_instant">If true, instantly plays the animation.</param>
        public void Play(Animator _animator, int _layerIndex, bool _instant = false) {

            int _hash = Hash;

            // Instant.
            if (_instant) {
                _animator.Play(_hash, _layerIndex);
                return;
            }

            int _currentHash = _animator.GetCurrentAnimatorStateInfo(_layerIndex).shortNameHash;
            int _nextHash    = _animator.GetNextAnimatorStateInfo(_layerIndex).shortNameHash;

            // Cancel transition if state is already playing, and not in transition / transitioning to self.
            if ((_currentHash == _hash) && ((_nextHash == 0) || (_nextHash == _hash))) {
                return;
            }

            EnhancedAnimatorTransition _transition = GetTransition();
            _transition.Play(_animator, this, _layerIndex);

            // ----- Local Method ----- \\

            EnhancedAnimatorTransition GetTransition() {

                ref List<EnhancedAnimatorStateTransition> _span = ref transitions.collection;
                int _count = _span.Count;

                for (int i = 0; i < _count; i++) {
                    EnhancedAnimatorStateTransition _state = _span[i];

                    if (_state.Contains(_currentHash, out EnhancedAnimatorTransition _transition) || _state.Contains(_nextHash, out _transition)) {
                        return _transition;
                    }
                }
                
                return defaultTransition;
            }
        }
        #endregion

        #region Motion
        /// <summary>
        /// Applies root motion on this state.
        /// </summary>
        /// <param name="_next"><see cref="Animator"/> next state.</param>
        /// <param name="_position">Root motion position velocity.</param>
        /// <param name="_rotation">Root motion rotation velocity.</param>
        public void ApplyMotion(EnhancedAnimatorState _next, ref Vector3 _position, ref Quaternion _rotation) {
            // Position.
            AxisConstraints _nextConstraint = _next.motionPositionConstraints;
            AxisConstraints _constraint     = motionPositionConstraints;

            if (!HasPositionContraint(_constraint, _nextConstraint, AxisConstraints.X)) {
                _position.x = 0f;
            }

            if (!HasPositionContraint(_constraint, _nextConstraint, AxisConstraints.Y)) {
                _position.y = 0f;
            }

            if (!HasPositionContraint(_constraint, _nextConstraint, AxisConstraints.Z)) {
                _position.z = 0f;
            }

            // Forward.
            _position = ApplyForwardConstraint(_position, motionForwardContraint);
            _position = ApplyForwardConstraint(_position, _next.motionForwardContraint);

            // Rotation.
            Vector3 _eulers = _rotation.eulerAngles;
            _nextConstraint = _next.motionRotationConstraints;
            _constraint     = motionRotationConstraints;

            if (!HasRotationContraint(_constraint, _nextConstraint, AxisConstraints.X)) {
                _eulers.x = 0f;
            }

            if (!HasRotationContraint(_constraint, _nextConstraint, AxisConstraints.Y)) {
                _eulers.y = 0f;
            }

            if (!HasRotationContraint(_constraint, _nextConstraint, AxisConstraints.Z)) {
                _eulers.z = 0f;
            }

            _rotation = Quaternion.Euler(_eulers);

            // ----- Local Methods ----- \\

            static Vector3 ApplyForwardConstraint(Vector3 _position, ForwardConstraint _constraint) {

                switch (_constraint) {

                    case ForwardConstraint.Forward:
                        _position.z = Mathf.Max(_position.z, 0f);
                        break;

                    case ForwardConstraint.Backward:
                        _position.z = Mathf.Min(_position.z, 0f);
                        break;

                    case ForwardConstraint.None:
                    default:
                        break;
                }

                return _position;
            }

            static bool HasPositionContraint(AxisConstraints _this, AxisConstraints _next, AxisConstraints _constraint) {
                return _this.HasFlagUnsafe(_constraint) || _next.HasFlagUnsafe(_constraint);
            }

            static bool HasRotationContraint(AxisConstraints _this, AxisConstraints _next, AxisConstraints _constraint) {
                return _this.HasFlagUnsafe(_constraint) || _next.HasFlagUnsafe(_constraint);
            }
        }
        #endregion

        // --- Utility --- \\

        #region Editor
        #if UNITY_EDITOR
        /// <summary>
        /// Creates and setups this state in an <see cref="AnimatorController"/>, on a specific layer.
        /// </summary>
        /// <param name="_animator"><see cref="AnimatorController"/> on which to create this state.</param>
        /// <param name="_layerIndex">Index of the layer on which to create this state.</param>
        internal void Create(AnimatorController _animator, int _layerIndex, EnhancedCollection<AnimatorState> _states) {

            AnimationClip _animation = animation;

            if (!FindStateByName(_states.collection, stateName, out AnimatorState _state)) {

                // An animation is required to create the state (otherwise, an error is thrown).
                if (_animation == null) {
                    _animation = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"t:{typeof(AnimationClip).Name}")[0]));
                }

                _state = _animator.AddMotion(_animation, _layerIndex);
                _states.Add(_state);
            }

            _state.writeDefaultValues   = writeDefaults;
            _state.cycleOffset          = cycleOffset;
            _state.iKOnFeet             = footIK;
            _state.mirror               = mirror;
            _state.speed                = speed;
            _state.name                 = stateName;
            _state.motion               = _animation;

            if (animation == null) {
                _state.motion = null;
            }

            EditorUtility.SetDirty(_state);
        }

        /// <summary>
        /// Creates and setups this state exit transition.
        /// </summary>
        /// <param name="_states">All states in the current layer.</param>
        /// <param name="_defaultState">Default state of the current layer.</param>
        /// <param name="_defaultTransition">Default transition used to exit the the current layer default state.</param>
        internal void CreateExitTransition(EnhancedCollection<AnimatorState> _states, EnhancedAnimatorState _defaultState,
                                           EnhancedAnimatorTransitionSettings _defaultTransition) {

            switch (exitTransitionMode) {

                // Default.
                case ExitTransitionMode.Default:
                    CreateTransition(_defaultState, _defaultTransition);
                    break;

                // Override duration.
                case ExitTransitionMode.DefaultWithDuration:

                    _defaultTransition.Duration = exitTransitionDuration;
                    CreateTransition(_defaultState, _defaultTransition);

                    break;

                // Custom.
                case ExitTransitionMode.Custom:
                    CreateTransition(exitState, exitTransition);
                    break;

                case ExitTransitionMode.None:
                default:
                    break;
            }

            // ----- Local Method ----- \\

            void CreateTransition(EnhancedAnimatorState _exitState, EnhancedAnimatorTransitionSettings _settings) {

                if (!FindStateByName(_states.collection, stateName, out AnimatorState _state) || !FindStateByName(_states.collection, _exitState.stateName, out AnimatorState _transitionState)) {
                    return;
                }

                AnimatorStateTransition[] _allTransitions = _state.transitions;
                if (!FindTransitionState(_allTransitions, _transitionState, out AnimatorStateTransition _transition)) {

                    // Create if don't exist.
                    // Overload taking a Transition does not saved it into the asset, so we don't use it.
                    _state.AddTransition(_transitionState, true);
                    _allTransitions = _state.transitions;

                    // Retrieve state.
                    if (!FindTransitionState(_allTransitions, _transitionState, out _transition)) {
                        return;
                    }
                }

                _transition.interruptionSource = TransitionInterruptionSource.None;
                _transition.orderedInterruption = false;
                _transition.canTransitionToSelf = false;
                _transition.isExit = false;
                _transition.mute = false;
                _transition.solo = false;
                _transition.hasExitTime = true;

                _transition.destinationState = _transitionState;
                _transition.exitTime = exitTime;

                _transition.duration = _settings.Duration;
                _transition.offset = _settings.Offset;
                _transition.hasFixedDuration = _settings.IsFixedDuration;

                EditorUtility.SetDirty(_transition);
                EditorUtility.SetDirty(_state);
            }
        }

        // -----------------------

        private bool FindStateByName(List<AnimatorState> _states, string _name, out AnimatorState _state) {

            int _count = _states.Count;
            for (int i = 0; i < _count; i++) {

                _state = _states[i];
                if (_state.name.Equals(_name, System.StringComparison.Ordinal)) {
                    return true;
                }
            }

            _state = null;
            return false;
        }

        private bool FindTransitionState(AnimatorStateTransition[] _transitions, AnimatorState _destinationState, out AnimatorStateTransition _transition) {

            for (int i = 0; i < _transitions.Length; i++) {

                _transition = _transitions[i];
                if (_transition.hasExitTime && (_transition.destinationState == _destinationState)) {
                    return true;
                }
            }

            _transition = null;
            return false;
        }
        #endif
        #endregion
    }
}
