// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using EnhancedFramework.Core.GameStates;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedFramework.UI {
    /// <summary>
    /// <see cref="EnhancedBehaviour"/> UI class used to manage the black bars displayed at the top and bottom of the screen. 
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "UI/Special/Black Bars"), DisallowMultipleComponent]
    public sealed class BlackBarsFadingGroup : FadingObjectSingleton<BlackBarsFadingGroup>, IGameStateOverrideCallback {
        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            GameStateManager.Instance.RegisterOverrideCallback(this);
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            GameStateManager.Instance.UnregisterOverrideCallback(this);
        }
        #endregion

        #region Game State
        private const int GameStateID = 774395153;

        // -----------------------

        void IGameStateOverrideCallback.OnGameStateOverride(in GameStateOverride _state) {
            if (_state is DefaultGameStateOverride _override) {
                SetVisibilityBuffer(GameStateID, _override.ShowBlackBars);
            }
        }
        #endregion

        #region Buffer
        private static readonly Set<int> visibilityBuffer = new Set<int>();

        private Action<bool> onFadeBufferShow = null;
        private Action onFadeBufferWait = null;

        private Action<bool> onFadeBufferCompleteCallback = null;
        private Action onFadeBufferBeforeFadeOutCallback  = null;
        private Action onFadeBufferAfterFadeInCallback    = null;

        // -----------------------

        /// <summary>
        /// Shows the screen black bars using a buffer system. Hide can only be performed when the buffer is empty.
        /// <br/> Use the same id to hide it using <see cref="HideBuffer(int, bool, Action)"/>.
        /// </summary>
        /// <param name="_id">This operation identifier to push in buffer.</param>
        /// <inheritdoc cref="FadingObjectSingleton{T}.Show(bool, Action)"/>
        public void ShowBuffer(int _id, bool _instant = false, Action<bool> _onComplete = null) {
            visibilityBuffer.Add(_id);
            base.Show(_instant, _onComplete);
        }

        /// <summary>
        /// Removes this identifier from the buffer and hide the screen black bars if it is empty.
        /// <br/> Use the same id that was used with <see cref="ShowBuffer(int, bool, Action)"/>.
        /// </summary>
        /// <param name="_id">This operation identifier to pop from the buffer.</param>
        /// <inheritdoc cref="FadingObjectSingleton{T}.Hide(bool, Action)"/>
        public void HideBuffer(int _id, bool _instant = false, Action<bool> _onComplete = null) {
            visibilityBuffer.Remove(_id);

            if (visibilityBuffer.Count == 0) {
                base.Hide(_instant, _onComplete);
            } else {
                _onComplete?.Invoke(true);
            }
        }

        /// <summary>
        /// Set this buffer visibility using the priority system.
        /// </summary>
        /// <param name="_id">This operation identifier to manage in buffer.</param>
        /// <inheritdoc cref="FadingObjectSingleton{T}.SetVisibility(bool, bool, Action)"/>
        public void SetVisibilityBuffer(int _id, bool _isVisible, bool _instant = false, Action<bool> _onComplete = null) {
            if (_isVisible) {
                ShowBuffer(_id, _instant, _onComplete);
            } else {
                HideBuffer(_id, _instant, _onComplete);
            }
        }

        /// <summary>
        /// Fades the black bars using the priority system.
        /// </summary>
        /// <param name="_id">This operation identifier to manage in buffer.</param>
        /// <inheritdoc cref="FadingObjectSingleton{T}.Fade(FadingMode, bool, Action, float)"/>
        public void FadeBuffer(int _id, FadingMode _mode, bool _isInstant, Action<bool> _onComplete = null, float _inOutWaitDuration = .5f) {

            switch (_mode) {
                case FadingMode.Show:
                    ShowBuffer(_id, _isInstant, _onComplete);
                    break;

                case FadingMode.Hide:
                    HideBuffer(_id, _isInstant, _onComplete);
                    break;

                case FadingMode.FadeInOut:
                    if (_isInstant) {
                        ShowBuffer(_id, _isInstant, _onComplete);
                        HideBuffer(_id, _isInstant, _onComplete);
                    } else {
                        FadeInOutBuffer(_id, _inOutWaitDuration, null, null, _onComplete);
                    }
                    break;

                case FadingMode.None:
                default:
                    break;
            }
        }

        /// <summary>
        /// Fades in out the black bars using the priority system.
        /// </summary>
        /// <param name="_id">This operation identifier to manage in buffer.</param>
        /// <inheritdoc cref="FadingObjectSingleton{T}.FadeInOut(float, Action, Action, Action)"/>
        public void FadeInOutBuffer(int _id, float _duration, Action _onAfterFadeIn = null, Action _onBeforeFadeOut = null, Action<bool> _onComplete = null) {

            CancelCurrentFade();

            onFadeBufferBeforeFadeOutCallback = _onBeforeFadeOut;
            onFadeBufferAfterFadeInCallback   = _onAfterFadeIn;
            onFadeBufferCompleteCallback      = _onComplete;

            onFadeBufferShow ??= OnShow;
            ShowBuffer(_id, false, onFadeBufferShow);

            // ----- Local Methods ----- \\

            void OnShow(bool _completed) {
                onFadeBufferAfterFadeInCallback?.Invoke();

                onFadeBufferWait ??= OnWaitComplete;
                Delayer.Call(_duration, onFadeBufferWait, true);
            }

            void OnWaitComplete() {
                onFadeBufferBeforeFadeOutCallback?.Invoke();
                HideBuffer(_id, false, onFadeBufferCompleteCallback);
            }
        }

        /// <summary>
        /// Clears this buffer content and hide the black bars.
        /// </summary>
        /// <inheritdoc cref="FadingObjectSingleton{T}.Hide(bool, Action)"/>
        public void ClearBuffer(bool _instant, Action<bool> _onComplete = null) {
            visibilityBuffer.Clear();
            base.Hide(_instant, _onComplete);
        }
        #endregion

        #region Visiblity
        private const int DefaultID = 0;

        // -------------------------------------------
        // General
        // -------------------------------------------

        public override void Show(Action<bool> _onComplete = null) {
            Show(false, _onComplete);
        }

        public override void Hide(Action<bool> _onComplete = null) {
            Hide(false, _onComplete);
        }

        public override void FadeInOut(float _duration, Action _onAfterFadeIn = null, Action _onBeforeFadeOut = null, Action<bool> _onComplete = null) {
            FadeInOutBuffer(DefaultID, _duration, _onAfterFadeIn, _onBeforeFadeOut, _onComplete);
        }

        public override void Fade(FadingMode _mode, Action<bool> _onComplete = null, float _inOutWaitDuration = .5f) {
            Fade(_mode, false, _onComplete, _inOutWaitDuration);
        }

        public override void Invert(Action<bool> _onComplete = null) {
            Invert(false, _onComplete);
        }

        public override void SetVisibility(bool _isVisible, Action<bool> _onComplete = null) {
            SetVisibility(_isVisible, false, _onComplete);
        }

        // -------------------------------------------
        // Instant
        // -------------------------------------------

        public override void Show(bool _isInstant, Action<bool> _onComplete = null) {
            ShowBuffer(DefaultID, _isInstant, _onComplete);
        }

        public override void Hide(bool _isInstant, Action<bool> _onComplete = null) {
            HideBuffer(DefaultID, _isInstant, _onComplete);
        }

        public override void Fade(FadingMode _mode, bool _isInstant, Action<bool> _onComplete = null, float _inOutWaitDuration = .5f) {
            FadeBuffer(DefaultID, _mode, _isInstant, _onComplete, _inOutWaitDuration);
        }

        public override void Invert(bool _isInstant, Action<bool> _onComplete = null) {
            if (IsVisible) {
                HideBuffer(DefaultID, _isInstant, _onComplete);
            } else {
                ShowBuffer(DefaultID, _isInstant, _onComplete);
            }
        }

        public override void SetVisibility(bool _isVisible, bool _isInstant, Action<bool> _onComplete = null) {
            SetVisibilityBuffer(DefaultID, _isVisible, false, _onComplete);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        public override void Evaluate(float _time, bool _show) {

            ref List<int> _span = ref visibilityBuffer.collection;
            int _count = _span.Count;

            if ((_count > 1) || ((_count == 1) && (_span[0] != DefaultID))) {
                return;
            }

            base.Evaluate(_time, _show);
        }

        public override void SetFadeValue(float _value, bool _show) {

            ref List<int> _span = ref visibilityBuffer.collection;
            int _count = _span.Count;

            if ((_count > 1) || ((_count == 1) && (_span[0] != DefaultID))) {
                return;
            }

            base.SetFadeValue(_value, _show);
        }
        #endregion
    }
}
