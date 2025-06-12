// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if ENABLE_INPUT_SYSTEM
#define NEW_INPUT_SYSTEM
#endif

#if NEW_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedFramework.Inputs {
    /// <summary>
    /// Interaction used to repeat a button using a specific delay and rate.
    /// </summary>
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    public sealed class RepeatInteraction : IInputInteraction {
        #region Global Members
        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        static RepeatInteraction() {
            InputSystem.RegisterInteraction<RepeatInteraction>();
        }

        // -------------------------------------------
        // Init
        // -------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInPlayer() { } // Will execute the static constructor as a side effect.
        #endregion

        #region Behaviour
        [Tooltip("Initial delay before starting repeating the input (in seconds)")]
        [Min(0f)] public float Delay = .5f;

        [Tooltip("Rate at wich to repeat this input after initial delay (in seconds)")]
        [Min(0f)] public float Rate = .1f;

        [Tooltip("Amount of this input elapsed interaction required before using rate duration instead of delay")]
        [Min(0f)] public int ElapsedBeforeRate = 2;

        [Space(10f)]

        [Tooltip("Amount of actuation required for the control to be considered as pressed.\nIf zero, uses default")]
        [Range(0f, 1f)] public float PressPoint = 0f;

        /// <summary>
        /// Required press point for this interaction.
        /// </summary>
        public float PressPointOrDefault {
            get { return (PressPoint > 0f) ? PressPoint : InputSystem.settings.defaultButtonPressPoint; }
        }

        /// <summary>
        /// Delay used for this interaction timeout (in seconds).
        /// </summary>
        public float TimeoutDelay {
            get { return (elapsedCount >= ElapsedBeforeRate) ? Rate : Delay; }
        }

        // -----------------------

        private InputControl currentControl = null;
        private int elapsedCount = 0;

        // -----------------------

        public void Process(ref InputInteractionContext _context) {

            bool _actuated = _context.ControlIsActuated(PressPointOrDefault);
            InputControl _control  = _context.control;
            InputActionPhase phase = _context.phase;

            // Detect if input is stuck.
            //
            // Stuck can happen when an input is disabled after being started, before being performed or canceled.
            // In this case, the current control is referencing a stuck control that is corrupted.
            // There are two cases to handle:
            //
            // 1. Stuck control is being processed. Control is performed, but actuated and timer has not expired.
            // 2. Other control is being processed. Control is different that current control.
            //
            // In both cases, reset this interaction state and proceed as if the input was in a waiting state,
            // to start and perform it from the beginning.

            bool isStuck = (_control != null) && (((currentControl != null)     && (currentControl != _control))
                                              || ((currentControl == _control)  && (phase == InputActionPhase.Performed) && _actuated && !_context.timerHasExpired));

            if (isStuck) {
                Reset();
                phase = InputActionPhase.Waiting;
            }

            // Process.
            switch (phase) {

                case InputActionPhase.Waiting:

                    // Start interaction.
                    if (_actuated && (currentControl == null)) {

                        currentControl = _control;

                        _context.Started();
                        _context.PerformedAndStayPerformed();
                        _context.SetTimeout(TimeoutDelay);
                    }

                    break;

                case InputActionPhase.Performed:

                    // Ignore any input on a control we're not currently tracking.
                    if (currentControl != _control) {

                        if (currentControl == null) {

                            _context.Canceled();
                            Reset();
                        }

                        return;
                    }

                    if (!_actuated) {

                        if (_context.timerHasExpired) {

                            elapsedCount++;

                            // Perform delay.
                            _context.PerformedAndStayPerformed();
                            _context.SetTimeout(TimeoutDelay);

                        } else {

                            // Cancel.
                            _context.Canceled();
                        }
                    }

                    break;

                default:
                    Reset();
                    break;
            }
        }

        public void Reset() {
            currentControl = null;
            elapsedCount   = 0;
        }
        #endregion
    }
}
#endif
