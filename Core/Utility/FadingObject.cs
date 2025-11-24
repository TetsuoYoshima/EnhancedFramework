// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using UnityEngine.UI;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="IFadingObject"/>-related fading mode.
    /// </summary>
    public enum FadingMode {
        None = 0,

        [Separator(SeparatorPosition.Top)]

        Show        = 1,
        Hide        = 2,
        FadeInOut   = 3,
    }

    /// <summary>
    /// Base interface to inherit any fading object from.
    /// </summary>
    public interface IFadingObject {
        #region Content
        /// <summary>
        /// Indicates whether this object is currently visible or not.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Duration used to fade in this object.
        /// </summary>
        float ShowDuration { get; }

        /// <summary>
        /// Duration used to fade out this object.
        /// </summary>
        float HideDuration { get; }

        // -------------------------------------------
        // General
        // -------------------------------------------

        /// <summary>
        /// Fades in this object and show it.
        /// </summary>
        /// <param name="_onComplete">Called once this object fade is completed - parameter indicates if the operation was successfully completed or prematuraly stopped (canceled).</param>
        void Show(Action<bool> _onComplete = null);

        /// <summary>
        /// Fades out this object and hide it.
        /// </summary>
        /// <param name="_onComplete"><inheritdoc cref="Show(Action{bool})" path="/param[@name='_onComplete']"/></param>
        void Hide(Action<bool> _onComplete = null);

        /// <summary>
        /// Fades in this object and show it, then wait for a given time, and fade out.
        /// </summary>
        /// <param name="_duration">The duration to wait before fading out.</param>
        /// <param name="_onAfterFadeIn">Called right after fading int.</param>
        /// <param name="_onBeforeFadeOut">Called right before fading out.</param>
        /// <param name="_onComplete"><inheritdoc cref="Show(Action{bool})" path="/param[@name='_onComplete']"/></param>
        void FadeInOut(float _duration, Action _onAfterFadeIn = null, Action _onBeforeFadeOut = null, Action<bool> _onComplete = null);

        /// <summary>
        /// Fades this object according to a given <see cref="FadingMode"/>.
        /// </summary>
        /// <param name="_mode">The <see cref="FadingMode"/> used to fade this object.</param>
        /// <param name="_onComplete"><inheritdoc cref="Show(Action{bool})" path="/param[@name='_onComplete']"/></param>
        /// <param name="_inOutWaitDuration">The duration to wait before fading out if using <see cref="FadingMode.FadeInOut"/>.</param>
        void Fade(FadingMode _mode, Action<bool> _onComplete = null, float _inOutWaitDuration = .5f);

        /// <summary>
        /// Inverts this object visibility (show it if hidden, hide if if visible).
        /// </summary>
        /// <param name="_onComplete"><inheritdoc cref="Show(Action{bool})" path="/param[@name='_onComplete']"/></param>
        void Invert(Action<bool> _onComplete = null);

        /// <summary>
        /// Sets this object visibility and show/hide it accordingly.
        /// </summary>
        /// <param name="_isVisible">Should this object be visible?</param>
        /// <param name="_onComplete"><inheritdoc cref="Show(Action{bool})" path="/param[@name='_onComplete']"/></param>
        void SetVisibility(bool _isVisible, Action<bool> _onComplete = null);

        // -------------------------------------------
        // Instant
        // -------------------------------------------

        /// <param name="_isInstant">If true, instantly fades this object.</param>
        /// <inheritdoc cref="Show(Action{bool})"/>
        void Show(bool _isInstant, Action<bool> _onComplete = null);

        /// <param name="_isInstant"><inheritdoc cref="Show(bool, Action{bool})" path="/param[@name='_isInstant']"/></param>
        /// <inheritdoc cref="Hide(Action{bool})"/>
        void Hide(bool _isInstant, Action<bool> _onComplete = null);

        /// <param name="_isInstant"><inheritdoc cref="Show(bool, Action{bool})" path="/param[@name='_isInstant']"/></param>
        /// <inheritdoc cref="Fade(FadingMode, Action{bool}, float)"/>
        void Fade(FadingMode _mode, bool _isInstant, Action<bool> _onComplete = null, float _inOutWaitDuration = .5f);

        /// <param name="_isInstant"><inheritdoc cref="Show(bool, Action{bool})" path="/param[@name='_isInstant']"/></param>
        /// <inheritdoc cref="Invert(Action{bool})"/>
        void Invert(bool _isInstant, Action<bool> _onComplete = null);

        /// <param name="_isInstant"><inheritdoc cref="Show(bool, Action{bool})" path="/param[@name='_isInstant']"/></param>
        /// <inheritdoc cref="SetVisibility(bool, Action{bool})"/>
        void SetVisibility(bool _isVisible, bool _isInstant, Action<bool> _onComplete = null);

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Evaluates this fading object at a specific time.
        /// </summary>
        /// <param name="_time">Time to evaluate this fade object at.</param>
        /// <param name="_show">Whether to show or hide this object.</param>
        void Evaluate(float _time, bool _show);

        /// <summary>
        /// Sets the fade value of this oject.
        /// </summary>
        /// <param name="_value">Normalized fade value (between 0 and 1).</param>
        /// <param name="_show">Whether to show or hide this object.</param>
        void SetFadeValue(float _value, bool _show);

        /// <summary>
        /// Cancels this object current fading operation.
        /// </summary>
        void CancelCurrentFade();
        #endregion
    }

    /// <summary>
    /// Base class to inherit your own <see cref="IFadingObject"/> from.
    /// </summary>
    public abstract class FadingObject : IFadingObject {
        #region Global Members
        /// <inheritdoc/>
        public abstract float ShowDuration { get; }

        /// <inheritdoc/>
        public abstract float HideDuration { get; }

        /// <inheritdoc/>
        public abstract bool IsVisible { get; }

        /// <summary>
        /// If true, fading will not be affected by time scale.
        /// </summary>
        public abstract bool RealTime { get; }
        #endregion

        #region Behaviour
        private Action<bool> onFadeInOutComplete = null;
        private Action onBeforeFadeOut           = null;
        private Action onAfterFadeIn             = null;
        private float fadeInOutDuration          = 0f;

        private Action<bool> onFadeInOutShow = null;
        private Action onFadeInOutWait       = null;

        protected DelayHandler delay  = default;
        protected bool hasDelayFailed = false;

        // -------------------------------------------
        // General
        // -------------------------------------------

        /// <inheritdoc/>
        public virtual void Show(Action<bool> _onComplete = null) {
            OnShow(false, _onComplete);
        }

        /// <inheritdoc/>
        public virtual void Hide(Action<bool> _onComplete = null) {
            OnHide(false, _onComplete);
        }

        /// <inheritdoc/>
        public virtual void FadeInOut(float _duration, Action _onAfterFadeIn = null, Action _onBeforeFadeOut = null, Action<bool> _onComplete = null) {
            CancelCurrentFade();

            fadeInOutDuration   = _duration;
            onFadeInOutComplete = _onComplete;
            onBeforeFadeOut     = _onBeforeFadeOut;
            onAfterFadeIn       = _onAfterFadeIn;

            onFadeInOutShow ??= OnShow;
            Show(onFadeInOutShow);

            // ----- Local Methods ----- \\

            void OnShow(bool _completed) {

                // Failed.
                if (!_completed) {
                    onFadeInOutComplete?.Invoke(false);
                    return;
                }

                onAfterFadeIn?.Invoke();
                hasDelayFailed = false;

                onFadeInOutWait ??= OnWaitComplete;
                delay = Delayer.Call(fadeInOutDuration, onFadeInOutWait, RealTime);
            }

            void OnWaitComplete() {

                // Failed.
                if (hasDelayFailed) {
                    onFadeInOutComplete?.Invoke(false);
                    return;
                }

                onBeforeFadeOut?.Invoke();
                Hide(onFadeInOutComplete);
            }
        }

        /// <inheritdoc/>
        public virtual void Fade(FadingMode _mode, Action<bool> _onComplete = null, float _inOutWaitDuration = .5f) {
            switch (_mode) {
                case FadingMode.Show:
                    Show(_onComplete);
                    break;

                case FadingMode.Hide:
                    Hide(_onComplete);
                    break;

                case FadingMode.FadeInOut:
                    FadeInOut(_inOutWaitDuration, null, null, _onComplete);
                    break;

                case FadingMode.None:
                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public virtual void Invert(Action<bool> _onComplete = null) {
            SetVisibility(!IsVisible, _onComplete);
        }

        /// <inheritdoc/>
        public virtual void SetVisibility(bool _isVisible, Action<bool> _onComplete = null) {
            if (_isVisible) {
                Show(_onComplete);
            } else {
                Hide(_onComplete);
            }
        }

        // -------------------------------------------
        // Instant
        // -------------------------------------------

        /// <inheritdoc/>
        public virtual void Show(bool _isInstant, Action<bool> _onComplete = null) {
            OnShow(_isInstant, _onComplete);
        }

        /// <inheritdoc/>
        public virtual void Hide(bool _isInstant, Action<bool> _onComplete = null) {
            OnHide(_isInstant, _onComplete);
        }

        /// <inheritdoc/>
        public virtual void Fade(FadingMode _mode, bool _isInstant, Action<bool> _onComplete = null, float _inOutWaitDuration = .5f) {
            switch (_mode) {
                case FadingMode.Show:
                    Show(_isInstant, _onComplete);
                    break;

                case FadingMode.Hide:
                    Hide(_isInstant, _onComplete);
                    break;

                case FadingMode.FadeInOut:
                    if (_isInstant) {
                        Show(_isInstant, _onComplete);
                        Hide(_isInstant, _onComplete);
                    } else {
                        FadeInOut(_inOutWaitDuration, null, null, _onComplete);
                    }
                    break;

                case FadingMode.None:
                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public virtual void Invert(bool _isInstant, Action<bool> _onComplete = null) {
            SetVisibility(!IsVisible, _isInstant, _onComplete);
        }

        /// <inheritdoc/>
        public virtual void SetVisibility(bool _isVisible, bool _isInstant, Action<bool> _onComplete = null) {
            if (_isVisible) {
                Show(_isInstant, _onComplete);
            } else {
                Hide(_isInstant, _onComplete);
            }
        }

        // -------------------------------------------
        // Core
        // -------------------------------------------

        /// <summary>
        /// Implements here the behaviour to show this object.
        /// </summary>
        protected abstract void OnShow(bool _isInstant, Action<bool> _onComplete);

        /// <summary>
        /// Implements here the behaviour to hide this object.
        /// </summary>
        protected abstract void OnHide(bool _isInstant, Action<bool> _onComplete);

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <inheritdoc/>
        public abstract void Evaluate(float _time, bool _show);

        /// <inheritdoc/>
        public abstract void SetFadeValue(float _value, bool _show);

        /// <inheritdoc/>
        public virtual void CancelCurrentFade() {
            hasDelayFailed = true;
            delay.Complete();
        }
        #endregion
    }

    // ===== Component ===== \\

    /// <summary>
    /// Base non-generic class for <see cref="IFadingObject"/>-encapsulated <see cref="EnhancedBehaviour"/>.
    /// </summary>
    public abstract class FadingObjectBehaviour : EnhancedBehaviour, IFadingObject {
        public override UpdateRegistration UpdateRegistration {
            get {
                UpdateRegistration _value = base.UpdateRegistration;
                if (InitMode != FadingMode.None) {
                    _value |= UpdateRegistration.Init;
                }

                return _value;
            }
        }

        #region Global Members
        /// <summary>
        /// <see cref="IFadingObject"/> of this behaviour.
        /// </summary>
        public abstract IFadingObject FadingObject { get; }

        /// <summary>
        /// <see cref="FadingMode"/> applied on this object initialization.
        /// </summary>
        public abstract FadingMode InitMode { get; }

        // -----------------------

        public virtual bool IsVisible {
            get { return FadingObject.IsVisible; }
        }

        public virtual float ShowDuration {
            get { return FadingObject.ShowDuration; }
        }

        public virtual float HideDuration {
            get { return FadingObject.HideDuration; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents from inheriting this class in other assemblies.
        /// </summary>
        internal protected FadingObjectBehaviour() { }
        #endregion

        #region Enhanced Behaviour
        protected override void OnInit() {
            base.OnInit();

            FadingObject.Fade(InitMode);
        }
        #endregion

        #region Fading Object
        // -------------------------------------------
        // General
        // -------------------------------------------

        /// <inheritdoc/>
        public virtual void Show(Action<bool> _onComplete = null) {
            FadingObject.Show(_onComplete);
        }

        /// <inheritdoc/>
        public virtual void Hide(Action<bool> _onComplete = null) {
            FadingObject.Hide(_onComplete);
        }

        /// <inheritdoc/>
        public virtual void FadeInOut(float _duration, Action _onAfterFadeIn = null, Action _onBeforeFadeOut = null, Action<bool> _onComplete = null) {
            FadingObject.FadeInOut(_duration, _onAfterFadeIn, _onBeforeFadeOut, _onComplete);
        }

        /// <inheritdoc/>
        public virtual void Fade(FadingMode _mode, Action<bool> _onComplete = null, float _inOutWaitDuration = .5f) {
            FadingObject.Fade(_mode, _onComplete, _inOutWaitDuration);
        }

        /// <inheritdoc/>
        public virtual void Invert(Action<bool> _onComplete = null) {
            FadingObject.Invert(_onComplete);
        }

        /// <inheritdoc/>
        public virtual void SetVisibility(bool _isVisible, Action<bool> _onComplete = null) {
            FadingObject.SetVisibility(_isVisible, _onComplete);
        }

        // -------------------------------------------
        // Instant
        // -------------------------------------------

        /// <inheritdoc/>
        public virtual void Show(bool _isInstant, Action<bool> _onComplete = null) {
            FadingObject.Show(_isInstant, _onComplete);
        }

        /// <inheritdoc/>
        public virtual void Hide(bool _isInstant, Action<bool> _onComplete = null) {
            FadingObject.Hide(_isInstant, _onComplete);
        }

        /// <inheritdoc/>
        public virtual void Fade(FadingMode _mode, bool _isInstant, Action<bool> _onComplete = null, float _inOutWaitDuration = .5f) {
            FadingObject.Fade(_mode, _isInstant, _onComplete, _inOutWaitDuration);
        }

        /// <inheritdoc/>
        public virtual void Invert(bool _isInstant, Action<bool> _onComplete = null) {
            FadingObject.Invert(_isInstant, _onComplete);
        }

        /// <inheritdoc/>
        public virtual void SetVisibility(bool _isVisible, bool _isInstant, Action<bool> _onComplete = null) {
            FadingObject.SetVisibility(_isVisible, _isInstant, _onComplete);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <inheritdoc/>
        public virtual void Evaluate(float _time, bool _show) {
            FadingObject.Evaluate(_time, _show);
        }

        /// <inheritdoc/>
        public virtual void SetFadeValue(float _value, bool _show) {
            FadingObject.SetFadeValue(_value, _show);
        }

        /// <inheritdoc/>
        public virtual void CancelCurrentFade() {
            FadingObject.CancelCurrentFade();
        }

        // -------------------------------------------
        // Inspector
        // -------------------------------------------

        /// <summary>
        /// Shows this object on screen.
        /// </summary>
        [Button(SuperColor.Green)]
        public void ShowGroup() {
            Show();
        }

        /// <summary>
        /// Hides this object from screen.
        /// </summary>
        [Button(SuperColor.Crimson)]
        public void HideGroup() {
            Hide();
        }
        #endregion
    }
}
