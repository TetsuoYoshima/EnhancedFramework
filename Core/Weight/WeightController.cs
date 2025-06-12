// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="WeightController{T}"/>-related weight fade interface.
    /// </summary>
    public interface IWeightControl {
        #region Content
        /// <summary>
        /// Duration used for fading in this object (0 for instant).
        /// </summary>
        public float FadeInDuration { get; }

        /// <summary>
        /// Duration used for fading out this object (0 for instant).
        /// </summary>
        public float FadeOutDuration { get; }

        /// <summary>
        /// Curve used for fading in this object.
        /// </summary>
        public AnimationCurve FadeInCurve { get; }

        /// <summary>
        /// Curve used for fading out this object.
        /// </summary>
        public AnimationCurve FadeOutCurve { get; }
        #endregion
    }

    /// <summary>
    /// <see cref="WeightController{T}"/>-related wrapper for a single control operation.
    /// </summary>
    public struct WeightHandler<T> : IHandler<WeightController<T>> where T : IWeightControl {
        #region Global Members
        private Handler<WeightController<T>> handler;

        // -----------------------

        public readonly int ID {
            get { return handler.ID; }
        }

        public bool IsValid {
            get { return GetHandle(out _); }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="TweenHandler(EnhancedTween, int)"/>
        public WeightHandler(WeightController<T> _controller) {
            handler = new Handler<WeightController<T>>(_controller);
        }

        /// <param name="_controller"><see cref="WeightController{T}"/> to handle.</param>
        /// <param name="_id">ID of the associated control operation.</param>
        /// <inheritdoc cref="WeightController{T}"/>
        public WeightHandler(WeightController<T> _controller, int _id) {
            handler = new Handler<WeightController<T>>(_controller, _id);
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="IHandler{T}.GetHandle(out T)"/>
        public bool GetHandle(out WeightController<T> _controller) {
            return handler.GetHandle(out _controller) && (_controller.Status != WeightController<T>.State.Inactive);
        }

        /// <summary>
        /// Pauses this handler associated control operation.
        /// </summary>
        public bool Pause() {
            if (GetHandle(out WeightController<T> _controller)) {
                _controller.Pause();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resumes this handler associated control operation.
        /// </summary>
        public bool Resume() {
            if (GetHandle(out WeightController<T> _controller)) {
                _controller.Resume();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops this handler associated control operation.
        /// </summary>
        /// <inheritdoc cref="WeightController{T}.Stop(bool, Action{bool})"/>
        public bool Stop(bool _instant = false, Action<bool> _onComplete = null) {
            if (GetHandle(out WeightController<T> _controller)) {
                _controller.Stop(_instant, _onComplete);
                return true;
            }

            _onComplete?.Invoke(true);
            return false;
        }

        /// <summary>
        /// Sets this handler associated target weight.
        /// </summary>
        /// <inheritdoc cref="WeightController{T}.SetWeight(float, float)"/>
        public bool SetWeight(float _weight, float _transitionDuration) {
            if (GetHandle(out WeightController<T> _controller)) {
                _controller.SetWeight(_weight, _transitionDuration);
                return true;
            }

            return false;
        }
        #endregion
    }

    /// <summary>
    /// Utility class used to control the weight of an object.
    /// </summary>
    /// <typeparam name="T">Controlled object type</typeparam>
    public class WeightController<T> : IHandle where T : IWeightControl {
        #region State
        /// <summary>
        /// References all available states for an <see cref="WeightController{T}"/>.
        /// </summary>
        public enum State {
            Inactive    = 0,

            Active      = 1,
            Paused      = 2,
        }
        #endregion

        #region Global Members
        private State state = State.Inactive;
        private int id = 0;

        private T blendObject = default;

        private float fadeWeight = 0f;
        private float weight = 0f;

        // -----------------------

        /// <inheritdoc cref="IHandle.ID"/>
        public int ID {
            get { return id; }
        }

        /// <summary>
        /// Current state of this controller.
        /// </summary>
        public State Status {
            get { return state;}
        }

        /// <summary>
        /// This controller wrapped object.
        /// </summary>
        public T Object {
            get { return blendObject; }
        }

        /// <summary>
        /// This controller weight (1 if completely active, 0 for inactive).
        /// </summary>
        public float Weight {
            get { return weight * fadeWeight; }
        }

        /// <summary>
        /// Get if this controller is currently fading its weight value.
        /// </summary>
        public bool IsFading {
            get { return fadeTween.IsValid || weightTween.IsValid; }
        }
        #endregion

        #region Controller
        private static int lastID = 0;

        private Action<float> fadeWeightSetter = null;
        private Action<float> weightSetter     = null;

        private Action<bool> onFadeOutComplete = null;
        private Action<bool> onStopped         = null;

        private Action<bool> onFadeOutCallback = null;
        private Action<bool> onStoppedCallback = null;

        private TweenHandler weightTween = default;
        private TweenHandler fadeTween   = default;

        private int fadeState = 0;

        // -----------------------

        /// <summary>
        /// Initializes this controller for a specific object.
        /// </summary>
        /// <param name="_object">Object to wrap within this controller.</param>
        /// <param name="_weight">Initial weight of this object (between 1 and 0).</param>
        /// <param name="_instant"><inheritdoc cref="FadeIn" path="/param[@name='_instant']"/></param>
        public WeightHandler<T> Initialize(T _object, float _weight, bool _instant) {

            Stop(true);

            blendObject = _object;

            fadeWeight = 0f;
            weight     = Mathf.Clamp01(_weight);

            SetState(State.Active);
            FadeIn(_instant);

            id = ++lastID;
            return new WeightHandler<T>(this, id);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Pauses this object weight control operations.
        /// </summary>
        public void Pause() {
            
            // Ignore if not active.
            if (state != State.Active) {
                return;
            }

            weightTween.Pause();
            fadeTween  .Pause();

            SetState(State.Paused);
        }

        /// <summary>
        /// Resumes this object weight control operations.
        /// </summary>
        public void Resume() {

            // Ignore if not active.
            if (state != State.Paused) {
                return;
            }

            weightTween.Resume();
            fadeTween  .Resume();

            SetState(State.Active);
        }

        /// <summary>
        /// Stops this controller operations and resets its state back to default.
        /// </summary>
        /// <param name="_instant">If true, instantly fades out this object.</param>
        /// <param name="_onComplete"><inheritdoc cref="FadeIn(bool, Action{bool})" path="/param[@name='_onComplete']"/></param>
        public void Stop(bool _instant = false, Action<bool> _onComplete = null) {

            // Ignore if already inactive.
            if (state == State.Inactive) {
                return;
            }

            onStopped ??= OnComplete;
            onStoppedCallback = _onComplete;

            FadeOut(_instant, onStopped);

            // ----- Local Method ----- \\

            void OnComplete(bool _success) {

                if (_success) {

                    weightTween.Stop();
                    fadeTween  .Stop();

                    id = 0;
                    SetState(State.Inactive);
                }

                onStoppedCallback?.Invoke(_success);
            }
        }

        /// <summary>
        /// Sets this object target weight.
        /// </summary>
        /// <param name="_weight">Target weight of this object.</param>
        /// <param name="_transitionDuration">Weight transition duration (in seconds).</param>
        public void SetWeight(float _weight, float _transitionDuration) {

            _weight = Mathf.Clamp01(_weight);

            if (_weight == 0f) {
                FadeOut();
                return;
            }

            // If fading out, fade in.
            if ((fadeState == -1) || (fadeWeight == 0f)) {
                FadeIn();
            }

            weightSetter ??= SetWeight;

            weightTween.Stop();
            weightTween = Tweener.Tween(weight, _weight, weightSetter, _transitionDuration, true);

            // ----- Local Method ----- \\

            void SetWeight(float _weight) {
                weight = _weight;
            }
        }

        // -------------------------------------------
        // Fade
        // -------------------------------------------

        /// <summary>
        /// Fades in this object weight.
        /// </summary>
        /// <param name="_instant">If true, instantly fades in this object.</param>
        /// <param name="_onComplete">Called when this fade is completed, with a boolean indicating if it was successfully completed or not.</param>
        public void FadeIn(bool _instant = false, Action<bool> _onComplete = null) {
            float _duration = _instant ? 0f : blendObject.FadeInDuration;

            fadeState = 1;
            Fade(fadeWeight, 1f, _duration, blendObject.FadeInCurve, _onComplete);
        }

        /// <summary>
        /// Fades out this object weight.
        /// </summary>
        /// <param name="_instant">If true, instantly fades out this object.</param>
        /// <param name="_onComplete"><inheritdoc cref="FadeIn(bool, Action{bool})" path="/param[@name='_onComplete']"/></param>
        public void FadeOut(bool _instant = false, Action<bool> _onComplete = null) {
            float _duration = _instant ? 0f : blendObject.FadeOutDuration;

            fadeState = -1;
            Fade(0f, fadeWeight, _duration, blendObject.FadeOutCurve, _onComplete);

            weightTween.Stop();
        }

        // -----------------------

        private void Fade(float _min, float _max, float _duration, AnimationCurve _curve, Action<bool> _onComplete = null) {

            if (fadeWeightSetter == null) {

                fadeWeightSetter  = SetWeight;
                onFadeOutComplete = OnComplete;
            }
            
            fadeTween.Stop();

            onFadeOutCallback = _onComplete;
            fadeTween = Tweener.Tween(_min, _max, fadeWeightSetter, _duration, _curve, true, onFadeOutComplete);

            // ----- Local Methods ----- \\

            void SetWeight(float _weight) {
                fadeWeight = _weight;
            }

            void OnComplete(bool _success) {
                fadeState = 0;
                onFadeOutCallback?.Invoke(_success);
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Sets the state of this object.
        /// </summary>
        /// <param name="_state">New state of this object.</param>
        private void SetState(State _state) {
            state = _state;
        }
        #endregion
    }
}
