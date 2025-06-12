// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if DOTWEEN_ENABLED
#define TWEENING
#endif

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

#if TWEENING
using DG.Tweening;
#endif

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.UI {
    // ===== Base ===== \\

    /// <summary>
    /// Class wrapper for a fading <see cref="CanvasGroup"/> instance.
    /// <para/> Fading is performed instantly. For a tweening behaviour, please use <see cref="TweeningFadingGroup"/>.
    /// </summary>
    [Serializable]
    public abstract class FadingGroup : FadingObject {
        // --- Enums --- \\

        #region Controller Event
        /// <summary>
        /// Lists all different <see cref="FadingGroupController"/> events to call.
        /// </summary>
        public enum ControllerEvent {
            None = 0,

            ShowStarted     = 1 << 0,
            ShowPerformed   = 1 << 1,
            ShowCompleted   = 1 << 2,

            HideStarted     = 1 << 3,
            HidePerformed   = 1 << 4,
            HideCompleted   = 1 << 5,

            CompleteShow = ShowPerformed    | ShowCompleted,
            CompleteHide = HidePerformed    | HideCompleted,

            FullShow = ShowStarted | ShowPerformed  | ShowCompleted,
            FullHide = HideStarted | HidePerformed  | CompleteHide,
        }
        #endregion

        #region State
        /// <summary>
        /// State used for a group event.
        /// </summary>
        public enum StateEvent {
            None        = 0,

            Show        = 1,
            Hide        = 2,

            ShowInstant = 3,
            HideInstant = 4,
        }
        #endregion

        #region Flags
        /// <summary>
        /// Contains a variety of parameters for this group.
        /// </summary>
        [Flags]
        public enum Parameters {
            None = 0,

            [Tooltip("Enables/disables the associated canvas on visibility toggle")]
            Canvas          = 1 << 0,

            [Tooltip("Enables/disables the group interactability on visibility toggle")]
            Interactable    = 1 << 1,

            [Tooltip("Selects a specific Selectable once visible")]
            Selectable      = 1 << 2,

            [Tooltip("Sends informations to a Fading Group Controller when updating state")]
            Controller      = 1 << 30,

            [Tooltip("Fading won't be affected by time scale")]
            UnscaledTime    = 1 << 31,
        }
        #endregion

        // --- Content --- \\

        #region Global Members
        [Tooltip("This object associated Canvas reference")]
        [Enhanced, ShowIf(nameof(UseCanvas)), Required] public Canvas Canvas = null;

        [Tooltip("This object associated Controller reference")]
        [Enhanced, ShowIf(nameof(UseController)), Required] public FadingGroupController Controller = null;

        [Tooltip("This object associated CanvasGroup reference")]
        [Enhanced, Required] public CanvasGroup Group = null;

        [Tooltip("The fading target alpha of this group: first value when fading out, second when fading in")]
        [Enhanced, MinMax(0f, 1f)] public Vector2 FadeAlpha = new Vector2(0f, 1f);

        [Tooltip("Parameters of this group")]
        [Enhanced, DisplayName("Parameters")] public Parameters GroupParameters = Parameters.Interactable | Parameters.UnscaledTime;

        [Space(10f)]

        [Tooltip("If true, disables all Selectable components in children when this group is not interactable")]
        [Enhanced, ShowIf(nameof(IsInteractable))] public bool EnableSelectable = false;

        [Tooltip("Object to first select when visible")]
        [Enhanced, ShowIf(nameof(UseSelectable)), Required] public Selectable Selectable = null;

        [Tooltip("Currently active object")]
        [Enhanced, ShowIf(nameof(UseSelectable)), ReadOnly] public Selectable ActiveSelectable = null;

        // -----------------------

        /// <inheritdoc/>
        public override float ShowDuration {
            get { return 0f; }
        }

        /// <inheritdoc/>
        public override float HideDuration {
            get { return 0f; }
        }

        /// <inheritdoc/>
        public override bool IsVisible {
            get { return Group.alpha == FadeAlpha.y; }
        }

        /// <inheritdoc/>
        public override bool RealTime {
            get { return HasParameter(Parameters.UnscaledTime); }
        }

        /// <summary>
        /// If true, use the default implementation to call controller events.
        /// </summary>
        public virtual bool UseDefaultControllerEvent {
            get { return true; }
        }

        /// <summary>
        /// If true, automatically enable/disable the associated canvas on visibility toggle.
        /// </summary>
        public bool UseCanvas {
            get { return HasParameter(Parameters.Canvas); }
        }

        /// <summary>
        /// If true, automatically sends events to a controller when updating this group state.
        /// </summary>
        public bool UseController {
            get { return HasParameter(Parameters.Controller); }
        }

        /// <summary>
        /// If true, automatically enable/disable the associated group interactability on visibility toggle.
        /// </summary>
        public bool IsInteractable {
            get { return HasParameter(Parameters.Interactable); }
        }

        /// <summary>
        /// Get if this group uses a specific selectable or not.
        /// </summary>
        public bool UseSelectable {
            get { return HasParameter(Parameters.Selectable); }
        }
        #endregion

        #region Behaviour
        private static List<Selectable> selectableBuffer = new List<Selectable>();

        // -------------------------------------------
        // Core
        // -------------------------------------------

        protected override void OnShow(bool _isInstant, Action<bool> _onComplete) {
            FadeGroup(FadeAlpha.y, _isInstant, ControllerEvent.FullShow, true, _onComplete, true);
        }

        protected override void OnHide(bool _isInstant, Action<bool> _onComplete) {
            FadeGroup(FadeAlpha.x, _isInstant, ControllerEvent.FullHide, true, _onComplete, true);
        }

        protected virtual void FadeGroup(float _value, bool _isInstant, ControllerEvent _event, bool _isEventDefault, Action<bool> _onComplete, bool _success) {
            CancelCurrentFade();

            if (Group.alpha != _value) {

                bool _isVisible = _value != FadeAlpha.x;
                bool _select    = _isVisible && !IsVisible;

                Group.alpha = _value;

                ToggleCanvas(_select);
                CallControllerEvent(_event, _isEventDefault);

                OnSetVisibility(_isVisible, _isInstant);
            }

            _onComplete?.Invoke(_success);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        public override void Evaluate(float _time, bool _show) {
            float _value = (_time == 0f) ? 0f : 1f;
            SetFadeValue(_value, _show);
        }

        public override void SetFadeValue(float _value, bool _show) {
            if (Group.alpha == _value) {
                return;
            }

            if (_value == 0f) {
                Hide();
            } else if (_value == 1f) {
                Show();
            } else {

                // Set alpha.
                CancelCurrentFade();
                Group.alpha = Mathf.Lerp(FadeAlpha.x, FadeAlpha.y, _value);

                ToggleCanvas(true);
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        protected void ToggleCanvas(bool _select = false) {
            ToggleCanvas(Group.alpha, _select);
        }

        protected void ToggleCanvas(float _value, bool _select = false) {
            bool _active = _value != FadeAlpha.x;

            EnableCanvas   (_active, _select);
            SetInteractable(_active);
        }

        public void EnableCanvas(bool _isVisible, bool _select) {
            if (UseCanvas) {
                Canvas.enabled = _isVisible;
            }

            if (UseSelectable) {

                if (_isVisible && _select) {

                    // Selection.
                    ActiveSelectable.Select();

                } else if (!_isVisible) {

                    // Reset to default.
                    ActiveSelectable = Selectable;
                }
            }
        }

        public void SetInteractable(bool _isInteractable) {
            if (!IsInteractable) {
                return;
            }

            CanvasGroup _group = Group;
            if (_group.interactable == _isInteractable) {
                return;
            }

            // Update.
            _group.interactable = _isInteractable;

            // Disable all children selectable.
            if (EnableSelectable) {

                ref List<Selectable> _span = ref selectableBuffer;
                _group.GetComponentsInChildren(_span);

                for (int i = _span.Count; i-- > 0;) {
                    _span[i].enabled = _isInteractable;
                }
            }
        }

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        public void OnDisabled() {
            CallControllerEvent(ControllerEvent.FullHide, false);
        }

        protected virtual void CallControllerEvent(ControllerEvent _event, bool _isDefault = false) {
            if ((_event == ControllerEvent.None) || !UseController || (_isDefault && !UseDefaultControllerEvent)) {
                return;
            }

            FadingGroupController _controller = Controller;

            #if UNITY_EDITOR
            if (!Application.isPlaying && !_controller.ExecuteInEditMode) {
                return;
            }
            #endif

            // Call event.
            if (HasEvent(ControllerEvent.ShowStarted)) {
                _controller.OnShowStarted(this);
            }

            if (HasEvent(ControllerEvent.ShowPerformed)) {
                _controller.OnShowPerformed(this);
            }

            if (HasEvent(ControllerEvent.ShowCompleted)) {
                _controller.OnShowCompleted(this);
            }

            if (HasEvent(ControllerEvent.HideStarted)) {
                _controller.OnHideStarted(this);
            }

            if (HasEvent(ControllerEvent.HidePerformed)) {
                _controller.OnHidePerformed(this);
            }

            if (HasEvent(ControllerEvent.HideCompleted)) {
                _controller.OnHideCompleted(this);
            }

            // ----- Local Method ----- \\

            [MethodImpl(MethodImplOptions.AggressiveInlining)] 
            bool HasEvent(ControllerEvent _eventType) {
                return _event.HasFlagUnsafe(_eventType);
            }
        }

        protected void OnSetVisibility(bool _visible, bool _instant) {

            // Effects.
            ref List<FadingGroupEffect> _span = ref effects;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                _span[i].OnSetVisibility(_visible, _instant);
            }
        }
        #endregion

        #region Effect
        private List<FadingGroupEffect> effects = new List<FadingGroupEffect>();

        // -----------------------

        /// <summary>
        /// Registers a specific <see cref="FadingGroupEffect"/> for this group.
        /// </summary>
        /// <param name="_effect"><see cref="FadingGroupEffect"/> to register.</param>
        public void RegisterEffect(FadingGroupEffect _effect) {
            effects.Add(_effect);
        }

        /// <summary>
        /// Unregisters a specific <see cref="FadingGroupEffect"/> from this group.
        /// </summary>
        /// <param name="_effect"><see cref="FadingGroupEffect"/> to unregister.</param>
        public void UnregisterEffect(FadingGroupEffect _effect) {
            effects.Remove(_effect);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get if this group has a specific parameter enabled.
        /// </summary>
        /// <param name="_parameter">Parameter to check.</param>
        /// <returns>True if this group has the parameter enabled, false otherwise.</returns>
        public bool HasParameter(Parameters _parameter) {
            return GroupParameters.HasFlagUnsafe(_parameter);
        }
        #endregion
    }

    /// <summary>
    /// Special <see cref="FadingGroup"/>, using an intermediary <see cref="IFadingObject"/> as a transition.
    /// <br/> Makes the transition group perform a fade in, set this group visibility, then fades out the transition group.
    /// </summary>
    [Serializable]
    public abstract class FadingObjectTransitionFadingGroup : FadingGroup {
        #region Global Members
        [Space(10f)]

        [Tooltip("Duration before fading out the transition group when showing this group")]
        [Enhanced, Range(0f, 5f)] public float ShowDelay = .5f;

        [Tooltip("Duration before fading out the transition group when hiding this group")]
        [Enhanced, Range(0f, 5f)] public float HideDelay = .5f;

        [Space(10f)]

        [Tooltip("If true, only the transition group fade in will be affected by the instant fade parameter")]
        public bool InstantOnlyAffectFadeIn = false;

        // -----------------------

        public override float ShowDuration {
            get { return TransitionGroup.ShowDuration + ShowDelay + TransitionGroup.HideDuration; }
        }

        public override float HideDuration {
            get { return TransitionGroup.ShowDuration + HideDelay + TransitionGroup.HideDuration; }
        }

        public override bool IsVisible {
            get { return base.IsVisible && !TransitionGroup.IsVisible; }
        }

        public override bool UseDefaultControllerEvent {
            get { return false; }
        }

        /// <summary>
        /// The <see cref="IFadingObject"/> used for this group transitions.
        /// </summary>
        public abstract IFadingObject TransitionGroup { get; }
        #endregion

        #region Behaviour
        private Action<bool> onFadeCompleteCallback = null;

        private Action<bool> onShowTransitionCompleted = null;
        private Action<bool> onShowTransitionPerformed = null;
        private Action       onShowTransitionFaded     = null;

        private Action<bool> onHideTransitionCompleted = null;
        private Action<bool> onHideTransitionPerformed = null;
        private Action       onHideTransitionFaded     = null;

        private Action<bool> onStartFadeCompleteCallback = null;
        private Action onStartFadedCallback = null;

        private Action onStartFadeOutWaitCompelte = null;
        private Action onStartFadeInWaitCompelte  = null;
        private Action<bool> onStartFadeOutFaded  = null;
        private Action<bool> onStartFadeInFaded   = null;

        private Action<bool> onCompleteFadeCallback = null;
        private Action<bool> onCompleteFade         = null;

        // -------------------------------------------
        // Core
        // -------------------------------------------

        protected override void OnShow(bool _isInstant, Action<bool> _onComplete = null) {

            CancelCurrentFade();
            onFadeCompleteCallback = _onComplete;

            // Already active.
            if (IsVisible) {
                OnComplete(true);
                return;
            }

            // Instant.
            if (_isInstant) {

                if (InstantOnlyAffectFadeIn) {

                    // Only apply on fade in.
                    TransitionGroup.Show(true);

                } else {

                    // Instant.
                    base.OnShow(true, _onComplete);
                    CallControllerEvent(ControllerEvent.FullShow);

                    return;
                }
            }

            CallControllerEvent(ControllerEvent.ShowStarted);

            if (onShowTransitionFaded == null) {
                onShowTransitionCompleted = OnComplete;
                onShowTransitionFaded     = OnTransitionFaded;
            }

            TransitionGroup.FadeInOut(ShowDelay, onShowTransitionFaded, null, onShowTransitionCompleted);

            // ----- Local Methods ----- \\

            void OnTransitionFaded() {
                onShowTransitionPerformed ??= OnShowed;
                base.OnShow(false, onShowTransitionPerformed);
            }

            void OnShowed(bool _completed) {

                // Failed.
                if (!_completed) {
                    OnFailed();
                    return;
                }

                CallControllerEvent(ControllerEvent.ShowPerformed);
            }

            void OnComplete(bool _completed) {

                // Failed.
                if (!_completed) {
                    OnFailed();
                    return;
                }

                CallControllerEvent(ControllerEvent.ShowCompleted);
                onFadeCompleteCallback?.Invoke(_completed);
            }

            void OnFailed() {
                onFadeCompleteCallback?.Invoke(false);
                CallControllerEvent(ControllerEvent.CompleteShow);
            }
        }

        protected override void OnHide(bool _isInstant, Action<bool> _onComplete = null) {

            CancelCurrentFade();
            onFadeCompleteCallback = _onComplete;

            // Already inactive.
            if (!IsVisible) {
                OnComplete(true);
                return;
            }

            // Instant.
            if (_isInstant) {

                if (InstantOnlyAffectFadeIn) {

                    // Only apply on fade in.
                    TransitionGroup.Show(true);

                } else {

                    // Instant.
                    base.OnHide(true, _onComplete);
                    CallControllerEvent(ControllerEvent.FullHide);

                    return;
                }
            }

            CallControllerEvent(ControllerEvent.HideStarted);

            if (onHideTransitionFaded == null) {
                onHideTransitionCompleted = OnComplete;
                onHideTransitionFaded     = OnTransitionFaded;
            }

            TransitionGroup.FadeInOut(HideDelay, onHideTransitionFaded, null, onHideTransitionCompleted);

            // ----- Local Methods ----- \\

            void OnTransitionFaded() {
                onHideTransitionPerformed ??= OnHided;
                base.OnHide(false, onHideTransitionPerformed);
            }

            void OnHided(bool _completed) {

                // Failed.
                if (!_completed) {
                    OnFailed();
                    return;
                }

                CallControllerEvent(ControllerEvent.HidePerformed);
            }

            void OnComplete(bool _completed) {

                // Failed.
                if (!_completed) {
                    OnFailed();
                    return;
                }

                CallControllerEvent(ControllerEvent.HideCompleted);
                onFadeCompleteCallback?.Invoke(_completed);
            }

            void OnFailed() {
                onFadeCompleteCallback?.Invoke(false);
                CallControllerEvent(ControllerEvent.CompleteHide);
            }
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        public override void CancelCurrentFade() {
            base.CancelCurrentFade();
            //TransitionGroup.CancelCurrentFade();
        }

        public override void Evaluate(float _time, bool _show) {
            float _duration = (_show ? ShowDuration : HideDuration) - TransitionGroup.HideDuration;
            float _alpha;

            if (_time < TransitionGroup.ShowDuration) {

                TransitionGroup.Evaluate(_time, true);
                _alpha = _show ? 0f : 1f;

            } else if (_time > _duration) {

                TransitionGroup.Evaluate(_time - _duration, true);
                _alpha = _show ? 1f : 0f;

            } else {

                TransitionGroup.Show(true, null);
                _alpha = 0f;
            }

            // Fade.
            FadeGroup(_alpha, true, ControllerEvent.None, false, null, true);
        }

        public override void SetFadeValue(float _value, bool _show) {
            float _duration = _show ? ShowDuration : HideDuration;
            float _time     = _value * _duration;

            Evaluate(_time, _show);
        }

        // -------------------------------------------
        // Transition
        // -------------------------------------------

        /// <summary>
        /// Fades in the transition group, then show this group, and wait for the show duration.
        /// <br/> Does not fade the transition group out.
        /// </summary>
        /// <param name="_onFaded">Called once the transition group has faded.</param>
        /// <param name="_onComplete">Called after waiting for this group show duration.</param>
        public void StartFadeIn(Action _onFaded = null, Action<bool> _onComplete = null) {
            CancelCurrentFade();
            CallControllerEvent(ControllerEvent.ShowStarted);

            onStartFadeCompleteCallback = _onComplete;
            onStartFadedCallback        = _onFaded;

            onStartFadeInFaded ??= OnFaded;
            TransitionGroup.Show(onStartFadeInFaded);

            // ----- Local Methods ----- \\

            void OnFaded(bool _completed) {
                base.OnShow(false, null);
                CallControllerEvent(ControllerEvent.ShowPerformed);

                onStartFadedCallback?.Invoke();
                hasDelayFailed = false;

                onStartFadeInWaitCompelte ??= OnWaitComplete;
                delay = Delayer.Call(ShowDelay, onStartFadeInWaitCompelte, RealTime);
            }

            void OnWaitComplete() {
                onStartFadeCompleteCallback?.Invoke(true);
            }
        }

        /// <summary>
        /// Fades in the transition group, then hide this group, and wait for the hide duration.
        /// <br/> Does not fade the transition group out.
        /// </summary>
        /// <param name="_onFaded">Called once the transition group has faded.</param>
        /// <param name="_onComplete">Called after waiting for this group hide duration.</param>
        public void StartFadeOut(Action _onFaded = null, Action<bool> _onComplete = null) {
            CancelCurrentFade();
            CallControllerEvent(ControllerEvent.HideStarted);

            onStartFadeCompleteCallback = _onComplete;
            onStartFadedCallback        = _onFaded;

            onStartFadeOutFaded ??= OnFaded;
            TransitionGroup.Show(onStartFadeOutFaded);

            // ----- Local Methods ----- \\

            void OnFaded(bool _completed) {
                base.OnHide(false, null);
                CallControllerEvent(ControllerEvent.HidePerformed);

                onStartFadedCallback?.Invoke();
                hasDelayFailed = false;

                onStartFadeOutWaitCompelte ??= OnWaitComplete;
                delay = Delayer.Call(ShowDelay, onStartFadeOutWaitCompelte, RealTime);
            }

            void OnWaitComplete() {
                onStartFadeCompleteCallback?.Invoke(true);
            }
        }

        /// <summary>
        /// Fades out the transition group.
        /// </summary>
        /// <param name="_onComplete">Called once fading has been completed.</param>
        public void CompleteFade(Action<bool> _onComplete = null) {
            onCompleteFadeCallback = _onComplete;

            onCompleteFade ??= OnComplete;
            TransitionGroup.Hide(onCompleteFade);

            // ----- Local Method ----- \\

            void OnComplete(bool _completed) {
                CallControllerEvent(IsVisible ? ControllerEvent.ShowCompleted : ControllerEvent.HideCompleted);
                onCompleteFadeCallback?.Invoke(_completed);
            }
        }
        #endregion
    }

    // ===== Derived ===== \\

    /// <summary>
    /// <see cref="FadingGroup"/> with no transition, instantly updating its associted group value.
    /// </summary>
    [Serializable]
    public sealed class InstantFadingGroup : FadingGroup { }

    /// <summary>
    /// Class wrapper for a fading <see cref="CanvasGroup"/> instance, using tweening.
    /// </summary>
    [Serializable]
    public sealed class TweeningFadingGroup : FadingGroup {
        #region Global Members
        [Space(10f)]

        [Tooltip("Total duration used to fade in this group (in seconds)")]
        [SerializeField, Enhanced, Range(0f, 10f)] private float fadeInDuration = .5f;
        #if TWEENING
        [Tooltip("Ease preset used to fade in this group")]
        [SerializeField] private Ease fadeInEase = Ease.OutSine;

        [Space(5f)]
        #endif

        [Tooltip("Total duration used to fade out this group (in seconds)")]
        [SerializeField, Enhanced, Range(0f, 10f)] private float fadeOutDuration = .5f;
        #if TWEENING
        [Tooltip("Ease preset used to fade out this group")]
        [SerializeField] private Ease fadeOutEase = Ease.InSine;
        #endif

        // -----------------------

        public override float ShowDuration {
            get { return fadeInDuration; }
        }

        public override float HideDuration {
            get { return fadeOutDuration; }
        }

        public override bool UseDefaultControllerEvent {
            get { return false; }
        }
        #endregion

        #region Behaviour
        private Action<bool> onFadeCompleteCallback = null;
        private Action<bool> onFadeComplete         = null;
        private Action<float> onFadeSetValue        = null;

        private TweenHandler tween = default;
        private float targetAlpha  = 0f;

        // -------------------------------------------
        // Core
        // -------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnShow(bool _isInstant, Action<bool> _onComplete) {
            OnShow(_isInstant, fadeInDuration,
                    #if TWEENING
                    fadeInEase,
                    #endif
                    _onComplete);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnHide(bool _isInstant, Action<bool> _onComplete) {
            OnHide(_isInstant, fadeOutDuration,
                    #if TWEENING
                    fadeOutEase,
                    #endif
                    _onComplete);
        }

        #if TWEENING
        // -------------------------------------------
        // Tween
        // -------------------------------------------

        /// <param name="_duration">Fading duration (in seconds).</param>
        /// <param name="_ease">Fading ease preset curve.</param>
        /// <inheritdoc cref="FadingObject.Show(bool, Action{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Show(float _duration, Ease _ease, Action<bool> _onComplete = null) {
            OnShow(false, _duration,
                    #if TWEENING
                    _ease,
                    #endif
                    _onComplete);
        }

        /// <param name="_duration">Fading duration (in seconds).</param>
        /// <param name="_ease">Fading ease preset curve.</param>
        /// <inheritdoc cref="FadingObject.Hide(bool, Action{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Hide(float _duration, Ease _ease, Action<bool> _onComplete = null) {
            OnHide(false, _duration,
                    #if TWEENING
                    _ease,
                    #endif
                    _onComplete);
        }
        #endif

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        public override void Evaluate(float _time, bool _show) {

            float _duration = _show ? fadeInDuration : fadeOutDuration;
            float _value    = (_duration != 0f) ? Mathf.Clamp01(_time / _duration) : 0f;

            SetFadeValue(_value, _show);
        }

        public override void SetFadeValue(float _value, bool _show) {
            float _alpha;

            #if TWEENING
            if (_show) {
                _alpha = DOVirtual.EasedValue(FadeAlpha.x, FadeAlpha.y, _value, fadeInEase);
            } else {
                _alpha = DOVirtual.EasedValue(FadeAlpha.y, FadeAlpha.x, _value, fadeOutEase);
            }
            #else
            if (_show) {
                _alpha = Mathf.Lerp(FadeAlpha.x, FadeAlpha.y, _value);
            } else {
                _alpha = Mathf.Lerp(FadeAlpha.y, FadeAlpha.x, _value);
            }
            #endif

            ControllerEvent _event = (_alpha == FadeAlpha.y) ? ControllerEvent.FullShow
                                   : (_alpha == FadeAlpha.x) ? ControllerEvent.FullHide
                                   : ControllerEvent.None;

            FadeGroup(_alpha, true, _event, false, null, true);
        }

        public override void CancelCurrentFade() {
            base.CancelCurrentFade();

            // Stop, without complete.
            tween.Stop();
        }

        // -------------------------------------------
        // Fade
        // -------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnShow(bool _isInstant, float _duration,
                            #if TWEENING
                            Ease _ease,
                            #endif
                            Action<bool> _onComplete) {
            OnFade(_isInstant, FadeAlpha.y, _duration,
                    #if TWEENING
                    _ease,
                    #endif
                    ControllerEvent.FullShow, _onComplete);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnHide(bool _isInstant, float _duration,
                            #if TWEENING
                            Ease _ease,
                            #endif
                            Action<bool> _onComplete) {
            OnFade(_isInstant, FadeAlpha.x, _duration,
                    #if TWEENING
                    _ease,
                    #endif
                    ControllerEvent.FullHide, _onComplete);
        }

        private void OnFade(bool _isInstant, float _value, float _duration,
                            #if TWEENING
                            Ease _ease,
                            #endif
                            ControllerEvent _event,
                            Action<bool> _onComplete) {

            // Instant.
            if (_isInstant || (_duration == 0f)) {
                FadeGroup(_value, true, _event, false, _onComplete, true);
                return;
            }

            // Register on callback if targetting the same alpha.
            if ((targetAlpha == _value) && tween.GetHandle(out EnhancedTween _tween)) {
                _tween.AddCallback(_onComplete);
                return;
            }

            CancelCurrentFade();
            onFadeCompleteCallback = _onComplete;

            // Already faded.
            if (Group.alpha == _value) {
                OnStopped(true);
                return;
            }

            bool _isVisible = _value != FadeAlpha.x;
            bool _select    = _isVisible && !IsVisible;

            ToggleCanvas(_value, _select);
            CallControllerEvent((_value == FadeAlpha.y) ? ControllerEvent.ShowStarted : ControllerEvent.HideStarted);

            OnSetVisibility(_isVisible, false);

            // Delegates.
            if (onFadeSetValue == null) {
                onFadeSetValue = OnSet;
                onFadeComplete = OnStopped;
            }

            // Tween.
            targetAlpha = _value;
            tween = Core.Tweener.Tween(Group.alpha, _value, onFadeSetValue, _duration,
                                       #if TWEENING
                                       _ease,
                                       #endif
                                       RealTime, onFadeComplete);

            // ----- Local Methods ----- \\

            void OnSet(float _value) {
                Group.alpha = _value;
            }

            void OnStopped(bool _completed) {

                ToggleCanvas(false);
                CallControllerEvent(IsVisible ? ControllerEvent.CompleteShow : ControllerEvent.CompleteHide);

                onFadeCompleteCallback?.Invoke(_completed);
            }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="FadingObjectTransitionFadingGroup"/> that can used any <see cref="IFadingObject"/> as a transition.
    /// </summary>
    [Serializable]
    public sealed class TransitionFadingGroup : FadingObjectTransitionFadingGroup {
        #region Global Members
        [Tooltip("The FadingGroup to use as a transition: performs a fade in, set this group visibility, then fades out")]
        [SerializeField] private SerializedInterface<IFadingObject> transitionGroup = new SerializedInterface<IFadingObject>();

        // -----------------------

        public override IFadingObject TransitionGroup {
            get { return transitionGroup.Interface;  }
        }
        #endregion
    }
}
