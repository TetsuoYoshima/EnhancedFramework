// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
// 
// Notes:
// 
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="DelayedCall"/>-related wrapper for a single call operation.
    /// </summary>
    public struct DelayHandler : IHandler<DelayedCall> {
        #region Global Members
        private Handler<DelayedCall> handler;

        // -----------------------

        public readonly int ID {
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

        /// <inheritdoc cref="DelayHandler(DelayedCall, int)"/>
        public DelayHandler(DelayedCall _tween) {
            handler = new Handler<DelayedCall>(_tween);
        }

        /// <param name="_call"><see cref="DelayedCall"/> to handle.</param>
        /// <param name="_id">ID of the associated call operation.</param>
        /// <inheritdoc cref="DelayHandler"/>
        public DelayHandler(DelayedCall _call, int _id) {
            handler = new Handler<DelayedCall>(_call, _id);
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="IHandler{T}.GetHandle(out T)"/>
        public bool GetHandle(out DelayedCall _call) {
            return handler.GetHandle(out _call) && (_call.Status != DelayedCall.State.Inactive);
        }

        /// <summary>
        /// Pauses this handler associated call.
        /// </summary>
        public bool Pause() {
            if (GetHandle(out DelayedCall _call)) {
                _call.Pause();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resumes this handler associated call.
        /// </summary>
        public bool Resume() {
            if (GetHandle(out DelayedCall _call)) {
                _call.Resume();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Completes this handler associated call.
        /// </summary>
        public bool Complete() {
            if (GetHandle(out DelayedCall _call)) {
                _call.Complete();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Cancels this handler associated call.
        /// </summary>
        public bool Cancel() {
            if (GetHandle(out DelayedCall _call)) {
                _call.Cancel();
                return true;
            }

            return false;
        }
        #endregion
    }

    /// <summary>
    /// Utility class used to delay method calls both during editor and runtime.
    /// </summary>
    [Serializable]
    public sealed class DelayedCall : IHandle, IPoolableObject {
        #region State
        /// <summary>
        /// References all available states of this object.
        /// </summary>
        public enum State {
            Inactive = 0,

            Active   = 1,
            Paused   = 2,
        }
        #endregion

        #region Global Members
        private State state = State.Inactive;
        private int id      = 0;

        private bool useRealTime = true;
        private float delay      = 0f;

        /// <summary>
        /// Callback delay within this object.
        /// </summary>
        public Action OnComplete = null;

        // -----------------------

        /// <inheritdoc cref="IHandle.ID"/>
        public int ID {
            get { return id; }
        }

        /// <summary>
        /// Current state of this call.
        /// </summary>
        public State Status {
            get { return state; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents from instanciating new instances without using the <see cref="Delayer"/> class.
        /// </summary>
        internal DelayedCall() { }
        #endregion

        #region Delay
        private static int lastID = 0;

        // -----------------------

        /// <summary>
        /// Initializes this object for a new delayed call operation.
        /// </summary>
        /// <param name="_delay">Call delay (in seconds).</param>
        /// <param name="_callback">Callback to call on completion.</param>
        /// <param name="_realTime">If true, time scale will be ignored.</param>
        /// <returns><see cref="DelayHandler"/> of this call operation.</returns>
        internal DelayHandler DelayCall(float _delay, Action _callback, bool _realTime = false) {
            // Cancel current operation.
            Cancel();

            // Prepare.
            SetState(State.Active);

            useRealTime = _realTime;
            OnComplete  = _callback;
            delay       = _delay;

            id = ++lastID;
            return new DelayHandler(this, id);
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        /// <summary>
        /// Pauses this call.
        /// </summary>
        public void Pause() {

            // Ignore if not active.
            if (state != State.Active) {
                return;
            }

            SetState(State.Paused);
        }

        /// <summary>
        /// Resumes this call.
        /// </summary>
        public void Resume() {

            // Ignore if not paused.
            if (state != State.Paused) {
                return;
            }

            SetState(State.Active);
        }

        /// <summary>
        /// Completes this call.
        /// </summary>
        public void Complete() {
            Stop(true);
        }

        /// <summary>
        /// Cancels this call.
        /// </summary>
        public void Cancel() {
            Stop(false);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        internal bool Update(float _deltaTime, float _realDeltaTime) {

            // Ignore if not active.
            if (state != State.Active) {
                return false;
            }

            delay -= useRealTime ? _realDeltaTime : _deltaTime;

            // Complete.
            if (delay <= 0f) {
                Stop(true);
                return true;
            }

            return false;
        }

        private void Stop(bool _isCompleted) {

            // Ignore if already inactive.
            if (state == State.Inactive) {
                return;
            }

            // State.
            SetState(State.Inactive);
            id = 0;

            // Callback.
            if (_isCompleted) {
                OnComplete.Invoke();
            }

            OnComplete = null;
            Delayer.ReleaseCall(this);
        }
        #endregion

        #region Pool
        void IPoolableObject.OnCreated(IObjectPool _pool) { }

        void IPoolableObject.OnRemovedFromPool() { }

        void IPoolableObject.OnSentToPool() {

            // Make sure the call is not active.
            Cancel();
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

    // ===== Delayer ===== \\

    /// <summary>
    /// Utility class used to dynamically delay a specific method call.
    /// </summary>
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    public sealed class Delayer : IObjectPoolManager<DelayedCall>, IStableUpdate {
        #region Global Members
        /// <summary>
        /// Static singleton instance.
        /// </summary>
        private static readonly Delayer instance = new Delayer();
        private static bool isInitialized = false;

        public int InstanceID { get; set; } = EnhancedUtility.GenerateGUID();

        public Object LogObject {
            get { return GameManager.Instance; }
        }

        // -------------------------------------------
        // Initialization
        // -------------------------------------------

        // Called after the first scene Awake.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize() {

            if (isInitialized)
                return;

            calls.Clear();
            pool.Initialize(instance);

            UpdateManager.Instance.Register(instance, UpdateRegistration.Stable);
            isInitialized = true;
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        // Editor constructor.
        static Delayer() {

            calls.Clear();
            pool.Initialize(instance);

            EditorApplication.update += EditorUpdate;
        }
        
        private static void EditorUpdate() {

            if (!Application.isPlaying) {
                instance.Update();
            }
        }
        #endif
        #endregion

        #region Behaviour
        private static readonly EnhancedCollection<DelayedCall> calls = new EnhancedCollection<DelayedCall>();
        private static int lastFrameUpdate = -1;
        private static bool requireRefresh = false;

        // -----------------------

        /// <summary>
        /// Calls a given callback after a specific delay.
        /// </summary>
        /// <inheritdoc cref="DelayedCall.DelayCall(float, Action, bool)"/>
        public static DelayHandler Call(float _delay, Action _callback, bool _realTime = false) {
            DelayedCall _call = GetCall();
            calls.Add(_call);

            if (Time.frameCount > lastFrameUpdate) {
                _delay += _realTime ? ChronosUtility.RealDeltaTime : ChronosUtility.DeltaTime;
            }

            return _call.DelayCall(_delay, _callback, _realTime);
        }

        /// <summary>
        /// Cancels all registered <see cref="DelayedCall"/>.
        /// </summary>
        public static void CancelAllDelayedCalls() {
            ref List<DelayedCall> _callSpan = ref calls.collection;
            for (var i = _callSpan.Count; i-- > 0;) {
                _callSpan[i].Cancel();
            }
        }

        // -------------------------------------------
        // Update
        // -------------------------------------------

        public void Update() {
            ref List<DelayedCall> _span = ref calls.collection;
            int _count = _span.Count;

            if (_count == 0)
                return;

            float _realDeltaTime = ChronosUtility.RealDeltaTime;
            float _deltaTime     = ChronosUtility.DeltaTime;

            requireRefresh = false;

            // Execute calls in registered order.
            for (int i = _count; i-- > 0;) {

                // Index management.
                try {
                    if (_span[i].Update(_deltaTime, _realDeltaTime) || requireRefresh) {

                        i = Mathf.Min(i, _span.Count);
                        requireRefresh = false;
                    }
                } catch (ArgumentOutOfRangeException) {
                    i = Mathf.Min(i, _span.Count);
                }
            }

            lastFrameUpdate = Time.frameCount;
        }
        #endregion

        #region Pool
        private static readonly ObjectPool<DelayedCall> pool = new ObjectPool<DelayedCall>(3);

        // -----------------------

        /// <summary>
        /// Get a <see cref="DelayedCall"/> instance from the pool.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.GetPoolInstance"/>
        private static DelayedCall GetCall() {
            Initialize();
            return pool.GetPoolInstance();
        }

        /// <summary>
        /// Releases a specific <see cref="DelayedCall"/> instance and sent it back to the pool.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.ReleasePoolInstance(T)"/>
        internal static bool ReleaseCall(DelayedCall _call) {

            if (calls.Remove(_call)) {
                requireRefresh = true;
            }

            return pool.ReleasePoolInstance(_call);
        }

        /// <summary>
        /// Clears the <see cref="DelayedCall"/> pool content.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.ClearPool(int)"/>
        public static void ClearPool(int _capacity = 1) {
            pool.ClearPool(_capacity);
        }

        // -------------------------------------------
        // Manager
        // -------------------------------------------

        DelayedCall IObjectPool<DelayedCall>.GetPoolInstance() {
            return GetCall();
        }

        bool IObjectPool<DelayedCall>.ReleasePoolInstance(DelayedCall _instance) {
            return ReleaseCall(_instance);
        }

        void IObjectPool.ClearPool(int _capacity) {
            ClearPool(_capacity);
        }

        DelayedCall IObjectPoolManager<DelayedCall>.CreateInstance() {
            return new DelayedCall();
        }

        void IObjectPoolManager<DelayedCall>.DestroyInstance(DelayedCall _call) {
            // Cannot destroy the instance, so simply ignore the object and wait for the garbage collector to pick it up.
        }
        #endregion
    }
}
