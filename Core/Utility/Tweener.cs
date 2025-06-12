// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
// 
// Notes:
// 
// ================================================================================== //

#if DOTWEEN_ENABLED
#define DOTWEEN
#endif

#if UNITY_EDITOR && EDITOR_COROUTINE_ENABLED
#define EDITOR_COROUTINE
#endif

using EnhancedEditor;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

#if DOTWEEN
using DG.Tweening;
using DG.Tweening.Core;
#endif

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

#if EDITOR_COROUTINE
using Unity.EditorCoroutines.Editor;
#endif

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="EnhancedTween"/>-related wrapper for a single tween operation.
    /// </summary>
    public struct TweenHandler : IHandler<EnhancedTween> {
        #region Global Members
        private Handler<EnhancedTween> handler;

        // -----------------------

        public int ID {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return handler.ID; }
        }

        public bool IsValid {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return GetHandle(out _); }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="TweenHandler(EnhancedTween, int)"/>
        public TweenHandler(EnhancedTween _tween) {
            handler = new Handler<EnhancedTween>(_tween);
        }

        /// <param name="_tween"><see cref="EnhancedTween"/> to handle.</param>
        /// <param name="_id">ID of the associated tween operation.</param>
        /// <inheritdoc cref="TweenHandler"/>
        public TweenHandler(EnhancedTween _tween, int _id) {
            handler = new Handler<EnhancedTween>(_tween, _id);
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="IHandler{T}.GetHandle(out T)"/>
        public bool GetHandle(out EnhancedTween _tween) {
            return handler.GetHandle(out _tween) && (_tween.Status != EnhancedTween.State.Inactive);
        }

        /// <summary>
        /// Pauses this handler associated tween.
        /// </summary>
        public bool Pause() {
            if (GetHandle(out EnhancedTween _tween)) {
                _tween.Pause();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resumes this handler associated tween.
        /// </summary>
        public bool Resume() {
            if (GetHandle(out EnhancedTween _tween)) {
                _tween.Resume();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Completes this handler associated tween.
        /// </summary>
        public bool Complete() {
            if (GetHandle(out EnhancedTween _tween)) {
                _tween.Complete();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops this handler associated tween.
        /// </summary>
        public bool Stop() {
            if (GetHandle(out EnhancedTween _tween)) {
                _tween.Stop();
                return true;
            }

            return false;
        }
        #endregion
    }

    /// <summary>
    /// Enhanced class used to perform simple tweens both during editor and runtime.
    /// </summary>
    [Serializable]
    public sealed class EnhancedTween : IHandle, IPoolableObject {
        #region State
        /// <summary>
        /// References all available states of this object.
        /// </summary>
        public enum State {
            Inactive = 0,

            Playing  = 1,
            Paused   = 2,
        }
        #endregion

        #region Global Members
        private State state = State.Inactive;
        private int id = 0;

        /// <summary>
        /// Called when this tween is stopped, with a <see cref="bool"/> parameter indicating if it was successfully completed or not.
        /// </summary>
        public List<Action<bool>> OnStopped = new List<Action<bool>>();

        // -----------------------

        /// <inheritdoc cref="IHandle.ID"/>
        public int ID {
            get { return id; }
        }

        /// <summary>
        /// Current state of this tweener.
        /// </summary>
        public State Status {
            get { return state; }
        }
        #endregion

        #region Tween
        private static int lastID = 0;

        private bool doComplete = false; // Used to complete coroutines.

        private Action<float> tweenSetter1 = null;
        private Action<float> tweenSetter2 = null;
        private Action<float> tweenSetter3 = null;

        private Action<float> setter = null;
        private AnimationCurve curve = null;
        private float fromValue = 0f;
        private float toValue   = 0f;

        #if DOTWEEN
        private DOGetter<float> tweenGetter = null;
        private DOSetter<float> tweenSetter = null;
        private TweenCallback tweenCompleteCallback = null;
        private TweenCallback tweenKillCallback     = null;

        private Action<float> tweenCurrentSetter = null;

        private Tween tween = null;
        private float time  = 0f;

        private Action<float> tweenDoSetter1 = null;
        private Action<float> tweenDoSetter2 = null;

        private Ease ease   = Ease.Unset;
        #else
        private Coroutine coroutine = null;
        #endif

        #if EDITOR_COROUTINE
        private EditorCoroutine editorCoroutine = null;
        #endif

        // -----------------------

        #if DOTWEEN
        /// <param name="_ease">Tween evaluation <see cref="Ease"/>.</param>
        /// <inheritdoc cref="Tween(float, float, Action{float}, float, bool, Action{bool})"/>
        public TweenHandler Tween(float _from, float _to, Action<float> _setter, float _duration, Ease _ease, bool _realTime = false, Action<bool> _onStopped = null) {

            fromValue = _from;
            toValue   = _to;
            setter    = _setter;
            ease      = _ease;

            tweenDoSetter1 ??= Set;
            return Tween(tweenDoSetter1, _duration, _realTime, _onStopped);

            // ----- Local Method ----- \\

            void Set(float _percent) {

                _percent = DOVirtual.EasedValue(0f, 1f, _percent, ease);
                float _value = fromValue + ((toValue - fromValue) * _percent);

                setter(_value);
            }
        }

        /// <param name="_ease">Tween evaluation <see cref="Ease"/>.</param>
        /// <inheritdoc cref="Tween(Action{float}, float, bool, Action{bool})"/>
        public TweenHandler Tween(Action<float> _setter, float _duration, Ease _ease, bool _realTime = false, Action<bool> _onStopped = null) {

            setter = _setter;
            ease   = _ease;

            tweenDoSetter2 ??= Set;
            return Tween(tweenDoSetter2, _duration, _realTime, _onStopped);

            // ----- Local Method ----- \\

            void Set(float _percent) {

                _percent = DOVirtual.EasedValue(0f, 1f, _percent, ease);
                setter(_percent);
            }
        }
        #endif

        /// <param name="_curve">Tween evaluation curve.</param>
        /// <inheritdoc cref="Tween(float, float, Action{float}, float, bool, Action{bool})"/>
        public TweenHandler Tween(float _from, float _to, Action<float> _setter, float _duration, AnimationCurve _curve, bool _realTime = false, Action<bool> _onStopped = null) {

            fromValue = _from;
            toValue   = _to;
            setter    = _setter;
            curve     = _curve;

            tweenSetter1 ??= Set;
            return Tween(tweenSetter1, _duration, _realTime, _onStopped);

            // ----- Local Method ----- \\

            void Set(float _percent) {

                _percent = (_percent == 0f) ? _percent : curve.Evaluate(_percent * curve.Duration());
                float _value = fromValue + ((toValue - fromValue) * _percent);

                setter(_value);
            }
        }

        /// <param name="_curve">Tween evaluation curve.</param>
        /// <inheritdoc cref="Tween(Action{float}, float, bool, Action{bool})"/>
        public TweenHandler Tween(Action<float> _setter, float _duration, AnimationCurve _curve, bool _realTime = false, Action<bool> _onStopped = null) {

            setter = _setter;
            curve  = _curve;

            tweenSetter2 ??= Set;

            return Tween(tweenSetter2, _duration, _realTime, _onStopped);

            // ----- Local Method ----- \\

            void Set(float _percent) {

                _percent = (_percent == 0f) ? _percent : curve.Evaluate(_percent * curve.Duration());
                setter(_percent);
            }
        }

        /// <summary>
        /// Tweens a float value from a start to an end value, using a given duration.
        /// <br/> Calls a specific setter with the new float value.
        /// </summary>
        /// <param name="_from">Initial tween value.</param>
        /// <param name="_to">Target tween value.</param>
        /// <param name="_setter">Called to set the new float value.</param>
        /// <inheritdoc cref="Tween(Action{float}, float, bool, Action{bool})"/>
        public TweenHandler Tween(float _from, float _to, Action<float> _setter, float _duration, bool _realTime = false, Action<bool> _onStopped = null) {

            fromValue = _from;
            toValue   = _to;
            setter    = _setter;

            tweenSetter3 ??= Set;
            return Tween(tweenSetter3, _duration, _realTime, _onStopped);

            // ----- Local Method ----- \\

            void Set(float _percent) {

                float _value = fromValue + ((toValue - fromValue) * _percent);
                setter(_value);
            }
        }

        /// <summary>
        /// Performs a tween during a given duration.
        /// <br/> Calls a specific setter with the current tween time percent (between 0 and 1).
        /// </summary>
        /// <param name="_setter">Setter to call, using the current tween time percent as a parameter to update the desired value.</param>
        /// <param name="_duration">Total duration of the tween (in seconds).</param>
        /// <param name="_realTime">If true, time scale will be ignored.</param>
        /// <param name="_onStopped">Called when the tween is stopped, with a <see cref="bool"/> parameter indicating if it was completed or not.</param>
        /// <returns><see cref="TweenHandler"/> of this tween operation.</returns>
        public TweenHandler Tween(Action<float> _setter, float _duration, bool _realTime = false, Action<bool> _onStopped = null) {

            // Stop current operation.
            Stop();

            // Prepare.
            SetState(State.Playing);

            OnStopped.ReplaceBy(_onStopped);
            id = ++lastID;

            TweenHandler _handler = new TweenHandler(this, id);

            if (_duration == 0f) {

                _setter(1f);
                OnStop(true);

                return _handler;
            }

            // Editor.
            #if UNITY_EDITOR
            if (!Application.isPlaying) {

                #if EDITOR_COROUTINE
                editorCoroutine = EditorCoroutineUtility.StartCoroutine(DoTween(_setter, _duration, _realTime), this);
                #else
                _setter(_duration);
                #endif

                return _handler;
            }
            #endif

            #if DOTWEEN
            // Tween.
            time = 0f;
            tweenCurrentSetter = _setter;

            if (tweenGetter == null) {

                tweenGetter = Get;
                tweenSetter = Set;
                tweenCompleteCallback = OnComplete;
                tweenKillCallback     = OnKill;
            }

            tween = DOTween.To(tweenGetter, tweenSetter, 1f, _duration).SetEase(Ease.Linear).SetUpdate(_realTime)
                           .SetRecyclable(true).SetAutoKill(true).OnComplete(tweenCompleteCallback).OnKill(tweenKillCallback);

            // ----- Local Methods ----- \\

            float Get() {
                return time;
            }

            void Set(float _value) {
                time = _value;
                tweenCurrentSetter(_value);
            }

            void OnComplete() {
                OnStop(true);
            }

            void OnKill() {
                tween = null;
                OnStop(false);
            }
            #else
            coroutine = UpdateManager.Instance.StartCoroutine(DoTween(_setter, _duration, _realTime));
            #endif

            return _handler;
        }

        // -----------------------

        private IEnumerator DoTween(Action<float> _setter, float _duration, bool _realTime) {
            float _timer = 0f;

            while (_timer < _duration) {
                // Pause.
                while (state == State.Paused) {
                    yield return null;
                }

                _timer += ChronosUtility.GetDeltaTime(_realTime);

                // End loop.
                if (doComplete || (_timer > _duration)) {
                    _setter(1f);
                    break;
                }

                float _percent = (_timer == 0f) ? 0f : (float)(_timer / _duration);
                _setter(_percent);

                yield return null;
            }

            #if !DOTWEEN
            coroutine = null;
            #endif

            #if EDITOR_COROUTINE
            editorCoroutine = null;
            #endif

            OnStop(true);
        }

        private void OnStop(bool _isCompleted) {

            // Ignore if already inactive.
            if (state == State.Inactive) {
                return;
            }

            // State.
            doComplete = false;
            SetState(State.Inactive);

            // Callback(s).
            ref List<Action<bool>> _span = ref OnStopped;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                _span[i]?.Invoke(_isCompleted);
            }

            _span.Clear();

            Tweener.ReleaseTween(this);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Pauses this tween.
        /// </summary>
        public void Pause() {

            // Ignore if not playing.
            if (state != State.Playing) {
                return;
            }

            #if DOTWEEN
            tween.Pause();
            #endif

            SetState(State.Paused);
        }

        /// <summary>
        /// Resumes this tween.
        /// </summary>
        public void Resume() {

            // Ignore if not paused.
            if (state != State.Paused) {
                return;
            }

            #if DOTWEEN
            tween.Play();
            #endif

            SetState(State.Playing);
        }

        /// <summary>
        /// Completes this tween.
        /// </summary>
        public void Complete() {

            // Ignore if inactive.
            if (state == State.Inactive) {
                return;
            }

            // Resume.
            Resume();

            doComplete = true;

            #if DOTWEEN
            tween.Complete(true);
            #endif
        }

        /// <summary>
        /// Stops this tweener.
        /// </summary>
        public void Stop() {

            // Ignore if inactive.
            if (state == State.Inactive) {
                return;
            }

            #if EDITOR_COROUTINE
            //Editor.
            if (!Application.isPlaying && (editorCoroutine != null)) {

                EditorCoroutineUtility.StopCoroutine(editorCoroutine);
                editorCoroutine = null;
            }
            #endif

            #if DOTWEEN
            tween.DoKill();
            #else
            if (coroutine != null) {
                UpdateManager.Instance.StopCoroutine(coroutine);
                coroutine = null;
            }
            #endif

            // Callback.
            OnStop(false);
        }
        #endregion

        #region Pool
        void IPoolableObject.OnCreated(IObjectPool _pool) { }

        void IPoolableObject.OnRemovedFromPool() { }

        void IPoolableObject.OnSentToPool() {

            // Make sure the tween is not running.
            Stop();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Registers a new callback when this tween ends.
        /// </summary>
        public void AddCallback(Action<bool> _onStopped) {
            if (_onStopped != null) {
                OnStopped.Add(_onStopped);
            }
        }

        /// <summary>
        /// Sets the state of this object.
        /// </summary>
        /// <param name="_state">New state of this object.</param>
        private void SetState(State _state) {
            state = _state;
        }
        #endregion
    }

    // ===== Tweener ===== \\

    /// <summary>
    /// Utility class used to dynamically tween values.
    /// </summary>
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    public sealed class Tweener : IObjectPoolManager<EnhancedTween> {
        #region Global Members
        /// <summary>
        /// Static singleton instance.
        /// </summary>
        private static readonly Tweener instance = new Tweener();

        // -------------------------------------------
        // Initialization
        // -------------------------------------------

        // Called after the first scene Awake.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize() {
            pool.Initialize(instance);
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        // Editor constructor.
        static Tweener() {
            Initialize();
        }
        #endif
        #endregion

        #region Behaviour
        #if DOTWEEN
        /// <inheritdoc cref="EnhancedTween.Tween(float, float, Action{float}, float, Ease, bool, Action{bool})"/>
        public static TweenHandler Tween(float _from, float _to, Action<float> _setter, float _duration, Ease _ease, bool _realTime = false, Action<bool> _onStopped = null) {
            return GetTween().Tween(_from, _to, _setter, _duration, _ease, _realTime, _onStopped);
        }

        /// <inheritdoc cref="EnhancedTween.Tween(Action{float}, float, Ease, bool, Action{bool})"/>
        public static TweenHandler Tween(Action<float> _setter, float _duration, Ease _ease, bool _realTime = false, Action<bool> _onStopped = null) {
            return GetTween().Tween(_setter, _duration, _ease, _realTime, _onStopped);
        }
        #endif

        /// <inheritdoc cref="EnhancedTween.Tween(float, float, Action{float}, float, AnimationCurve, bool, Action{bool})"/>
        public static TweenHandler Tween(float _from, float _to, Action<float> _setter, float _duration, AnimationCurve _curve, bool _realTime = false, Action<bool> _onStopped = null) {
            return GetTween().Tween(_from, _to, _setter, _duration, _curve, _realTime, _onStopped);
        }

        /// <inheritdoc cref="EnhancedTween.Tween(Action{float}, float, AnimationCurve, bool, Action{bool})"/>
        public static TweenHandler Tween(Action<float> _setter, float _duration, AnimationCurve _curve, bool _realTime = false, Action<bool> _onStopped = null) {
            return GetTween().Tween(_setter, _duration, _curve, _realTime, _onStopped);
        }

        /// <inheritdoc cref="EnhancedTween.Tween(float, float, Action{float}, float, bool, Action{bool})"/>
        public static TweenHandler Tween(float _from, float _to, Action<float> _setter, float _duration, bool _realTime = false, Action<bool> _onStopped = null) {
            return GetTween().Tween(_from, _to, _setter, _duration, _realTime, _onStopped);
        }

        /// <inheritdoc cref="EnhancedTween.Tween(Action{float}, float, bool, Action{bool})"/>
        public static TweenHandler Tween(Action<float> _setter, float _duration, bool _realTime = false, Action<bool> _onStopped = null) {
            return GetTween().Tween(_setter, _duration, _realTime, _onStopped);
        }
        #endregion

        #region Pool
        private static readonly ObjectPool<EnhancedTween> pool = new ObjectPool<EnhancedTween>(3);

        // -----------------------

        /// <summary>
        /// Get a <see cref="EnhancedTween"/> instance from the pool.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.GetPoolInstance"/>
        public static EnhancedTween GetTween() {
            return pool.GetPoolInstance();
        }

        /// <summary>
        /// Releases a specific <see cref="EnhancedTween"/> instance and sent it back to the pool.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.ReleasePoolInstance(T)"/>
        internal static bool ReleaseTween(EnhancedTween _tween) {
            return pool.ReleasePoolInstance(_tween);
        }

        /// <summary>
        /// Clears the <see cref="EnhancedTween"/> pool content.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.ClearPool(int)"/>
        public static void ClearPool(int _capacity = 1) {
            pool.ClearPool(_capacity);
        }

        // -------------------------------------------
        // Manager
        // -------------------------------------------

        EnhancedTween IObjectPool<EnhancedTween>.GetPoolInstance() {
            return GetTween();
        }

        bool IObjectPool<EnhancedTween>.ReleasePoolInstance(EnhancedTween _instance) {
            return ReleaseTween(_instance);
        }

        void IObjectPool.ClearPool(int _capacity) {
            ClearPool(_capacity);
        }

        EnhancedTween IObjectPoolManager<EnhancedTween>.CreateInstance() {
            return new EnhancedTween();
        }

        void IObjectPoolManager<EnhancedTween>.DestroyInstance(EnhancedTween _tween) {
            // Cannot destroy the instance, so simply ignore the object and wait for the garbage collector to pick it up.
        }
        #endregion
    }
}
