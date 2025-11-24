// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.GameStates;
using System;
using UnityEngine;

namespace EnhancedFramework.DeveloperConsoleSystem.GameStates {
    /// <summary>
    /// Base interface for every <see cref="DeveloperConsole"/>-related <see cref="GameState"/>.
    /// <br/> Used within a <see cref="SerializedType{T}"/> instead of the generic <see cref="DeveloperConsoleGameState{T}"/> class.
    /// </summary>
    public interface IDeveloperConsoleState { }

    // ===== Base ===== \\

    /// <summary>
    /// Enables the <see cref="DeveloperConsole"/> while on the stack.
    /// </summary>
    [Serializable]
    public abstract class DeveloperConsoleGameState<T> : GameState<T>, IDeveloperConsoleState where T : GameStateOverride {
        #region Global Members
        // Prevent from discarding this state.
        public override bool IsPersistent {
            get { return true; }
        }

        public override IGameStateLifetimeCallback LifetimeCallback {
            get { return DeveloperConsole.Instance; }
        }
        #endregion

        #region State Override
        public override void OnStateOverride(T _state) {
            base.OnStateOverride(_state);

            // Set the cursor free to use the console.
            _state.IsCursorVisible = true;
            _state.CursorLockMode  = CursorLockMode.None;

            _state.IsPaused = true;
        }
        #endregion
    }

    // ===== Derived ===== \\

    /// <summary>
    /// Default state active while the <see cref="DeveloperConsole"/> is active.
    /// </summary>
    [Serializable, DisplayName("Utility/Developer Console [Default]")]
    public sealed class DefaultDeveloperConsoleGameState : DeveloperConsoleGameState<GameStateOverride> {
        #region Global Members
        /// <summary>
        /// Don't need a high priority, just enough to remain above the default state.
        /// </summary>
        public const int PriorityConst = 99999;

        public override int Priority {
            get { return PriorityConst; }
        }
        #endregion
    }
}
