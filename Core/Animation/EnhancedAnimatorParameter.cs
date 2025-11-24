// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if DOTWEEN_ENABLED
#define TWEENER
#endif

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

#if TWEENER
using DG.Tweening;
#endif

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="Animator"/>-related parameter configuration asset.
    /// </summary>
    [CreateAssetMenu(fileName = "ANIP_AnimatorParameter", menuName = FrameworkUtility.MenuPath + "Animation/Animator Parameter", order = FrameworkUtility.MenuOrder)]
    #pragma warning disable
    public sealed class EnhancedAnimatorParameter : EnhancedScriptableObject {
        #region Animator Wrapper
        /// <summary>
        /// Wrapper for a single <see cref="UnityEngine.Animator"/> operations.
        /// </summary>
        public sealed class AnimatorWrapper {
            private const float DelayValue = .001f;

            public readonly EnhancedAnimatorParameter Parameter = null;
            public readonly Animator Animator = null;

            private DelayHandler delay = default;
            private TweenHandler tween = default;

            private Action<float> tweenSetter = null;
            private float tweenTarget = 0f;

            /// <summary>
            /// True this animator is not affected by the game time scale, false otherwise.
            /// </summary>
            public bool RealTime {
                get { return Animator.updateMode == AnimatorUpdateMode.UnscaledTime; }
            }

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            public AnimatorWrapper(Animator _animator, EnhancedAnimatorParameter _parameter) {
                Parameter = _parameter;
                Animator  = _animator;
            }

            // -------------------------------------------
            // Utility
            // -------------------------------------------

            /// <inheritdoc cref="Delayer.Call(float, Action, bool)"/>
            public void Delay(Action _callback) {

                delay.Cancel();
                delay = Delayer.Call(DelayValue, _callback, RealTime);
            }

            #if TWEENER
            /// <inheritdoc cref="Tweener.Tween(float, float, Action{float}, float, Ease, bool, Action{bool})"/>
            public void Tween(float _from, float _to, Action<Animator, float> _setter, float _duration, Ease _ease) {

                if (tween.IsValid && (tweenTarget == _to)) {
                    return;
                }

                tween.Stop();

                tweenSetter ??= OnTweenSet;
                tweenTarget = _to;

                tween = Tweener.Tween(_from, _to, tweenSetter, _duration, _ease, RealTime, null);
            }
            #endif

            /// <inheritdoc cref="Tweener.Tween(float, float, Action{float}, float, AnimationCurve, bool, Action{bool})"/>
            public void Tween(float _from, float _to, Action<Animator, float> _setter, float _duration, AnimationCurve _curve) {

                if (tween.IsValid && (tweenTarget == _to)) {
                    return;
                }

                tween.Stop();

                tweenSetter ??= OnTweenSet;
                tweenTarget = _to;

                tween = Tweener.Tween(_from, _to, tweenSetter, _duration, _curve, RealTime, null);
            }

            /// <summary>
            /// Clears this wrapper content and stops its operations.
            /// </summary>
            public void Clear() {
                delay.Cancel();
                tween.Stop();
            }

            /// <summary>
            /// Called when setting this parameter value from a tween.
            /// </summary>
            private void OnTweenSet(float _value) {
                Parameter.OnSetCompelexValue(Animator, _value);
            }
        }
        #endregion

        #region Global Members
        [Section("Animator Parameter")]

        [Tooltip("Identifier name of this parameter")]
        [SerializeField, Enhanced, DisplayName("Name")] protected string parameterName = "Parameter";

        [Tooltip("Type of this parameter")]
        [SerializeField] private AnimatorControllerParameterType type = AnimatorControllerParameterType.Trigger;

        [Space(10f)]

        [Tooltip("Default value of this parameter")]
        [SerializeField, Enhanced, ShowIf(nameof(IsFloat)), DisplayName("Default")] private float defaultFloat  = 0f;

        [Tooltip("Default value of this parameter")]
        [SerializeField, Enhanced, ShowIf(nameof(IsBool)),  DisplayName("Default")] private bool defaultBool    = false;

        [Tooltip("Default value of this parameter")]
        [SerializeField, Enhanced, ShowIf(nameof(IsInt)),   DisplayName("Default")] private int defaultInt      = 0;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("If true, parameter value update will be delayed to the next frame")]
        [SerializeField] private bool delayUpdate = false;

        [Space(5f)]

        [Tooltip("Parameter update tween duration (in seconds)")]
        [SerializeField, Enhanced, ShowIf(nameof(CanUseTween)), Range(0f, 10f)] private float updateValueDuration = 0f;

        #if TWEENER
        [Tooltip("Parameter update evaluation ease")]
        [SerializeField, Enhanced, ShowIf(nameof(CanUseTween))] private Ease updateEase = Ease.Linear;
        #else
        [Tooltip("Parameter update evaluation curve")]
        [SerializeField, Enhanced, ShowIf(nameof(CanUseTween))] private AnimationCurve updateCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        #endif

        // -----------------------

        [NonSerialized] protected int hash = 0;

        // -----------------------

        /// <summary>
        /// Name hash of this parameter.
        /// </summary>
        public int Hash {
            get {
                if (hash == 0) {
                    hash = Animator.StringToHash(parameterName);
                    this.LogErrorMessage("Parameter hash value was not correctly configured");
                }

                return hash;
            }
        }

        /// <summary>
        /// Whether this parameter value can be updated using a tween or not.
        /// </summary>
        public bool CanUseTween {
            get { return IsInt || IsFloat; }
        }

        /// <summary>
        /// Whether this parameter is of <see cref="bool"/> type or not.
        /// </summary>
        public bool IsBool {
            get { return type == AnimatorControllerParameterType.Bool; }
        }

        /// <summary>
        /// Whether this parameter is of <see cref="int"/> type or not.
        /// </summary>
        public bool IsInt {
            get { return type == AnimatorControllerParameterType.Int; }
        }

        /// <summary>
        /// Whether this parameter is of <see cref="float"/> type or not.
        /// </summary>
        public bool IsFloat {
            get { return type == AnimatorControllerParameterType.Float; }
        }

        /// <summary>
        /// Whether this parameter is of trigger type or not.
        /// </summary>
        public bool IsTrigger {
            get { return type == AnimatorControllerParameterType.Trigger; }
        }
        #endregion

        #region Initialization
        private readonly EnhancedCollection<AnimatorWrapper> animators = new EnhancedCollection<AnimatorWrapper>();

        // -----------------------

        /// <summary>
        /// Initializes this parameter.
        /// </summary>
        public void Initialize(Animator _animator) {
            hash = Animator.StringToHash(parameterName);
        }

        // -------------------------------------------
        // Registration
        // -------------------------------------------

        /// <summary>
        /// Registers a specific runtime <see cref="Animator"/> on this parameter.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> to register.</param>
        public void Register(Animator _animator) {
            animators.Add(new AnimatorWrapper(_animator, this));
        }

        /// <summary>
        /// Unregisters a specific runtime <see cref="Animator"/> from this parameter.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> to unregister.</param>
        public void Unregister(Animator _animator) {

            ref List<AnimatorWrapper> _span = ref animators.collection;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                if (_span[i].Animator == _animator) {

                    _span.RemoveAt(i);
                    return;
                }
            }
        }
        #endregion

        // --- Update --- \\

        #region Simple
        private readonly List<Pair<Animator, bool>> simpleUpdateBuffer = new List<Pair<Animator, bool>>();
        private Action simpleUpdateCallback = null;

        // -------------------------------------------
        // Set
        // -------------------------------------------

        /// <inheritdoc cref="EnhancedAnimatorController.SetBool(Animator, int, bool)"/>
        public void SetBool(Animator _animator, bool _value) {

            if (!IsBool) {
                this.LogErrorMessage($"Parameter is not of type {typeof(bool).Name.Bold()}   ({type})");
                return;
            }

            UpdateValue(_animator, _value);
        }

        /// <inheritdoc cref="EnhancedAnimatorController.SetTrigger(Animator, int)"/>
        public void SetTrigger(Animator _animator) {

            if (!IsTrigger) {
                this.LogErrorMessage($"Parameter is not of type Trigger   ({type})");
                return;
            }

            UpdateValue(_animator, true);
        }

        /// <inheritdoc cref="EnhancedAnimatorController.ResetTrigger(Animator, int)"/>
        public void ResetTrigger(Animator _animator) {

            if (!IsTrigger) {
                this.LogErrorMessage($"Parameter is not of type Trigger   ({type})");
                return;
            }

            UpdateValue(_animator, false);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <summary>
        /// Manages a specific update call.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> associated with this call.</param>
        private void UpdateValue(Animator _animator, bool _value) {

            if (delayUpdate && GetWrapperIndex(_animator, out AnimatorWrapper _wrapper)) {

                simpleUpdateCallback ??= OnUpdateSimpleValue;
                simpleUpdateBuffer.Add(new Pair<Animator, bool>(_animator, _value));

                _wrapper.Delay(simpleUpdateCallback);
                return;
            }

            UpdateSimpleValue(_animator, _value);
        }

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        private void OnUpdateSimpleValue() {

            // Invalid.
            if (!simpleUpdateBuffer.SafeFirst(out Pair<Animator, bool> _value)) {
                return;
            }

            simpleUpdateBuffer.RemoveAt(0);
            UpdateSimpleValue(_value.First, _value.Second);
        }

        private void UpdateSimpleValue(Animator _animator, bool _value) {

            switch (type) {

                case AnimatorControllerParameterType.Bool:
                    _animator.SetBool(Hash, _value);
                    break;

                case AnimatorControllerParameterType.Trigger:
                    if (_value) {
                        _animator.SetTrigger(Hash);
                    } else {
                        _animator.ResetTrigger(Hash);
                    }
                    break;

                case AnimatorControllerParameterType.Float:
                case AnimatorControllerParameterType.Int:
                default:
                    break;
            }
        }
        #endregion

        #region Complex
        private readonly List<Pair<Animator, Pair<float, float>>> complexUpdateBuffer = new List<Pair<Animator, Pair<float, float>>>();
        private Action<Animator, float> complexUpdateSetter = null;
        private Action complexUpdateCallback      = null;

        // -------------------------------------------
        // Set
        // -------------------------------------------

        /// <inheritdoc cref="EnhancedAnimatorController.SetInt(Animator, int, int)"/>
        public void SetInt(Animator _animator, int _value) {

            if (!IsInt) {
                this.LogErrorMessage($"Parameter is not of type {typeof(int).Name.Bold()}   ({type})");
                return;
            }

            float _from = _animator.GetInteger(Hash);
            TweenValue(_animator, _from, _value);
        }

        /// <inheritdoc cref="EnhancedAnimatorController.SetFloat(Animator, int, float)"/>
        public void SetFloat(Animator _animator, float _value) {

            if (!IsFloat) {
                this.LogErrorMessage($"Parameter is not of type {typeof(float).Name.Bold()}   ({type})");
                return;
            }

            float _from = _animator.GetFloat(Hash);
            TweenValue(_animator, _from, _value);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <summary>
        /// Manages a specific update call.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> associated with this call.</param>
        private void TweenValue(Animator _animator, float _from, float _to) {

            if (delayUpdate && GetWrapperIndex(_animator, out AnimatorWrapper _wrapper)) {

                complexUpdateCallback ??= OnUpdateComplexValue;
                complexUpdateBuffer.Add(new Pair<Animator, Pair<float, float>>(_animator, new Pair<float, float>(_from, _to)));

                _wrapper.Delay(complexUpdateCallback);
                return;
            }

            UpdateComplexValue(_animator, _from, _to);
        }

        // -------------------------------------------
        // Callbacks
        // -------------------------------------------

        private void OnUpdateComplexValue() {

            // Invalid.
            if (!complexUpdateBuffer.SafeFirst(out Pair<Animator, Pair<float, float>> _value)) {
                return;
            }

            complexUpdateBuffer.RemoveAt(0);

            Pair<float, float> _range = _value.Second;
            UpdateComplexValue(_value.First, _range.First, _range.Second);
        }

        private void UpdateComplexValue(Animator _animator, float _from, float _to) {

            // Tween.
            if ((updateValueDuration > 0f) && GetWrapperIndex(_animator, out AnimatorWrapper _wrapper)) {

                complexUpdateSetter ??= OnSetCompelexValue;

                #if TWEENER
                _wrapper.Tween(_from, _to, complexUpdateSetter, updateValueDuration, updateEase);
                #else
                _wrapper.Tween(_from, _to, complexUpdateSetter, updateValueDuration, updateCurve);
                #endif

            } else {
                // Instant.
                OnSetCompelexValue(_animator, _to);
            }
        }

        private void OnSetCompelexValue(Animator _animator, float _value) {

            switch (type) {

                case AnimatorControllerParameterType.Float:
                    _animator.SetFloat(Hash, _value);
                    break;

                case AnimatorControllerParameterType.Int:
                    _animator.SetInteger(Hash, Mathf.RoundToInt(_value));
                    break;

                case AnimatorControllerParameterType.Trigger:
                case AnimatorControllerParameterType.Bool:
                default:
                    break;
            }
        }
        #endregion

        // --- Utility --- \\

        #region Utility
        /// <summary>
        /// Get the index of the wrapper associated with a specific <see cref="Animator"/>.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> to get the associated wrapper index.</param>
        /// <param name="_index">Index of this animator wrapper.</param>
        /// <returns>True if an associated wrapper could be found for this animator, false otherwise.</returns>
        private bool GetWrapperIndex(Animator _animator, out AnimatorWrapper _wrapper) {

            ref List<AnimatorWrapper> _span = ref animators.collection;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                _wrapper = _span[i];

                if (_wrapper.Animator == _animator) {
                    return true;
                }
            }

            _wrapper = null;
            return false;
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        /// <summary>
        /// Creates and setups this parameter in an <see cref="AnimatorController"/>.
        /// </summary>
        /// <param name="_animator"><see cref="AnimatorController"/> on which to create this parameter.</param>
        /// <param name="_parameters">All parameters of this animator.</param>
        internal void Create(AnimatorController _animator, ref AnimatorControllerParameter[] _parameters) {

            // Parameter.
            if (!FindParameterByName(_parameters, parameterName, out AnimatorControllerParameter _parameter)) {

                _animator.AddParameter(parameterName, type);
                _parameters = _animator.parameters;

                if (!FindParameterByName(_parameters, parameterName, out _parameter)) {
                    return;
                }
            }

            // Setup.
            _parameter.type = type;

            _parameter.defaultFloat = defaultFloat;
            _parameter.defaultBool  = defaultBool;
            _parameter.defaultInt   = defaultInt;

            // ----- Local Method ----- \\

            static bool FindParameterByName(AnimatorControllerParameter[] _parameters, string _name, out AnimatorControllerParameter _parameter) {

                int _count = _parameters.Length;
                for (int i = 0; i < _count; i++) {

                    _parameter = _parameters[i];
                    if (_parameter.name.Equals(_name, System.StringComparison.Ordinal)) {
                        return true;
                    }
                }

                _parameter = null;
                return false;
            }
        }
        #endif
        #endregion
    }
}
