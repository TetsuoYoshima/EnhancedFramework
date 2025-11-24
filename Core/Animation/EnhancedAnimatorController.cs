// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if UNITY_2020_3 || UNITY_2022_2_OR_NEWER
#define DEFAULT_VALUES
#endif

#if UNITY_2020_3 || UNITY_2022_1_OR_NEWER
#define KEEP_ANIMATOR_STATE
#endif

using EnhancedEditor;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="Animator"/>-related controller <see cref="ScriptableObject"/> configuration asset.
    /// </summary>
    [CreateAssetMenu(fileName = "ANIC_AnimatorController", menuName = FrameworkUtility.MenuPath + "Animation/Animator Controller", order = FrameworkUtility.MenuOrder)]
    #pragma warning disable
    public sealed class EnhancedAnimatorController : EnhancedScriptableObject {
        #region Global Members
        [Section("Animator Controller")]

        #if UNITY_EDITOR
        [SerializeField, Enhanced, EnhancedTextArea(true)] private string description = string.Empty;
        #endif

        [Space(10f)]

        [Tooltip("Blends pivot point between body center of mass and feet pivot")]
        [SerializeField, Enhanced, Range(0f, 1f)] private float feetPivotBlend = 1f;

        [Space(5f)]

        [Tooltip("Automatic stabilization of feet during transition and blending")]
        [SerializeField] private bool stabilizeFeet = false;

        [Tooltip("Set whether the Animator sends events of type AnimationEvent")]
        [SerializeField] private bool animationEvents = true;

        [Space(10f)]

        [Tooltip("If true, keeps the current animator state when it is disabled")]
        [SerializeField, Enhanced, DisplayName("Keep State on Disable")] private bool keepAnimatorStateOnDisable = true;

        [Tooltip("If true, resets the animator default values when it is disabled")]
        [SerializeField, Enhanced, DisplayName("Reset to Default on Disable")] private bool resetDefaultValuesOnDisable = true;

        [Space(10f)]

        [Tooltip("If true, optimizes this animator rig on init (removing any non-exposed transforms)")]
        [SerializeField] private bool optimizeRig = false;

        [Tooltip("Bones of all rig transforms to not remove on optimization")]
        [SerializeField] private HumanBodyBones[] exposedTransforms = new HumanBodyBones[0];

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("All configured layers in this controller")]
        [SerializeField] private EnhancedAnimatorLayer[] layers = new EnhancedAnimatorLayer[0];

        [Tooltip("All configured parameters in this controller")]
        [SerializeField] private EnhancedAnimatorParameter[] parameters = new EnhancedAnimatorParameter[0];

        // -----------------------

        /// <summary>
        /// Total count of layeres configured in this animator.
        /// </summary>
        public int LayerCount {
            get { return layers.Length; }
        }

        /// <summary>
        /// Total count of parameters configured in this animator.
        /// </summary>
        public int ParameterCount {
            get { return parameters.Length; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a specific <see cref="Animator"/> instance with this controller.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> to initialize.</param>
        [Button(ActivationMode.Play, SuperColor.HarvestGold, IsDrawnOnTop = false)]
        public void Initialize(Animator _animator) {

            #if DEFAULT_VALUES
            _animator.writeDefaultValuesOnDisable = resetDefaultValuesOnDisable;
            #endif

            #if KEEP_ANIMATOR_STATE
            _animator.keepAnimatorStateOnDisable = keepAnimatorStateOnDisable;
            #else
            _animator.keepAnimatorControllerStateOnDisable = keepAnimatorStateOnDisable;
            #endif

            _animator.feetPivotActive   = feetPivotBlend;
            _animator.fireEvents        = animationEvents;
            _animator.stabilizeFeet     = stabilizeFeet;

            // Optimization.
            if (optimizeRig && _animator.isOptimizable) {

                string[] _exposedTransforms = Array.ConvertAll(exposedTransforms, t => {
                    Transform _transform = _animator.GetBoneTransform(t);
                    return _transform.IsValid() ? _transform.name : string.Empty;

                });

                AnimatorUtility.OptimizeTransformHierarchy(_animator.gameObject, _exposedTransforms);
            }

            // Layers.
            ref EnhancedAnimatorLayer[] _layerSpan = ref layers;
            int _count = _layerSpan.Length;

            for (int i = 0; i < _count; i++) {
                _layerSpan[i].Initialize(i);
            }

            // Parameters.
            ref EnhancedAnimatorParameter[] _parameterSpan = ref parameters;
            _count = _parameterSpan.Length;

            for (int i = 0; i < _count; i++) {
                _parameterSpan[i].Initialize(_animator);
            }
        }

        // -------------------------------------------
        // Registration
        // -------------------------------------------

        /// <summary>
        /// Registers a specific runtime <see cref="Animator"/> on this controller.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> to register.</param>
        public void Register(Animator _animator) {

            // Parameters.
            ref EnhancedAnimatorParameter[] _span = ref parameters;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _span[i].Register(_animator);
            }
        }

        /// <summary>
        /// Unregisters a specific runtime <see cref="Animator"/> from this controller.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> to unregister.</param>
        public void Unregister(Animator _animator) {

            // Parameters.
            ref EnhancedAnimatorParameter[] _span = ref parameters;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _span[i].Unregister(_animator);
            }
        }
        #endregion

        #region Animation
        /// <param name="_stateName">Name of the state to play.</param>
        /// <inheritdoc cref="Play(Animator, int, int, bool)"/>
        public bool Play(Animator _animator, string _stateName, bool _instant = false) {
            int _hash = Animator.StringToHash(_stateName);
            return Play(_animator, _hash, _instant);
        }

        /// <inheritdoc cref="Play(Animator, int, int, bool)"/>
        public bool Play(Animator _animator, int _stateHash, bool _instant = false) {
            int _count = layers.Length;
            for (int i = 0; i < _count; i++) {

                if (Play(_animator, _stateHash, i, _instant)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Plays a specific state in this animator.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> on which to play the state.</param>
        /// <param name="_stateHash">Hash of the state to play.</param>
        /// <param name="_layerIndex">Index of the layer on which to play the state.</param>
        /// <param name="_instant">If true, instantly plays the animation.</param>
        /// <returns>True if the state could be successfully played, false otherwise.</returns>
        public bool Play(Animator _animator, int _stateHash, int _layerIndex, bool _instant = false) {
            if (_layerIndex >= layers.Length) {
                return false;
            }

            return layers[_layerIndex].Play(_animator, _stateHash, _instant);
        }

        /// <inheritdoc cref="PlayDefault(Animator, int, float)"/>
        public void PlayDefault(Animator _animator, int _layerIndex, bool _instant = false) {
            layers[_layerIndex].PlayDefault(_animator, _instant);
        }

        /// <summary>
        /// Plays the default state on a specific layer.
        /// </summary>
        /// <param name="_animator"><see cref="Animator"/> on which to play the state.</param>
        /// <param name="_layerIndex">Index of the layer on which to play the default state.</param>
        /// <param name="_transitionDuration">State transition duration (in seconds).</param>
        public void PlayDefault(Animator _animator, int _layerIndex, float _transitionDuration) {
            layers[_layerIndex].PlayDefault(_animator, _transitionDuration);
        }
        #endregion

        #region Parameter
        // -------------------------------------------
        // Bool
        // -------------------------------------------

        /// <inheritdoc cref="GetDoc{T}(Animator, string, int)"/>
        public bool GetBool(Animator _animator, string _parameterName) {
            int _hash = Animator.StringToHash(_parameterName);
            return GetBool(_animator, _hash);
        }

        /// <inheritdoc cref="GetDoc{T}(Animator, string, int)"/>
        public bool GetBool(Animator _animator, int _parameterHash) {
            return _animator.GetBool(_parameterHash);
        }

        /// <inheritdoc cref="SetDoc{T}(Animator, string, int, T)"/>
        public void SetBool(Animator _animator, string _parameterName, bool _value) {
            int _hash = Animator.StringToHash(_parameterName);
            SetBool(_animator, _parameterName, _value);
        }

        /// <inheritdoc cref="SetDoc{T}(Animator, string, int, T)"/>
        public void SetBool(Animator _animator, int _parameterHash, bool _value) {
            if (GetParameter(_parameterHash, out EnhancedAnimatorParameter _parameter)) {
                _parameter.SetBool(_animator, _value);
                return;
            }

            _animator.SetBool(_parameterHash, _value);
        }

        // -------------------------------------------
        // Int
        // -------------------------------------------

        /// <inheritdoc cref="GetDoc{T}(Animator, string, int)"/>
        public int GetInt(Animator _animator, string _parameterName) {
            int _hash = Animator.StringToHash(_parameterName);
            return GetInt(_animator, _hash);
        }

        /// <inheritdoc cref="GetDoc{T}(Animator, string, int)"/>
        public int GetInt(Animator _animator, int _parameterHash) {
            return _animator.GetInteger(_parameterHash);
        }

        /// <inheritdoc cref="SetDoc{T}(Animator, string, int, T)"/>
        public void SetInt(Animator _animator, string _parameterName, int _value) {
            int _hash = Animator.StringToHash(_parameterName);
            SetInt(_animator, _parameterName, _value);
        }

        /// <inheritdoc cref="SetDoc{T}(Animator, string, int, T)"/>
        public void SetInt(Animator _animator, int _parameterHash, int _value) {
            if (GetParameter(_parameterHash, out EnhancedAnimatorParameter _parameter)) {
                _parameter.SetInt(_animator, _value);
                return;
            }

            _animator.SetInteger(_parameterHash, _value);
        }

        // -------------------------------------------
        // Float
        // -------------------------------------------

        /// <inheritdoc cref="GetDoc{T}(Animator, string, int)"/>
        public float GetFloat(Animator _animator, string _parameterName) {
            int _hash = Animator.StringToHash(_parameterName);
            return GetFloat(_animator, _hash);
        }

        /// <inheritdoc cref="GetDoc{T}(Animator, string, int)"/>
        public float GetFloat(Animator _animator, int _parameterHash) {
            return _animator.GetFloat(_parameterHash);
        }

        /// <inheritdoc cref="SetDoc{T}(Animator, string, int, T)"/>
        public void SetFloat(Animator _animator, string _parameterName, float _value) {
            int _hash = Animator.StringToHash(_parameterName);
            SetFloat(_animator, _parameterName, _value);
        }

        /// <inheritdoc cref="SetDoc{T}(Animator, string, int, T)"/>
        public void SetFloat(Animator _animator, int _parameterHash, float _value) {
            if (GetParameter(_parameterHash, out EnhancedAnimatorParameter _parameter)) {
                _parameter.SetFloat(_animator, _value);
                return;
            }

            _animator.SetFloat(_parameterHash, _value);
        }

        // -------------------------------------------
        // Trigger
        // -------------------------------------------

        /// <inheritdoc cref="SetTrigger(Animator, int)"/>
        public void SetTrigger(Animator _animator, string _parameterName) {
            int _hash = Animator.StringToHash(_parameterName);
            SetTrigger(_animator, _parameterName);
        }

        /// <summary>
        /// Sets this parameter trigger on a specific <see cref="Animator"/>.
        /// </summary>
        /// <inheritdoc cref="SetDoc{T}(Animator, string, int, T)"/>
        public void SetTrigger(Animator _animator, int _parameterHash) {
            if (GetParameter(_parameterHash, out EnhancedAnimatorParameter _parameter)) {
                _parameter.SetTrigger(_animator);
                return;
            }

            _animator.SetTrigger(_parameterHash);
        }

        /// <inheritdoc cref="SetTrigger(Animator, int)"/>
        public void ResetTrigger(Animator _animator, string _parameterName) {
            int _hash = Animator.StringToHash(_parameterName);
            ResetTrigger(_animator, _parameterName);
        }

        /// <summary>
        /// Resets this parameter trigger on a specific <see cref="Animator"/>.
        /// </summary>
        /// <inheritdoc cref="SetDoc{T}(Animator, string, int, T)"/>
        public void ResetTrigger(Animator _animator, int _parameterHash) {
            if (GetParameter(_parameterHash, out EnhancedAnimatorParameter _parameter)) {
                _parameter.ResetTrigger(_animator);
                return;
            }

            _animator.ResetTrigger(_parameterHash);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <param name="_parameterName">Name of the parameter to get.</param>
        /// <inheritdoc cref="GetParameter(int, out EnhancedAnimatorParameter)"/>
        public bool GetParameter(string _parameterName, out EnhancedAnimatorParameter _parameter) {
            int _hash = Animator.StringToHash(_parameterName);
            return GetParameter(_parameterName, out _parameter);
        }

        /// <summary>
        /// Get an <see cref="EnhancedAnimatorParameter"/> from this controller.
        /// </summary>
        /// <param name="_parameterHash">Hash of the parameter to get.</param>
        /// <param name="_parameter">Matching parameter from this controller.</param>
        /// <returns>True if an associated <see cref="EnhancedAnimatorParameter"/> could be found, false otherwise.</returns>
        public bool GetParameter(int _parameterHash, out EnhancedAnimatorParameter _parameter) {

            ref EnhancedAnimatorParameter[] _span = ref parameters;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _parameter = _span[i];

                if (_parameter.Hash == _parameterHash) {
                    return true; 
                }
            }

            _parameter = null;
            return false;
        }

        // -------------------------------------------
        // Doc
        // -------------------------------------------

        /// <summary>
        /// Get this parameter value from a specific <see cref="Animator"/>.
        /// </summary>
        /// <typeparam name="T">Parameter value type.</typeparam>
        /// <param name="_animator"><see cref="Animator"/> from which to get this parameter value.</param>
        /// <param name="_parameterName">Name of the parameter to get the associated value.</param>
        /// <param name="_parameterHash">Hash of the parameter to get the associated value.</param>
        /// <returns>This parameter value.</returns>
        private T GetDoc<T>(Animator _animator, string _parameterName, int _parameterHash) {
            return default;
        }

        /// <summary>
        /// Set this parameter value on a specific <see cref="Animator"/>.
        /// </summary>
        /// <typeparam name="T">Parameter value type.</typeparam>
        /// <param name="_animator"><see cref="Animator"/> on which to set this parameter value.</param>
        /// <param name="_parameterName">Name of the parameter to get the associated value.</param>
        /// <param name="_parameterHash">Hash of the parameter to get the associated value.</param>
        /// <param name="_value">Value to assign to this parameter.</param>
        private void SetDoc<T>(Animator _animator, string _parameterName, int _parameterHash, T _value) { }
        #endregion

        #region Utility
        /// <summary>
        /// Get an <see cref="EnhancedAnimatorLayer"/> at a specific index.
        /// <para/>
        /// Use <see cref="LayerCount"/> to get the total count of layers in this animator.
        /// </summary>
        /// <param name="_index">Index of the layer to get.</param>
        /// <returns>The <see cref="EnhancedAnimatorLayer"/> at the given index.</returns>
        public EnhancedAnimatorLayer GetLayer(int _index) {
            return layers[_index];
        }

        /// <summary>
        /// Get an <see cref="EnhancedAnimatorParameter"/> at a specific index.
        /// <para/>
        /// Use <see cref="ParameterCount"/> to get the total count of parameters in this animator.
        /// </summary>
        /// <param name="_index">Index of the parameter to get.</param>
        /// <returns>The <see cref="EnhancedAnimatorParameter"/> at the given index.</returns>
        public EnhancedAnimatorParameter GetParameter(int _index) {
            return parameters[_index];
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        /// <summary>
        /// Creates and setups this controller in a specific <see cref="AnimatorController"/>.
        /// </summary>
        /// <param name="_animator"><see cref="AnimatorController"/> on which to configure this object.</param>
        [Button(ActivationMode.Editor, SuperColor.Crimson, IsDrawnOnTop = false)]
        internal void Create(AnimatorController _animator) {

            int _result = EditorUtility.DisplayDialogComplex("Erase Controller",
                                                             "Do you want to erase this animator content and re-create it from scratch?\n\nAll configuration will be lost.",
                                                             "Yes", "Cancel", "No");

            switch (_result) {

                // Erase.
                case 0:

                    while (_animator.layers.Length > 1) {
                        _animator.RemoveLayer(1);
                    }

                    var _layer = _animator.layers[0].stateMachine;

                    _layer.stateMachines = new ChildAnimatorStateMachine[0];
                    _layer.states = new ChildAnimatorState[0];

                    AnimatorControllerParameter[] _params = _animator.parameters;

                    foreach (AnimatorControllerParameter _param in _params) {
                        _animator.RemoveParameter(_param);
                    }

                    break;

                // Standard.
                case 2:
                    break;

                // Cancel.
                case 1:
                default:
                    return;
            }

            Undo.RecordObject(_animator, "Animator Setup");

            for (int i = 0; i < layers.Length; i++) {
                layers[i].Create(_animator, i);
            }

            AnimatorControllerParameter[] _parameters = _animator.parameters;
            for (int i = 0; i < parameters.Length; i++) {
                parameters[i].Create(_animator, ref _parameters);
            }

            _animator.parameters = _parameters;

            EditorUtility.SetDirty(_animator);
        }
        #endif
        #endregion
    }
}
