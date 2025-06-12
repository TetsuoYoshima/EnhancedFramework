// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using UnityEngine;

#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

using Min = EnhancedEditor.MinAttribute;

namespace EnhancedFramework.Core {
    // ===== Base ===== \\
    
    /// <summary>
    /// Base class for ease/curve values.
    /// <br/> Prefer using <see cref="EaseValue"/>/<see cref="CurveValue"/> instead of this.
    /// </summary>
    [Serializable]
    public abstract class EaseCurveValue {
        #region Global Members
        /// <summary>
        /// Range of the curve (minimum and maximum values).
        /// </summary>
        [Tooltip("Minimum and maximum values of this curve")]
        public Vector2 Range = new Vector2(0f, 1f);

        /// <summary>
        /// Duration of the curve.
        /// </summary>
        [Tooltip("Total duration of this curve")]
        [Enhanced, Min(.001f)] public float Duration = 1f;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        protected EaseCurveValue() { }

        protected EaseCurveValue(Vector2 _range, float _duration) {
            Range    = _range;
            Duration = _duration;
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Continue evaluating this value by increasing its current time.
        /// </summary>
        /// <param name="_time">Reference time value to increase.</param>
        /// <param name="_increase">Value time increase (in seconds).</param>
        /// <returns>This value at the current evaluation time.</returns>
        public float EvaluateContinue(ref float _time, float _increase, float _coef = 1f) {
            float _duration = Duration * _coef;

            _time = Mathf.Clamp(_time + _increase, 0f, _duration);
            return DoEvaluate(_time / _duration, _coef);
        }

        /// <summary>
        /// Evaluates this value at a specific time.
        /// </summary>
        /// <param name="_time">Time to evaluate this value at.</param>
        /// <returns>This value at the given time.</returns>
        public float Evaluate(float _time, float _coef = 1f) {
            float _duration = Duration * _coef;
            return DoEvaluate(_time / _duration, _coef);
        }

        /// <summary>
        /// Evaluates this curve value at a specific percentage (from 0 to 1).
        /// </summary>
        /// <param name="_percent">Lifetime percentage to evaluate the curve at.</param>
        /// <returns>The curve value at the given percentage.</returns>
        public float EvaluatePercent(float _percent, float _coef = 1f) {
            return DoEvaluate(_percent, _coef);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <summary>
        /// Use this to implement this value evaluation behaviour.
        /// </summary>
        /// <param name="_percent">Lifetime percentage to evaluate the curve at.</param>
        protected abstract float DoEvaluate(float _percent, float _coef);
        #endregion

        #region Utility
        /// <summary>
        /// Get the current evaluation time ratio (between 0 and 1) of this value.
        /// </summary>
        /// <param name="_time">Time to get the associated ratio.</param>
        public float GetTimeRatio(float _time, float _coef = 1f) {
            float _duration = Duration / _coef;
            return _time / _duration;
        }

        /// <summary>
        /// Resets this value back to its first value.
        /// </summary>
        /// <returns>This value first default value.</returns>
        public float Reset() {
            return DoEvaluate(0f, 1f);
        }
        #endregion
    }

    // ===== Derived ===== \\

    #if DOTWEEN_ENABLED
    /// <summary>
    /// Wrapper utility class for an ease curve type value.
    /// </summary>
    [Serializable]
    public sealed class EaseValue : EaseCurveValue {
        #region Content
        /// <summary>
        /// Ease used to evaluate this value.
        /// </summary>
        [Tooltip("Ease used to evaluate values")]
        public Ease Ease = Ease.OutSine;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        public EaseValue() { }

        public EaseValue(Vector2 _range, float _duration, Ease _ease) : base(_range, _duration) {
            Ease = _ease;
        }
        #endregion

        #region Behaviour
        protected override float DoEvaluate(float _percent, float _coef) {
            return DOVirtual.EasedValue(Range.x, Range.y * _coef, _percent, Ease);
        }
        #endregion
    }
    #endif

    /// <summary>
    /// Wrapper utility class for an animation curve type value.
    /// </summary>
    [Serializable]
    public class CurveValue : EaseCurveValue {
        #region Content
        /// <summary>
        /// Animation curve used to evaluate this value.
        /// </summary>
        [Tooltip("Curve used to evaluate values")]
        [Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Lime)] public AnimationCurve Curve = AnimationCurve.Constant(0f, 1f, 0f);

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        public CurveValue() { }

        public CurveValue(Vector2 _range, float _duration, AnimationCurve _curve) : base(_range, _duration) {
            Curve = _curve;
        }
        #endregion

        #region Behaviour
        protected override float DoEvaluate(float _percent, float _coef) {
            return Mathf.Lerp(Range.x, Range.y * _coef, Curve.Evaluate(_percent));
        }
        #endregion
    }

    /// <summary>
    /// <inheritdoc cref="CurveValue"/>
    /// <para/> Uses an additional curve to decrease its value.
    /// </summary>
    [Serializable]
    public sealed class AdvancedCurveValue : CurveValue {
        #region Decrease Wrapper
        /// <summary>
        /// Wrapper for object instance decrease values.
        /// </summary>
        public class DecreaseWrapper {
            public float Duration = 0f;
            public float From = 0f;
            public float Time = 0f;

            // -----------------------

            public float GetTime(AnimationCurve _curve) {
                return Mathf.Lerp(0f, From, _curve.Evaluate(Time / Duration));
            }

            public void Reset() {
                Time = 0f;
            }
        }
        #endregion

        #region Global Members
        /// <summary>
        /// Duration of the decrease curve.
        /// </summary>
        [Tooltip("Total duration of the decrease curve")]
        [Space(10f), Enhanced, Min(.001f)] public float DecreaseDuration = .1f;

        /// <summary>
        /// Animation curve used to decrease this value current time.
        /// </summary>
        [Tooltip("Curve used to decrease values")]
        [Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Crimson)] public AnimationCurve DecreaseCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        public AdvancedCurveValue() { }

        public AdvancedCurveValue(Vector2 _range, float _duration, AnimationCurve _curve) : base(_range, _duration, _curve) { }
        #endregion

        #region Behaviour
        /// <summary>
        /// Decreases this value, using a specific decrease curve.
        /// </summary>
        /// <param name="_decrease">Value time decrease (in seconds).
        /// <br/> Must be positive.</param>
        /// <returns>The curve value with applied time decrease.</returns>
        public float Decrease(ref float _time, float _decrease, DecreaseWrapper _wrapper, float _coef = 1f) {

            if (_wrapper.Time == 0f) {
                _wrapper.Duration = Mathf.Max(DecreaseDuration * GetTimeRatio(_time), .01f);
                _wrapper.From = _time;
            }

            _wrapper.Time = Mathf.Clamp(_wrapper.Time + _decrease, 0f, _wrapper.Duration);

            float _duration = Duration * _coef;
            _time = _wrapper.GetTime(DecreaseCurve);

            return DoEvaluate(_time / _duration, _coef);
        }
        #endregion
    }
}
