// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if DOTWEEN_ENABLED
using DG.Tweening;
using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.UI {
    /// <summary>
    /// <see cref="FadingGroupEffect"/> playing a looping blink animation on a <see cref="CanvasGroup"/> while a source <see cref="FadingGroup"/> is visible.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(MenuPath + "Blink [Fading Group Effect]")]
    #pragma warning disable
    public sealed class BlinkFadingGroupEffect : FadingGroupEffect {
        #region Global Members
        [Section("Blink [Fading Group Effect]"), PropertyOrder(0)]

        [Tooltip("Group to blink alpha value")]
        [SerializeField, Enhanced, Required] private CanvasGroup group = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f), PropertyOrder(2)]

        [Tooltip("If true, animations not be affected by the game time scale")]
        [SerializeField] private bool realTime = false;

        [Tooltip("Total animation duration (in seconds)")]
        [SerializeField, Enhanced, Range(0f, 30f)] private float duration = 1f;

        [Tooltip("Delay before stopping this animation when the source group is hidden (in seconds)")]
        [SerializeField, Enhanced, Range(0f, 10f)] private float stopDelay = 1f;

        [Space(10f)]

        [Tooltip("Blink animation evaluation curve")]
        [SerializeField, Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.HarvestGold)] private AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Debug state of this effect animation")]
        [SerializeField, Enhanced, ReadOnly] private bool isActive = false;
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();
            Stop();
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            
            // Reference.
            if (!group) {
                group = GetComponent<CanvasGroup>();
            }
        }
        #endif
        #endregion

        #region Effect
        private TweenCallback onKilledCallback = null;
        private Action stopAnimationCallback   = null;

        private DelayHandler delay = default;
        private Tween tween        = null;

        // -----------------------

        protected internal override void OnSetVisibility(bool _visible, bool _instant) {

            if (!_visible) {

                // Stop.
                if (!delay.IsValid && tween.IsActive()) {

                    stopAnimationCallback ??= StopAnimation;
                    delay = Delayer.Call(stopDelay, stopAnimationCallback, true);
                }

                return;

                // ----- Local Method ----- \\

                void StopAnimation(){
                    tween.DoKill();
                }
            }

            delay.Cancel();

            // Already playing.
            if (tween.IsActive()) {
                return;
            }

            // Blink.
            group.alpha = 0f;

            onKilledCallback ??= OnKilled;

            tween = group.DOFade(1f, duration).SetEase(blinkCurve).SetLoops(-1, LoopType.Restart);
            tween.SetUpdate(realTime).SetRecyclable(true).SetAutoKill(false).OnKill(onKilledCallback);

            isActive = true;

            // ----- Local Method ----- \\

            void OnKilled() {
                tween    = null;
                isActive = false;

                group.alpha = 0f;
            }
        }

        private void Stop() {
            delay.Cancel();
            tween.DoKill();
        }
        #endregion
    }
}
#endif
