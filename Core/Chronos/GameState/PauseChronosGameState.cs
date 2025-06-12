// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using UnityEngine;

namespace EnhancedFramework.Core.GameStates {
    /// <summary>
    /// Base interface for every pause <see cref="GameState"/>.
    /// <br/> Used within a <see cref="SerializedType{T}"/> instead of the generic <see cref="PauseChronosGameState{T}"/> class.
    /// </summary>
    internal interface IPauseChronosState { }

    // ===== Base ===== \\

    /// <summary>
    /// Base <see cref="GameState{T}"/> to inherit your own pause state from.
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="GameState{T}" path="/typeparam[@name='T']"/></typeparam>
    [Serializable]
    public abstract class PauseChronosGameState<T> : GameState<T>, IPauseChronosState where T : GameStateOverride {
        #region Global Members
        // Prevent from discarding this state.
        public override bool IsPersistent {
            get { return true; }
        }

        public override IGameStateLifetimeCallback LifetimeCallback {
            get { return ChronosManager.Instance; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="PauseChronosGameState{T}"/>
        public PauseChronosGameState() : base(false) { }
        #endregion

        #region State Override
        public override void OnStateOverride(T _state) {
            base.OnStateOverride(_state);

            // Set the cursor free.
            #if UNITY_EDITOR
            _state.IsCursorVisible = true;
            _state.CursorLockMode  = CursorLockMode.None;
            #endif

            _state.IsPaused      = true;
            _state.FreezeChronos = true;
        }
        #endregion
    }

    // ===== Derived ===== \\

    /// <summary>
    /// Defualt state used to completely pauses the game and set its chronos to 0 while active.
    /// </summary>
    [Serializable, DisplayName("Chronos/Pause [Default]")]
    public sealed class DefaultPauseChronosGameState : PauseChronosGameState<GameStateOverride> {
        #region Global Members
        /// <summary>
        /// One of the highest priority, to prevent most of the other states to continue updating.
        /// </summary>
        public const int PriorityConst = int.MaxValue - 999;

        public override int Priority {
            get { return PriorityConst; }
        }
        #endregion
    }
}
