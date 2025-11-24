// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace EnhancedFramework.UI {
    /// <summary>
    /// <see cref="FadingGroupController"/> used to manage UI interaction.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "UI/Fading Group/Interaction [Fading Group Controller]"), DisallowMultipleComponent]
    public sealed class FadingGroupInteractionController : FadingGroupController {
        #region Global Members
        [Section("Interaction [Fading Group Controller]")]

        [Tooltip("If true, fires a callback when the interface cancel button is being performed")]
        [SerializeField] private bool useCancelCallback = false;

        [Tooltip("Audio to play on cancel")]
        [SerializeField] private AudioAsset cancelAudio = null;

        [Space(10f)]

        [Tooltip("Callback to fire when the interface cancel button is being performed")]
        [SerializeField] private UnityEvent onCancel = new UnityEvent();
        #endregion

        #region Callback
        private static readonly PairCollection<FadingGroup, FadingGroupInteractionController> groupBuffer = new PairCollection<FadingGroup, FadingGroupInteractionController>();

        /// <summary>
        /// Called when requesting to enable the interface inputs.
        /// </summary>
        public static event Action OnEnableInputs = null;

        /// <summary>
        /// Called when requesting to disable the interface inputs.
        /// </summary>
        public static event Action OnDisableInputs = null;

        // -------------------------------------------
        // Show
        // -------------------------------------------
        public override void OnShowStarted(FadingGroup _group) {
            base.OnShowStarted(_group);

            // Disable inputs until completion.
            OnDisableInputs?.Invoke();
            SelectionUtility.ResetSelection();
        }

        public override void OnShowPerformed(FadingGroup _group) {
            base.OnShowPerformed(_group);

            // Disable last group.
            if (groupBuffer.SafeLast(out var _active) && (_active.First != _group)) {
                _active.First.SetInteractable(false);
            }

            // Register group.
            groupBuffer.Add(_group, this);
        }

        public override void OnShowCompleted(FadingGroup _group) {
            base.OnShowCompleted(_group);

            // Enable inputs once completed.
            OnEnableInputs?.Invoke();
            SelectionUtility.ApplySelection();
        }

        // -------------------------------------------
        // Hide
        // -------------------------------------------

        public override void OnHideStarted(FadingGroup _group) {
            base.OnHideStarted(_group);

            // Disable inputs until completion.
            OnDisableInputs?.Invoke();
            SelectionUtility.ResetSelection();
        }

        public override void OnHidePerformed(FadingGroup _group) {
            base.OnHidePerformed(_group);

            bool _wasActive = groupBuffer.SafeLast(out var _lastActive) && (_lastActive.First == _group);

            // Unregister group.
            groupBuffer.Remove(_group);

            // Enable last group.
            if (_wasActive && groupBuffer.SafeLast(out var _active)) {
                FadingGroup _activeGroup = _active.First;

                _activeGroup.SetInteractable(true);
                _activeGroup.EnableCanvas(true, true);
            }
        }

        public override void OnHideCompleted(FadingGroup _group) {
            base.OnHideCompleted(_group);

            // Enable inputs once completed.
            if (groupBuffer.Count != 0) {

                OnEnableInputs?.Invoke();
                SelectionUtility.ApplySelection();
            }
        }

        // -------------------------------------------
        // Special
        // -------------------------------------------

        /// <summary>
        /// Can be called when receiving a cancel event from UI, which will execute the last registered associated group callback.
        /// </summary>
        /// <returns>True if a cancel event could be called, false otherwise.</returns>
        public static bool OnGroupCancelEvent() {

            // Call last registered cancel event.
            ref var _bufferSpan = ref groupBuffer.collection;

            for (int i = _bufferSpan.Count; i-- > 0;) {

                FadingGroupInteractionController _controller = _bufferSpan[i].Second;
                if (_controller.useCancelCallback) {

                    // Call event.
                    _controller.onCancel.Invoke();

                    // Audio.
                    AudioAsset _audio = _controller.cancelAudio;
                    if (_audio.IsValid()) {
                        _audio.PlayAudio();
                    }

                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
