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
    /// <see cref="FadingGroupEffect"/> playing an idle animation while a source <see cref="FadingGroup"/> is visible.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(MenuPath + "Idle Anim - [Fading Group Effect]")]
    #pragma warning disable
    public sealed class IdleAnimationFadingGroupEffect : FadingGroupEffect {
        #region Global Members
        [Section("Idle Animation [Fading Group Effect]"), PropertyOrder(0)]

        [Tooltip("RectTransform to animate")]
        [SerializeField, Enhanced, Required] private RectTransform rectTransform = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f), PropertyOrder(2)]

        [Tooltip("If true, animations not be affected by the game time scale")]
        [SerializeField] private bool realTime = false;

        [Tooltip("Total duration of this animation (in seconds)")]
        [SerializeField, Enhanced, Range(0f, 10f)] private float duration = .5f;

        [Tooltip("Delay before stopping this animation when the group is hidden (in seconds)")]
        [SerializeField, Enhanced, Range(0f, 10f)] private float stopDelay = 1f;

        [Space(10f)]

        [Tooltip("Offset to apply to this RectTransform position")]
        [SerializeField] private Vector2 positionOffset = Vector2.one;

        [Tooltip("Offset to apply to this RectTransform scale")]
        [SerializeField] private Vector2 scale          = Vector2.one;

        [Space(5f)]

        [Tooltip("Curve used to evaluate this position animation")]
        [SerializeField, Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.HarvestGold)] private AnimationCurve positionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Tooltip("Curve used to evaluate this size animation")]
        [SerializeField, Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Crimson)] private AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

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
            if (!rectTransform) {
                rectTransform = GetComponent<RectTransform>();
            }
        }
        #endif
        #endregion

        #region Effect
        private TweenCallback onKilledCallback = null;
        private Action stopAnimationCallback   = null;

        private Vector2 originalPosition = Vector2.zero;
        private Vector2 originalSize     = Vector2.zero;

        private DelayHandler delay = default;
        private Sequence sequence  = null;

        // -----------------------

        protected internal override void OnSetVisibility(bool _visible, bool _instant) {

            if (!_visible) {

                // Stop.
                if (!delay.IsValid && sequence.IsActive()) {

                    stopAnimationCallback ??= StopAnimation;
                    delay = Delayer.Call(stopDelay, stopAnimationCallback, true);
                }

                return;

                // ----- Local Method ----- \\

                void StopAnimation(){
                    sequence.DoKill();
                }
            }

            delay.Cancel();

            // Already playing.
            if (sequence.IsActive()) {
                return;
            }

            originalPosition = rectTransform.anchoredPosition;
            originalSize     = rectTransform.sizeDelta;

            // Animation.
            sequence = DOTween.Sequence(); {

                onKilledCallback ??= OnKilled;

                sequence.Join(rectTransform.DOAnchorPos(originalPosition + positionOffset, duration, false).SetEase(positionCurve));
                sequence.Join(rectTransform.DOScale(originalSize + scale, duration).SetEase(scaleCurve));

                sequence.SetLoops(-1, LoopType.Restart);
                sequence.SetUpdate(realTime).SetRecyclable(true).SetAutoKill(false).OnKill(onKilledCallback);
            }

            isActive = true;

            // ----- Local Method ----- \\

            void OnKilled() {
                sequence = null;
                isActive = false;

                rectTransform.anchoredPosition = originalPosition;
                rectTransform.sizeDelta = originalSize;
            }
        }

        private void Stop() {
            sequence.DoKill();
            delay.Cancel();
        }
        #endregion
    }
}
#endif
