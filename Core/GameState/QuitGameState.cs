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
    /// <see cref="GameState"/> automatically pushed on the stack when the application is being quit.
    /// </summary>
    [Serializable, Ethereal, DisplayName("Quit")]
    public sealed class QuitGameState : GameState {
        #region Global Members
        /// <summary>
        /// Make sure this state is behind all other gameplay states,
        /// <br/> but in front of <see cref="DefaultGameState"/> for a correct override on <see cref="GameStateOverride.IsQuitting"/>.
        /// </summary>
        public const int PriorityConst = 0;

        public override int Priority {
            get { return PriorityConst; }
        }

        public override bool IsPersistent {
            get { return true; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="QuitGameState"/>
        public QuitGameState() : base(false) { }
        #endregion

        #region State Override
        public override void OnGameStateOverride(GameStateOverride _state) {
            base.OnGameStateOverride(_state);

            // Indicates the game is currently being quit.
            GameManager.IsQuittingApplication = true;
            _state.IsQuitting = true;
        }
        #endregion

        #region Game Quit
        [RuntimeInitializeOnLoadMethod]
        private static void OnInitialization() {
            Application.quitting += OnQuit;
        }

        private static void OnQuit() {
            CreateState<QuitGameState>();
        }
        #endregion
    }
}
