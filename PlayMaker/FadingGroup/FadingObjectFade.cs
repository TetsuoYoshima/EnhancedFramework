// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedFramework.Core;
using HutongGames.PlayMaker;
using System;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// Base abstract <see cref="FsmStateAction"/> used to fade an <see cref="IFadingObject"/>.
    /// </summary>
    public abstract class FadingObjectFade : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Mode - Instant - Wait - Event
        // -------------------------------------------

        [Tooltip("Fading Mode used to fade the group.")]
        [RequiredField, ObjectType(typeof(FadingMode))]
        public FsmEnum FadingMode = null;

        [Tooltip("Whether to fade the group instantly or not.")]
        public FsmBool Instant = null;

        [Tooltip("Whether to fade the group instantly or not.")]
        [HideIf(nameof(HideWaitDuration))]
        public FsmFloat WaitDuration = null;

        [Tooltip("Event to send when fade is completed.")]
        public FsmEvent CompletedEvent;

        // -----------------------

        /// <summary>
        /// The <see cref="IFadingObject"/> to fade.
        /// </summary>
        public abstract IFadingObject FadingObject { get; }

        public bool HideWaitDuration() {
            return (((FadingMode)FadingMode.Value) != Core.FadingMode.FadeInOut) || Instant.Value;
        }
        #endregion

        #region Behaviour
        private Action<bool> onFadeComplete = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            CompletedEvent = null;
            WaitDuration   = null;
            FadingMode     = null;
            Instant        = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            Fade();
            Finish();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void Fade() {
            IFadingObject _fadingObject = FadingObject;

            if (_fadingObject != null) {

                onFadeComplete ??= OnComplete;
                PerformFade(_fadingObject, (FadingMode)FadingMode.Value, Instant.Value, onFadeComplete, WaitDuration.Value);
            }

            // ----- Local Method ----- \\

            void OnComplete(bool _completed) {
                Fsm.Event(CompletedEvent);
            }
        }

        protected virtual void PerformFade(IFadingObject _fadingObject, FadingMode _fadingMode, bool _instant, Action<bool> _onComplete, float _waitDuration) {
            _fadingObject.Fade((FadingMode)FadingMode.Value, Instant.Value, _onComplete, WaitDuration.Value);
        }
        #endregion
    }
}
