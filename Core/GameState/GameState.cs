// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnhancedFramework.Core.GameStates {
    /// <summary>
    /// Use this to receive a callback when a <see cref="GameState"/> has been pushed or removed from the stack.
    /// <para/>
    /// Associate this callback with a specific <see cref="GameState"/> using <see cref="GameState.LifetimeCallback"/>.
    /// </summary>
    public interface IGameStateLifetimeCallback {
        #region Content
        /// <summary>
        /// Called when a <see cref="GameState"/> has been pushed on stack.
        /// </summary>
        /// <param name="_state">The <see cref="GameState"/> being initialized.</param>
        void OnInit(GameState _state);

        /// <summary>
        /// Called when a <see cref="GameState"/> has been removed from the stack.
        /// </summary>
        /// <param name="_state">The <see cref="GameState"/> being terminated.</param>
        void OnTerminate(GameState _state);
        #endregion
    }

    /// <summary>
    /// Use this to receive a callback when a <see cref="GameState"/> has been enabled or disabled,
    /// <br/> that is when it starts or stops from being the active state.
    /// <para/>
    /// Associate this callback with a specific <see cref="GameState"/> using <see cref="GameState.ActivationCallback"/>.
    /// </summary>
    public interface IGameStateActivationCallback {
        #region Content
        /// <summary>
        /// Called when a <see cref="GameState"/> is being enabled and set as the currently active one.
        /// </summary>
        /// <param name="_state">The <see cref="GameState"/> being enabled.</param>
        void OnEnable(GameState _state);

        /// <summary>
        /// Called when a <see cref="GameState"/> is being disabled.
        /// </summary>
        /// <param name="_state">The <see cref="GameState"/> being disabled.</param>
        void OnDisable(GameState _state);
        #endregion
    }

    /// <summary>
    /// Base class to inherit your own game states from.
    /// <para/>
    /// Note that inheriting from this class will only give you access to the base <see cref="GameStateOverride"/> value.
    /// <br/> For getting access to a specific override type, please use <see cref="GameState{T}"/> instead.
    /// <para/>
    /// Newly created <see cref="GameState"/> are automatically pushed on the stack.
    /// </summary>
    [Serializable]
    public abstract class GameState : IComparable<GameState> {
        #region Default Callback
        /// <summary>
        /// Default <see cref="GameState"/> callback receiver class used when no other callback has been specified,
        /// <br/> inheriting from both <see cref="IGameStateLifetimeCallback"/> and <see cref="IGameStateActivationCallback"/>.
        /// </summary>
        public class DefaultCallbackReceiver : IGameStateLifetimeCallback, IGameStateActivationCallback {
            public void OnInit(GameState _state) { }

            public void OnEnable(GameState _state) { }

            public void OnDisable(GameState _state) { }

            public void OnTerminate(GameState _state) { }
        }
        #endregion

        #region State
        /// <summary>
        /// Used to indicate the current life state of a <see cref="GameState"/>.
        /// </summary>
        public enum Lifetime {
            Created  = 0,
            Pending  = 1,
            OnStack  = 2,
            Inactive = 3,
        }
        #endregion

        #region Global Members
        /// <summary>
        /// The default <see cref="IGameStateActivationCallback"/> and <see cref="IGameStateLifetimeCallback"/> callback receiver.
        /// </summary>
        public static readonly DefaultCallbackReceiver DefaultCallback = new DefaultCallbackReceiver();

        // -----------------------

        /// <summary>
        /// Priority of this state. The state with the higher priority in the list is the one enabled.
        /// <br/> Note that the priority value must (absolutely) be unique for each different class type.
        /// </summary>
        public abstract int Priority { get; }

        /// <summary>
        /// Persistent states should be preserved between scene loadings.
        /// <br/> Use this to clear the state stack when the level changed.
        /// </summary>
        public virtual bool IsPersistent {
            get { return false; }
        }

        /// <summary>
        /// If false, allows only one instance of this <see cref="GameState"/> type to be on the stack at a time.
        /// </summary>
        public virtual bool MultipleInstance {
            get { return false; }
        }

        /// <summary>
        /// If true, pops this state on the next frame.
        /// </summary>
        public virtual bool PopOnNextFrame {
            get { return false; }
        }

        /// <summary>
        /// Callback receiver for whenever this <see cref="GameState"/> is being pushed or removed from the stack.
        /// </summary>
        public virtual IGameStateLifetimeCallback LifetimeCallback {
            get { return DefaultCallback; }
        }

        /// <summary>
        /// Callback receiver for whenever this <see cref="GameState"/> is being enabled or disabled.
        /// </summary>
        public virtual IGameStateActivationCallback ActivationCallback {
            get { return DefaultCallback; }
        }

        /// <summary>
        /// Get if this state is the currently active and enabled state or not.
        /// </summary>
        public bool IsCurrentState {
            get { return GameStateManager.Instance.CurrentState == this; }
        }

        /// <summary>
        /// The current life state of this <see cref="GameState"/>.
        /// </summary>
        public Lifetime LifeState { get; private set; } = Lifetime.Created;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="GameState(bool)"/>
        public GameState() : this(true) { }

        /// <summary>
        /// Creates a new state and automatically push it on the stack.
        /// </summary>
        /// <param name="_pushOnNextFrame">Should the state be pushed on the stack during this frame or the next one?</param>
        protected GameState(bool _pushOnNextFrame) {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif

            if (_pushOnNextFrame) {
                GameStateManager.Instance.PushStateOnNextFrame(this);
            } else {
                GameStateManager.Instance.PushState(this);
            }
        }
        #endregion

        #region Creation / Destruction
        /// <summary>
        /// Creates a new game state, that is automatically added on the stack.
        /// </summary>
        /// <typeparam name="T">The type of game state to create.</typeparam>
        /// <returns>The newly created state.</returns>
        public static T CreateState<T>() where T : GameState, new() {
            return Activator.CreateInstance<T>();
        }

        /// <param name="_stateType"><inheritdoc cref="CreateState{T}" path="/typeparam[@name='T']"/></param>
        /// <inheritdoc cref="CreateState{T}"/>
        public static GameState CreateState(Type _stateType) {
            if (!_stateType.IsSubclassOf(typeof(GameState))) {
                Debug.LogError($"GameState - The type \'{_stateType.Name}\' does not inherit from \'{typeof(GameState).Name}\'");
                return null;
            }

            return Activator.CreateInstance(_stateType) as GameState;
        }

        /// <inheritdoc cref="GameStateManager.PopState{T}"/>
        public static bool RemoveState<T>() where T : GameState {
            return GameStateManager.Instance.PopState<T>();
        }

        /// <inheritdoc cref="GameStateManager.PopState(Type)"/>
        public static bool RemoveState(Type _stateType) {
            return GameStateManager.Instance.PopState(_stateType);
        }

        /// <summary>
        /// Removes this state from the game stack.
        /// </summary>
        public void RemoveState(bool _fallbackOnNextFrame = false) {
            if (LifeState != Lifetime.Inactive) {

                if (!PopOnNextFrame && (GameStateManager.Instance.PopState(this) || !_fallbackOnNextFrame)) {
                    return;
                }

                GameStateManager.Instance.PopStateOnNextFrame(this);
            }
        }

        // -----------------------

        /// <typeparam name="T"><inheritdoc cref="ToggleState(Type)" path="/param[@name='_type']"/></typeparam>
        /// <inheritdoc cref="ToggleState(Type)"/>
        public static T ToggleState<T>() where T : GameState, new() {
            if (IsActive<T>(out GameState _state)) {
                _state.RemoveState();
                return null;
            }

            _state = CreateState<T>();
            return _state as T;
        }

        /// <summary>
        /// Creates a new state of the given type if none is currently active,
        /// or remove it from the stack.
        /// </summary>
        /// <param name="_type">The <see cref="GameState"/> type to toggle.</param>
        /// <returns>The <see cref="GameState"/> of the given type instance (null if removed).</returns>
        public static GameState ToggleState(Type _type) {
            if (IsActive(_type, out GameState _state)) {
                _state.RemoveState();
                return null;
            }

            _state = CreateState(_type);
            return _state;
        }
        #endregion

        #region State Override
        /// <summary>
        /// Implement this to override and modify shared various values on the game global state.
        /// </summary>
        /// <param name="_state">Global game state shared values to override.</param>
        public virtual void OnGameStateOverride(GameStateOverride _state) { }

        /// <summary>
        /// Implement this to override the game global chtonos (time scale) value. 
        /// </summary>
        /// <param name="_chronos">The new chronos value to apply.</param>
        /// <param name="_priority">The priority of this chronos (only the one with the highest priority will be applied).</param>
        /// <returns>True to override the game chronos, false otheriwse.</returns>
        public virtual bool OverrideChronos(out float _chronos, out int _priority) {
            _chronos  = 1f;
            _priority = -1;

            return false;
        }
        #endregion

        #region Behaviour
        internal void OnPushedOnStack() {
            LifeState = Lifetime.OnStack;
            OnInit();
        }

        internal void OnRemovedFromStack() {
            LifeState = Lifetime.Inactive;
            OnTerminate();
        }

        internal void OnPushedOnPending() {
            LifeState = Lifetime.Pending;
        }

        // -----------------------

        /// <summary>
        /// Called when this state has been pushed on stack.
        /// </summary>
        protected virtual void OnInit() {
            LifetimeCallback.OnInit(this);
        }

        /// <summary>
        /// Called when this state is being enabled and set as the currently active one.
        /// </summary>
        protected internal virtual void OnEnabled() {
            ActivationCallback.OnEnable(this);
        }

        /// <summary>
        /// Called each and every frame during update while this state is active.
        /// </summary>
        protected internal virtual void OnUpdate() { }

        /// <summary>
        /// Called when this state is being disabled.
        /// </summary>
        protected internal virtual void OnDisabled() {
            ActivationCallback.OnDisable(this);
        }

        /// <summary>
        /// Called when this state has been removed from the stack.
        /// </summary>
        protected virtual void OnTerminate() {
            LifetimeCallback.OnTerminate(this);
        }
        #endregion

        #region Comparer
        int IComparable<GameState>.CompareTo(GameState _other) {
            return Priority.CompareTo(_other.Priority);
        }
        #endregion

        #region Utility
        public override string ToString() {
            return GetType().Name;
        }

        /// <inheritdoc cref="GameStateManager.IsActive{T}(out GameState, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsActive<T>(out GameState _state, bool _inherit = true) where T : GameState {
            return GameStateManager.Instance.IsActive<T>(out _state, _inherit);
        }

        /// <inheritdoc cref="GameStateManager.IsActive(Type, out GameState, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsActive(Type _type, out GameState _state, bool _inherit = true) {
            return GameStateManager.Instance.IsActive(_type, out _state, _inherit);
        }
        #endregion
    }

    /// <summary>
    /// Base class to inherit your own game states from, associated with a specific override type.
    /// <para/>
    /// Inherit from this class instead of <see cref="GameState"/> to get access to a specific override type.
    /// </summary>
    /// <typeparam name="T">Override type to be associated with this state.</typeparam>
    [Serializable]
    public abstract class GameState<T> : GameState where T : GameStateOverride {
        #region Constructors
        /// <inheritdoc cref="GameState(bool)"/>
        public GameState() : base() { }

        /// <inheritdoc cref="GameState(bool)"/>
        protected GameState(bool _pushOnNextFrame) : base(_pushOnNextFrame) { }
        #endregion

        #region State Override
        public override sealed void OnGameStateOverride(GameStateOverride _state) {
            base.OnGameStateOverride(_state);
            OnStateOverride(_state as T);
        }

        /// <inheritdoc cref="OnGameStateOverride(GameStateOverride)"/>
        public virtual void OnStateOverride(T _state) { }
        #endregion
    }

    // ===== Extensions ===== \\

    /// <summary>
    /// Contains <see cref="GameState"/>-related extension utility method(s).
    /// </summary>
    public static class GameStateExtensions {
        #region Content
        /// <summary>
        /// Get if this state is currently active (not null and on the stack) or not.
        /// </summary>
        /// <param name="_state">The <see cref="GameState"/> to check.</param>
        /// <returns>True if this state is not null and currently on the stack, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsActive(this GameState _state) {
            return (_state != null) && (_state.LifeState != GameState.Lifetime.Inactive);
        }
        #endregion
    }
}
