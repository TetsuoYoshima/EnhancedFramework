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
    /// Enables the <see cref="DeveloperConsole"/> while on the stack.
    /// </summary>
    [Serializable, DisplayName("Utility/Developer Console")]
    public sealed class DeveloperConsoleGameState : GameState {
        #region Global Members
        /// <summary>
        /// Don't need a high priority, just enough to remain above the default state.
        /// </summary>
        public const int PriorityConst = 99999;

        public override int Priority {
            get { return PriorityConst; }
        }

        // Prevent from discarding this state.
        public override bool IsPersistent {
            get { return true; }
        }

        public override IGameStateLifetimeCallback LifetimeCallback {
            get { return DeveloperConsole.Instance; }
        }
        #endregion

        #region State Override
        public override void OnGameStateOverride(GameStateOverride _state) {
            base.OnGameStateOverride(_state);

            // Set the cursor free to use the console.
            _state.IsCursorVisible = true;
            _state.CursorLockMode  = CursorLockMode.None;

            _state.IsPaused = true;
        }
        #endregion
    }
}
