// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.GameStates;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedFramework.Core {
    // ===== Modifier ===== \\

    /// <summary>
    /// <see cref="ChronosModifier"/>-related callback interface.
    /// </summary>
    public interface IChronosModifierController {
        #region Content
        /// <summary>
        /// Called whenever an associated <see cref="ChronosModifier"/> is stopped and removed.
        /// </summary>
        void OnStoppedChronos(ChronosModifier _chronos, ChronosModifier.Type _type);
        #endregion
    }

    /// <summary>
    /// <see cref="ChronosModifier"/>-related wrapper for a single chronos operation.
    /// </summary>
    public struct ChronosHandler : IHandler<ChronosModifier> {
        #region Global Members
        private Handler<ChronosModifier> handler;

        // -----------------------

        public int ID {
            get { return handler.ID; }
        }

        public bool IsValid {
            get { return GetHandle(out _); }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="ChronosHandler(ChronosModifier, int)"/>
        public ChronosHandler(ChronosModifier _chronos) {
            handler = new Handler<ChronosModifier>(_chronos);
        }

        /// <param name="_chronos"><see cref="ChronosModifier"/> to handle.</param>
        /// <param name="_id">ID of the associated call operation.</param>
        /// <inheritdoc cref="ChronosHandler"/>
        public ChronosHandler(ChronosModifier _chronos, int _id) {
            handler = new Handler<ChronosModifier>(_chronos, _id);
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="IHandler{T}.GetHandle(out T)"/>
        public bool GetHandle(out ChronosModifier _chronos) {
            return handler.GetHandle(out _chronos) && (_chronos.ModifierType != ChronosModifier.Type.None);
        }

        /// <summary>
        /// Removes this handler associated chronos modifier.
        /// </summary>
        public bool Remove(bool _withCallback = true) {
            if (GetHandle(out ChronosModifier _chronos)) {
                _chronos.Remove(_withCallback);
                return true;
            }

            return false;
        }
        #endregion
    }

    /// <summary>
    /// Utility class used to modify the game chronos both during editor and runtime.
    /// </summary>
    [Serializable]
    public sealed class ChronosModifier : IHandle, IPoolableObject, IComparable<ChronosModifier> {
        #region Type
        /// <summary>
        /// References all different types of modifiers.
        /// </summary>
        public enum Type {
            None        = 0,

            Override    = 1,
            Coefficient = 2,
        }
        #endregion

        #region Global Members
        private IChronosModifierController controller = null;
        private Type type = Type.None;

        private float chronos = 0f;
        private int id = 0;

        // -----------------------

        /// <summary>
        /// The type of this chronos modifier.
        /// </summary>
        public Type ModifierType {
            get { return type; }
        }

        /// <summary>
        /// Chronos modifier value.
        /// </summary>
        public float Chronos {
            get { return chronos; }
        }

        /// <inheritdoc cref="IHandle.ID"/>
        public int ID {
            get { return id; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="ChronosModifier(float)"/>
        internal ChronosModifier() : this(1f) { }

        /// <summary>
        /// Prevents from instanciating new instances without using the <see cref="ChronosManager"/> class.
        /// </summary>
        internal ChronosModifier(float _chronos) {
            chronos = _chronos;
        }
        #endregion

        #region Comparer
        int IComparable<ChronosModifier>.CompareTo(ChronosModifier _other) {
            return id.CompareTo(_other.id);
        }
        #endregion

        #region Chronos
        private Action onCompleteCallback = null;
        private Action onChronosComplete  = null;

        private DelayHandler delay = default;

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        /// <inheritdoc cref="ApplyChronos(int, float, Type, float, bool, Action)"/>
        internal ChronosHandler ApplyChronos(int _id, float _chronos, Type _type) {
            // Setup.
            chronos = _chronos;

            SetType(_type);

            delay.Cancel();
            id = _id;

            return new ChronosHandler(this, id);
        }

        /// <summary>
        /// Initializes this object for a new chronos modifier.
        /// </summary>
        /// <param name="_id">Id of this modifier.</param>
        /// <param name="_chronos">Chronos override value.</param>
        /// <param name="_type">Chronos modifier type..</param>
        /// <param name="_duration">Duration of this override (in seconds).</param>
        /// <param name="_realTime">If true, the given duration will not be affected by the game time scale.</param>
        /// <param name="_onComplete">Delegate to be called once the override is removed.</param>
        /// <returns><see cref="ChronosHandler"/> of this chronos modifier.</returns>
        internal ChronosHandler ApplyChronos(int _id, float _chronos, Type _type, float _duration, bool _realTime, Action _onComplete) {
            ChronosHandler _handler = ApplyChronos(_id, _chronos, _type);

            onChronosComplete ??= () => Remove(true);
            onCompleteCallback  = _onComplete;

            if (_duration >= 0f) {
                delay = Delayer.Call(_duration, onChronosComplete, _realTime);
            }

            return _handler;
        }

        /// <summary>
        /// Removes this chronos modifer from the game.
        /// </summary>
        public void Remove(bool _withCallback = true) {

            // Ignore if inactive.
            if (type == Type.None) {
                return;
            }

            Type _type = type;
            SetType(Type.None);

            delay.Cancel();
            id = 0;

            if (_withCallback) {
                onCompleteCallback?.Invoke();
            }

            onCompleteCallback = null;

            controller.OnStoppedChronos(this, _type);
            ChronosManager.Instance.ReleaseChronos(this);
        }
        #endregion

        #region Pool
        void IPoolableObject.OnCreated(IObjectPool _pool) { }

        void IPoolableObject.OnRemovedFromPool() { }

        void IPoolableObject.OnSentToPool() {

            // Make sure that the chronos is no longer active.
            Remove(false);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Sets this chronos controller callback receiver.
        /// </summary>
        internal void SetController(IChronosModifierController _controller) {
            controller = _controller;
        }

        /// <summary>
        /// Sets the type of this object.
        /// </summary>
        /// <param name="_type">New type of this object.</param>
        private void SetType(Type _type) {
            type = _type;
        }
        #endregion
    }

    /// <summary>
    /// Wrapper class used to push and pop chronos overrides and coefficients, around the utilisation of <see cref="ChronosModifier"/>.
    /// </summary>
    internal sealed class ChronosWrapper {
        #region Coefficient
        private static readonly EnhancedCollection<ChronosModifier> coefficientBuffer = new EnhancedCollection<ChronosModifier>();

        // -----------------------

        /// <inheritdoc cref="PushCoefficient(int, float, IChronosModifierController, out ChronosHandler, float, bool, Action)"/>
        public float PushCoefficient(int _id, float _chronosOverride, IChronosModifierController _controller, out ChronosHandler _handler) {
            return PushCoefficient(_id, _chronosOverride, _controller, out _handler, -1f, false, null);
        }

        /// <summary>
        /// Pushes a new chronos coefficient in buffer.
        /// </summary>
        /// <param name="_id">Id of this coefficient - use the same id to modify or "pop" its value.</param>
        /// <param name="_chronosOverride">Chronos coefficient value.</param>
        /// <param name="_duration">Duration of this coefficient (in seconds) - use a negative value for infinite duration.</param>
        /// <param name="_realTime">If true, the given duration will not be affected by the game time scale.</param>
        /// <param name="_onComplete">Delegate to be called once the coefficient is removed.</param>
        /// <inheritdoc cref="ChronosModifier.ApplyChronos(int, float, ChronosModifier.Type, float, bool, Action)"/>
        public float PushCoefficient(int _id, float _chronosOverride, IChronosModifierController _controller, out ChronosHandler _handler, float _duration, bool _realTime, Action _onComplete) {

            ChronosModifier _modifier = GetCoefficient(_id, _controller);

            if (_duration < 0f) {
                _handler = _modifier.ApplyChronos(_id, _chronosOverride, ChronosModifier.Type.Coefficient);
            } else {
                _handler = _modifier.ApplyChronos(_id, _chronosOverride, ChronosModifier.Type.Coefficient, _duration, _realTime, _onComplete);
            }

            return RefreshCoefficient();
        }

        /// <summary>
        /// Pops a previously pushed in buffer chronos coefficient.
        /// </summary>
        /// <param name="_id">Id of the coefficient to pop - same as used to "push" it.</param>
        public void PopCoefficient(int _id, bool _withCallback = true) {
            if (GetCoefficientModifier(_id, out ChronosModifier _chronos)) {
                _chronos.Remove(_withCallback);
            }
        }

        // -------------------------------------------
        // Refresh
        // -------------------------------------------

        /// <summary>
        /// Refreshes and get this object coefficient value.
        /// </summary>
        public float RefreshCoefficient() {

            float _coef = ChronosManager.ChronosDefaultValue;
            ref List<ChronosModifier> _span = ref coefficientBuffer.collection;

            for (int i = _span.Count; i-- > 0;) {
                _coef *= _span[i].Chronos;
            }

            return _coef;
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private ChronosModifier GetCoefficient(int _id, IChronosModifierController _controller) {

            if (!GetCoefficientModifier(_id, out ChronosModifier _modifier)) {

                _modifier = ChronosManager.Instance.GetChronos(_controller);
                coefficientBuffer.Add(_modifier);
            }

            return _modifier;
        }

        private bool GetCoefficientModifier(int _id, out ChronosModifier _modifier) {

            ref List<ChronosModifier> _span = ref coefficientBuffer.collection;

            for (int i = _span.Count; i-- > 0;) {
                _modifier = _span[i];

                if (_modifier.ID == _id) {
                    return true;
                }
            }

            _modifier = null;
            return false;
        }
        #endregion

        #region Override
        private static readonly BufferR<ChronosModifier> overrideBuffer = new BufferR<ChronosModifier>(new ChronosModifier(1f));

        // -----------------------

        /// <inheritdoc cref="PushOverride(int, float, int, IChronosModifierController, out ChronosHandler, float, bool, Action)"/>
        public float PushOverride(int _id, float _chronosOverride, int _priority, IChronosModifierController _controller, out ChronosHandler _handler) {
            return PushOverride(_id, _chronosOverride, _priority, _controller, out _handler, -1f, false, null);
        }

        /// <summary>
        /// Pushes a new chronos override in buffer.
        /// <br/> The active override is always the last pushed in buffer with the highest priority.
        /// </summary>
        /// <param name="_id">Id of this override. Use the same id to modify or pop its value.</param>
        /// <param name="_chronosOverride">Chronos override value.</param>
        /// <param name="_priority">Priority of this override. Only the one with the highest value will be active.</param>
        /// <param name="_duration">Duration of this override (in seconds).</param>
        /// <param name="_realTime">If true, the given duration will not be affected by the game time scale.</param>
        /// <param name="_onComplete">Delegate to be called once the override is removed.</param>
        /// <inheritdoc cref="ChronosModifier.ApplyChronos(int, float, ChronosModifier.Type, float, bool, Action)"/>
        public float PushOverride(int _id, float _chronosOverride, int _priority, IChronosModifierController _controller, out ChronosHandler _handler, float _duration, bool _realTime, Action _onComplete) {

            ChronosModifier _modifier = GetOverride(_id, _controller);

            if (_duration < 0f) {
                _handler = _modifier.ApplyChronos(_id, _chronosOverride, ChronosModifier.Type.Override);
            } else {
                _handler = _modifier.ApplyChronos(_id, _chronosOverride, ChronosModifier.Type.Override, _duration, _realTime, _onComplete);
            }

            return overrideBuffer.Push(_modifier, _priority).Chronos;
        }

        /// <summary>
        /// Pops a previously pushed in buffer chronos override.
        /// </summary>
        /// <param name="_id">Id of the override to pop.</param>
        public void PopOverride(int _id, bool _withCallback = true) {
            if (GetOverrideModifier(_id, out ChronosModifier _modifier)) {
                _modifier.Remove(_withCallback);
            }
        }

        // -------------------------------------------
        // Refresh
        // -------------------------------------------

        /// <summary>
        /// Refreshes and get this object override value.
        /// </summary>
        private float RefreshOverride() {
            return overrideBuffer.Value.Chronos;
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private ChronosModifier GetOverride(int _id, IChronosModifierController _controller) {

            if (!GetOverrideModifier(_id, out ChronosModifier _modifier)) {
                _modifier = ChronosManager.Instance.GetChronos(_controller);
            }

            return _modifier;
        }

        private bool GetOverrideModifier(int _id, out ChronosModifier _modifier) {

            ref List<Pair<ChronosModifier, int>> _span = ref overrideBuffer.collection;

            for (int i = _span.Count; i-- > 0;) {
                _modifier = _span[i].First;

                if (_modifier.ID == _id) {
                    return true;
                }
            }

            _modifier = null;
            return false;
        }
        #endregion

        #region General
        public float OnStoppedChronos(ChronosModifier _chronos, ChronosModifier.Type _type) {

            switch (_type) {

                // Override.
                case ChronosModifier.Type.Override:
                    return overrideBuffer.Pop(_chronos).Chronos;

                // Coefficient.
                case ChronosModifier.Type.Coefficient:
                    coefficientBuffer.Remove(_chronos);
                    return RefreshCoefficient();

                // Ignore.
                case ChronosModifier.Type.None:
                default:
                    break;
            }

            return 1f;
        }
        #endregion
    }

    // ===== Manager ===== \\

    /// <summary>
    /// Game global chronos manager singleton instance.
    /// <br/> Manages the whole time scale of the game, with numerous multiplicators and overrides.
    /// </summary>
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(-990)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Chronos/Chronos Manager"), DisallowMultipleComponent]
    public sealed class ChronosManager : EnhancedSingleton<ChronosManager>, IObjectPoolManager<ChronosModifier>, IChronosModifierController, IGameStateLifetimeCallback {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Global Members
        public const float ChronosDefaultValue = 1f;

        [Section("Chronos Manager")]

        [SerializeField] private SerializedType<IPauseChronosState> pauseStateType = new SerializedType<IPauseChronosState>(SerializedTypeConstraint.None,
                                                                                                                            typeof(DefaultPauseChronosGameState));

        [Space(5f)]

        [SerializeField] private bool usePauseInterface = false;
        [SerializeField, Enhanced, ShowIf(nameof(usePauseInterface)), Required] private FadingObjectBehaviour pauseInterface = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, ReadOnly, DisplayName("Chronos Overr.")] private float chronosOverride    = ChronosDefaultValue;
        [SerializeField, Enhanced, ReadOnly, DisplayName("Chronos Coef. ")] private float chronosCoefficient = ChronosDefaultValue;

        // -----------------------

        private readonly ChronosWrapper chronosWrapper = new ChronosWrapper();

        // -----------------------

        /// <summary>
        /// <see cref="GameState"/> type used when pausing the game chronos.
        /// </summary>
        public Type PauseStateType {
            get { return pauseStateType.Type; }
        }
        #endregion

        #region Enhanced Behaviour
        private readonly int editorChronosID = EnhancedUtility.GenerateGUID();

        // -----------------------

        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Override the default chronos behaviour to implement it as a coefficient.
            ChronosStepper.OnSetChronos = ApplyChronosStepper;
        }

        protected override void OnInit() {
            base.OnInit();

            // Initialization.
            pool.Initialize(this);
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Back to default behaviour.
            ChronosStepper.OnSetChronos = (c) => Time.timeScale = c;
        }

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        private void ApplyChronosStepper(float _chronos) {
            PushCoefficient(editorChronosID, _chronos);
        }
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
        /// <summary>
        /// Called whenever the game time scale is changed.
        /// </summary>
        public static Action<float> OnTimeScaleUpdate = null;

        // -----------------------

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
            float _chronos = Mathf.Min(99f, chronosOverride * chronosCoefficient);

            Chronos        = _chronos;
            Time.timeScale = _chronos;

            OnTimeScaleUpdate?.Invoke(_chronos);
        }
        #endregion

        // ===== Miscs ===== \\

        #region Pause
        private GameState pauseState = null;

        // -----------------------

        /// <summary>
        /// Pauses the game and set its chronos to zero.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Pumpkin)]
        public void Pause() {
            if (!pauseState.IsActive()) {
                pauseState = GameState.CreateState(pauseStateType);
            }
        }

        /// <summary>
        /// Resumes the game state and reset its chronos.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Green)]
        public void Resume() {
            if (pauseState.IsActive()) {
                pauseState.RemoveState();
            }
        }

        // -----------------------

        void IGameStateLifetimeCallback.OnInit(GameState _state) {
            pauseState = _state;

            if (usePauseInterface) {
                pauseInterface.Show();
            }
        }

        void IGameStateLifetimeCallback.OnTerminate(GameState _state) {
            pauseState = null;

            if (usePauseInterface) {
                pauseInterface.Hide();
            }
        }
        #endregion

        #region Pool
        private static readonly ObjectPool<ChronosModifier> pool = new ObjectPool<ChronosModifier>(3);

        // -----------------------

        /// <summary>
        /// Get a <see cref="ChronosModifier"/> instance from the pool.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.GetPoolInstance"/>
        internal ChronosModifier GetChronos(IChronosModifierController _controller) {
            ChronosModifier _instance = pool.GetPoolInstance();
            _instance.SetController(_controller);

            return _instance;
        }

        /// <summary>
        /// Releases a specific <see cref="ChronosModifier"/> instance and sent it back to the pool.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.ReleasePoolInstance(T)"/>
        internal bool ReleaseChronos(ChronosModifier _chronos) {
            return pool.ReleasePoolInstance(_chronos);
        }

        /// <summary>
        /// Clears the <see cref="ChronosModifier"/> pool content.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.ClearPool(int)"/>
        public void ClearPool(int _capacity = 1) {
            pool.ClearPool(_capacity);
        }

        // -------------------------------------------
        // Manager
        // -------------------------------------------

        ChronosModifier IObjectPool<ChronosModifier>.GetPoolInstance() {
            return GetChronos(this);
        }

        bool IObjectPool<ChronosModifier>.ReleasePoolInstance(ChronosModifier _instance) {
            return ReleaseChronos(_instance);
        }

        ChronosModifier IObjectPoolManager<ChronosModifier>.CreateInstance() {
            return new ChronosModifier();
        }

        void IObjectPoolManager<ChronosModifier>.DestroyInstance(ChronosModifier _call) {
            // Cannot destroy the instance, so simply ignore the object and wait for the garbage collector to pick it up.
        }
        #endregion
    }
}
