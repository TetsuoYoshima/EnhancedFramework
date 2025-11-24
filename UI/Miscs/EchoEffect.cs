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
    /// UI component using various delayed objects to simulate an echo when a target <see cref="RectTransform"/> has changed.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "UI/Echo Effect")]
    public sealed class EchoEffect : EnhancedBehaviour, ILateUpdate {
        #region Echo
        /// <summary>
        /// <see cref="EchoEffect"/>-related single echo settings wrapper.
        /// </summary>
        [Serializable]
        public sealed class Echo {
            [Tooltip("Transform of this echo")]
            [Enhanced, Required] public RectTransform Transform = null;

            [Tooltip("Duration of this echo animations (in seconds)")]
            [Enhanced, Range(0f, 10f)] public float Duration = .5f;

            [Tooltip("Delay of this echo animations (in seconds)")]
            [Enhanced, Range(0f, 5f)] public float Delay = 0f;

            [Tooltip("Animation ease of this echo")]
            public Ease Ease = Ease.OutSine;

            // -----------------------

            /// <summary>
            /// Creates a tween to move this echo to a given position.
            /// </summary>
            /// <param name="_anchoredPosition">Destination position of this echo transform.</param>
            /// <returns>This echo movement tween.</returns>
            public Tween Move(Vector2 _anchoredPosition) {
                return Transform.DOAnchorPos(_anchoredPosition, Duration, false).SetEase(Ease).SetDelay(Delay);
            }

            /// <summary>
            /// Creates a tween to modify this echo to a given size.
            /// </summary>
            /// <param name="_size">End size of this echo transform.</param>
            /// <returns>This echo size tween.</returns>
            public Tween SizeDelta(Vector2 _size) {
                return Transform.DOSizeDelta(_size, Duration).SetEase(Ease).SetDelay(Delay);
            }

            /// <summary>
            /// Creates a tween to modify this echo to a given scale.
            /// </summary>
            /// <param name="_scale">End scale of this echo transform.</param>
            /// <returns>This echo scale tween.</returns>
            public Tween Scale(float _scale) {
                return Transform.DOScale(_scale, Duration * .5f).SetEase(Ease);
            }
        }
        #endregion

        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init | UpdateRegistration.Late;

        #region Global Members
        [Section("Echo Effect")]

        [Tooltip("Target RectTransform for this echo to follow")]
        [SerializeField, Enhanced, Required] private RectTransform target = null;

        [Tooltip("If true, this echo animations will not be affected by the game time scale")]
        [SerializeField] private bool realTime = true;

        [Space(10f)]

        [Tooltip("All echoes of this effect")]
        [SerializeField] private Echo[] echoes = new Echo[0];
        #endregion

        #region Enhanced Behaviour
        protected override void OnInit() {
            base.OnInit();

            // Instantly update this effect.
            UpdateEffect();

            positionSequence.DoComplete(true);
            sizeSequence    .DoComplete(true);
        }

        void ILateUpdate.Update() {
            UpdateEffect();
        }
        #endregion

        #region Behaviour
        private TweenCallback onKillPositionSequence = null;
        private TweenCallback onKillSizeSequence     = null;

        private Sequence positionSequence = null;
        private Sequence sizeSequence    = null;

        private Vector2 lastPosition = Vector2.zero;
        private Vector2 lastSize     = Vector2.zero;

        // -----------------------

        /// <summary>
        /// Updates this echo effect.
        /// </summary>
        private void UpdateEffect() {

            // No echo.
            if (echoes.Length == 0) {
                return;
            }

            Vector2 _position = target.anchoredPosition;
            Vector2 _size = target.sizeDelta;

            // Position.
            if (_position != lastPosition) {

                lastPosition = _position;
                positionSequence.DoKill();

                positionSequence = DOTween.Sequence(); {

                    ref Echo[] _span = ref echoes;
                    int _count = _span.Length;

                    for (int i = 0; i < _count; i++) {
                        positionSequence.Join(_span[i].Move(_position));
                    }
                }

                onKillPositionSequence ??= OnPositionKilled;
                positionSequence.SetUpdate(realTime).SetRecyclable(true).SetAutoKill(true).OnKill(onKillPositionSequence);
            }

            // Size.
            if (_size != lastSize) {

                lastSize = _size;
                sizeSequence.DoKill();

                sizeSequence = DOTween.Sequence(); {

                    float _scale = (Mathm.ApproximatelyZero(_size.x) && Mathm.ApproximatelyZero(_size.y))
                                 ? 0f : 1f;

                    ref Echo[] _span = ref echoes;
                    int _count = _span.Length;

                    for (int i = 0; i < _count; i++) {
                        Echo _echo = _span[i];

                        sizeSequence.Join(_echo.SizeDelta(_size));
                        sizeSequence.Join(_echo.Scale(_scale));
                    }

                }

                onKillSizeSequence ??= OnSizeKilled;
                sizeSequence.SetUpdate(realTime).SetRecyclable(true).SetAutoKill(true).OnKill(onKillSizeSequence);
            }

            // Instant.
            if (!SelectionUtility.IsSelection) {

                positionSequence.Kill(true);
                sizeSequence    .Kill(true);
            }

            // ----- Local Methods ----- \\

            void OnPositionKilled() {
                positionSequence = null;
            }

            void OnSizeKilled() {
                sizeSequence = null;
            }
        }
        #endregion
    }
}
#endif
