// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedFramework.Core.GameStates;
using System;

namespace EnhancedFramework.Inputs.GameStates {
    /// <summary>
    /// References all different modes used to enable/disable the associated input.
    /// </summary>
    public enum InputGameStateMode {
        /// <summary>
        /// Enables the associated input at the creation of the state,
        /// and disable it when it terminates.
        /// </summary>
        Creation   = 0,

        /// <summary>
        /// Enables the associated input when the state becomes the active one,
        /// and disable it when it stops to be.
        /// </summary>
        Activation = 1,
    }

    /// <summary>
    /// Base <see cref="GameState{T}"/> class used to automatically enable/disable
    /// a specific <see cref="InputEnhancedAsset"/>, whenever it became the current active state.
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="GameState{T}" path="/typeparam[@name='T']"/></typeparam>
    [Serializable]
    public abstract class InputGameState<T> : GameState<T> where T : GameStateOverride {
        #region Global Members
        /// <summary>
        /// The <see cref="InputEnhancedAsset"/> to manage activation in this <see cref="GameState"/>.
        /// </summary>
        public readonly InputEnhancedAsset Input = null;

        /// <summary>
        /// Determines when to enable/disable the associated input.
        /// </summary>
        public abstract InputGameStateMode Mode { get; }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Creates a new instance of this state, with a specific <see cref="InputEnhancedAsset"/> associated.
        /// </summary>
        /// <param name="_input"><inheritdoc cref="Input" path="/summary"/></param>
        public InputGameState(InputEnhancedAsset _input) : base(true) {
            Input = _input;
        }
        #endregion

        #region Behaviour
        protected override void OnInit() {
            base.OnInit();

            if (Mode == InputGameStateMode.Creation) {
                EnableInput();
            }
        }

        protected override void OnEnabled() {
            base.OnEnabled();

            if (Mode == InputGameStateMode.Activation) {
                EnableInput();
            }
        }

        protected override void OnDisabled() {
            base.OnDisabled();

            if (Mode == InputGameStateMode.Activation) {
                DisableInput();
            }
        }

        protected override void OnTerminate() {
            base.OnTerminate();

            if (Mode == InputGameStateMode.Creation) {
                DisableInput();
            }
        }

        // -----------------------

        /// <summary>
        /// Called when this state associated input is being enabled.
        /// </summary>
        protected virtual void EnableInput() {
            Input.Enable();
        }

        /// <summary>
        /// Called when this state associated input is being disabled.
        /// </summary>
        protected virtual void DisableInput() {
            Input.Disable();
        }
        #endregion
    }
}
