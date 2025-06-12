// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;

namespace EnhancedFramework.UI {
    /// <summary>
    /// Inherit from this to quickly implement <see cref="EnhancedSingleton{T}"/>-encapsulated <see cref="FadingObjectBehaviour"/>.
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="EnhancedSingleton{T}" path="/typeparam[@name='T']"/></typeparam>
    public abstract class FadingObjectSingleton<T> : EnhancedSingleton<T>, IFadingObject where T : FadingObjectSingleton<T> {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Global Members
        [Section("Fading Group")]

        [Tooltip("FadingObject associated with this singleton")]
        [SerializeField, Enhanced, Required] protected FadingObjectBehaviour fadingObject = null;

        [Tooltip("Fading mode applied on this behaviour init")]
        [SerializeField] protected FadingMode initMode = FadingMode.Hide;

        // -----------------------

        public virtual bool IsVisible {
            get { return fadingObject.IsVisible; }
        }

        public virtual float ShowDuration {
            get { return fadingObject.ShowDuration; }
        }

        public virtual float HideDuration {
            get { return fadingObject.HideDuration; }
        }

        /// <summary>
        /// The <see cref="FadingObjectBehaviour"/> of this singleton.
        /// </summary>
        public FadingObjectBehaviour FadingObject {
            get { return fadingObject; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnInit() {
            base.OnInit();

            fadingObject.Fade(initMode);
        }
        #endregion

        #region Visiblity
        // -------------------------------------------
        // General
        // -------------------------------------------

        public virtual void Show(Action<bool> _onComplete = null) {
            fadingObject.Show(_onComplete);
        }

        public virtual void Hide(Action<bool> _onComplete = null) {
            fadingObject.Hide(_onComplete);
        }

        public virtual void FadeInOut(float _duration, Action _onAfterFadeIn = null, Action _onBeforeFadeOut = null, Action<bool> _onComplete = null) {
            fadingObject.FadeInOut(_duration, _onAfterFadeIn, _onBeforeFadeOut, _onComplete);
        }

        public virtual void Fade(FadingMode _mode, Action<bool> _onComplete = null, float _inOutWaitDuration = .5f) {
            fadingObject.Fade(_mode, _onComplete, _inOutWaitDuration);
        }

        public virtual void Invert(Action<bool> _onComplete = null) {
            fadingObject.Invert(_onComplete);
        }

        public virtual void SetVisibility(bool _isVisible, Action<bool> _onComplete = null) {
            fadingObject.SetVisibility(_isVisible, _onComplete);
        }

        // -------------------------------------------
        // Instant
        // -------------------------------------------

        public virtual void Show(bool _isInstant, Action<bool> _onComplete = null) {
            fadingObject.Show(_isInstant, _onComplete);
        }

        public virtual void Hide(bool _isInstant, Action<bool> _onComplete = null) {
            fadingObject.Hide(_isInstant, _onComplete);
        }

        public virtual void Fade(FadingMode _mode, bool _isInstant, Action<bool> _onComplete = null, float _inOutWaitDuration = .5f) {
            fadingObject.Fade(_mode, _isInstant, _onComplete, _inOutWaitDuration);
        }

        public virtual void Invert(bool _isInstant, Action<bool> _onComplete = null) {
            fadingObject.Invert(_isInstant, _onComplete);
        }

        public virtual void SetVisibility(bool _isVisible, bool _isInstant, Action<bool> _onComplete = null) {
            fadingObject.SetVisibility(_isVisible, _isInstant, _onComplete);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        public virtual void Evaluate(float _time, bool _show) {
            fadingObject.Evaluate(_time, _show);
        }

        public virtual void SetFadeValue(float _value, bool _show) {
            fadingObject.SetFadeValue(_value, _show);
        }

        public virtual void CancelCurrentFade() {
            fadingObject.CancelCurrentFade();
        }

        // -------------------------------------------
        // Inspector
        // -------------------------------------------

        [Button(SuperColor.Green)]
        protected void ShowGroup() {
            Show();
        }

        [Button(SuperColor.Crimson)]
        protected void HideGroup() {
            Hide();
        }
        #endregion
    }
}
