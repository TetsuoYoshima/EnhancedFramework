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
    /// Manages the chronos of all associated <see cref="EnhancedBehaviour"/> and <see cref="Animator"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Utility/Chronos Controller"), DisallowMultipleComponent]
    public sealed class ChronosController : EnhancedBehaviour, IChronosModifierController {
        #region Global Members
        [Section("Chronos Controller")]

        [SerializeField] private EnhancedBehaviour[] behaviours = new EnhancedBehaviour[0];
        [SerializeField] private Animator[] animators           = new Animator[0];

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, ReadOnly, DisplayName("Chronos Overr.")] private float chronosOverride    = ChronosManager.ChronosDefaultValue;
        [SerializeField, Enhanced, ReadOnly, DisplayName("Chronos Coef. ")] private float chronosCoefficient = ChronosManager.ChronosDefaultValue;

        // -----------------------

        private readonly ChronosWrapper chronosWrapper = new ChronosWrapper();
        #endregion

        #region Enhanced Behaviour
        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            if (behaviours.Length == 0) {
                behaviours = GetComponentsInChildren<EnhancedBehaviour>();
            }

            if (animators.Length == 0) {
                animators = GetComponentsInChildren<Animator>();
            }
        }
#endif
        #endregion

        // ===== Chronos ===== \\

        #region Coefficient
        /// <inheritdoc cref="ChronosWrapper.PushCoefficient"/>
        public ChronosHandler PushCoefficient(int _id, float _chronosOverride) {

            float _coef = chronosWrapper.PushCoefficient(_id, _chronosOverride, this, out ChronosHandler _handler);
            SetCoefficient(_coef);

            return _handler;
        }

        /// <inheritdoc cref="ChronosWrapper.PushCoefficient"/>
        public ChronosHandler PushCoefficient(int _id, float _chronosOverride, float _duration, bool _realTime = true, Action _onComplete = null) {

            float _coef = chronosWrapper.PushCoefficient(_id, _chronosOverride, this, out ChronosHandler _handler, _duration, _realTime, _onComplete);
            SetCoefficient(_coef);

            return _handler;
        }

        /// <inheritdoc cref="ChronosWrapper.PopCoefficient"/>
        public void PopCoefficient(int _id, bool _withCallback = true) {
            chronosWrapper.PopCoefficient(_id, _withCallback);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private void SetCoefficient(float _coef) {
            chronosCoefficient = _coef;
            RefreshChronos();
        }
        #endregion

        #region Override
        /// <inheritdoc cref="ChronosWrapper.PushOverride"/>
        public ChronosHandler PushOverride(int _id, float _chronosOverride, int _priority) {

            float _override = chronosWrapper.PushOverride(_id, _chronosOverride, _priority, this, out ChronosHandler _handler);
            SetOverride(_override);

            return _handler;
        }

        /// <inheritdoc cref="ChronosWrapper.PushOverride"/>
        public ChronosHandler PushOverride(int _id, float _chronosOverride, int _priority, float _duration, bool _realTime = true, Action _onComplete = null) {

            float _override = chronosWrapper.PushOverride(_id, _chronosOverride, _priority, this, out ChronosHandler _handler, _duration, _realTime, _onComplete);
            SetOverride(_override);

            return _handler;
        }

        /// <inheritdoc cref="ChronosWrapper.PopOverride"/>
        public void PopOverride(int _id, bool _withCallback = true) {
            chronosWrapper.PopOverride(_id, _withCallback);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private void SetOverride(float _override) {
            chronosOverride = _override;
            RefreshChronos();
        }
        #endregion

        #region General
        /// <inheritdoc/>
        void IChronosModifierController.OnStoppedChronos(ChronosModifier _chronos, ChronosModifier.Type _type) {

            float _value = chronosWrapper.OnStoppedChronos(_chronos, _type);
            switch (_type) {

                // Override.
                case ChronosModifier.Type.Override:
                    SetOverride(_value);
                    break;

                // Coefficient.
                case ChronosModifier.Type.Coefficient:
                    SetCoefficient(_value);
                    break;

                // Ignore.
                case ChronosModifier.Type.None:
                default:
                    break;
            }
        }

        /// <summary>
        /// Refreshes this object chronos.
        /// </summary>
        private void RefreshChronos() {
            float _chronos = chronosOverride * chronosCoefficient;
            Chronos = _chronos;

            ref EnhancedBehaviour[] _behaviourSpan = ref behaviours;
            for (int i = _behaviourSpan.Length; i-- > 0;) {
                _behaviourSpan[i].Chronos = _chronos;
            }

            ref Animator[] _animatorSpan = ref animators;
            for (int i = _animatorSpan.Length; i-- > 0;) {
                _animatorSpan[i].speed = _chronos;
            }
        }
        #endregion
    }
}
