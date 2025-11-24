// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedFramework.UI {
    /// <summary>
    /// Enum referencing all <see cref="Selectable"/> possible states.
    /// </summary>
    public enum SelectableState {
        Normal      = 0,

        Highlighted = 1,
        Pressed     = 2,
        Selected    = 3,
        Disabled    = 4,
    }

    /// <summary>
    /// Base class to inherit your own <see cref="EnhancedSelectable"/> effects from.
    /// </summary>
    public abstract class EnhancedSelectableEffect : EnhancedBehaviour {
        public const string MenuPath = FrameworkUtility.MenuPath + "UI/Effect/";

        #region Behaviour
        /// <summary>
        /// Called when changing the selection state of the associated selectable.
        /// </summary>
        /// <param name="_selectable">Source selectable of this effect.</param>
        /// <param name="_state"><see cref="SelectableState"/> of the button.</param>
        /// <param name="_instant">Whether the effect should be applied instantly or not.</param>
        public abstract void OnSelectionState(EnhancedSelectable _selectable, SelectableState _state, bool _instant);
        #endregion
    }

    /// <summary>
    /// Enhanced <see cref="Selectable"/> behaviour, being automatically selected on mouse hover and with many additional options.
    /// </summary>
    [ScriptGizmos(false, true)]
    public abstract class EnhancedSelectable : Selectable {
        #region Global Members
        [Tooltip("Automatically selects this selectable whenever its GameObject gets enabled")]
        [SerializeField, HideInInspector] public bool AutoSelectOnEnabled = false;

        [Tooltip("Automatically deselects this selectable whenever the pointer exits this object")]
        [SerializeField, HideInInspector] public bool DeselectOnPointerExit = true;

        [Space(10f)]

        [SerializeField, HideInInspector] public EnhancedSelectableEffect[] Effects = new EnhancedSelectableEffect[0];

        // -----------------------

        [SerializeField, Enhanced, ReadOnly] private FadingObjectBehaviour group = null;

        // -----------------------

        /// <summary>
        /// Is this selectable currently selected?
        /// </summary>
        public bool IsSelected {
            get {
                switch (currentSelectionState) {

                    case SelectionState.Pressed:
                    case SelectionState.Selected:
                        return true;

                    case SelectionState.Disabled:
                    case SelectionState.Normal:
                    case SelectionState.Highlighted:
                    default:
                        return false;
                }
            }
        }
        #endregion

        #region Mono Behaviour
        protected override void OnEnable() {
            base.OnEnable();
            
            // Selection.
            if (AutoSelectOnEnabled) {
                Select();
            } else {
                OnDeselect(null);
            }
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {

            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                return;
            }

            // Find nearest parent group.
            Transform _transform = transform;
            FadingObjectBehaviour _object;

            while ((_transform != null) && ((_object = _transform.GetComponentInParent<FadingObjectBehaviour>()) != null)){

                if ((_object.FadingObject is FadingGroup _group) && _group.UseSelectable) {

                    if (group != _object) {
                        group = _object;
                        EditorUtility.SetDirty(this);
                    }

                    return;
                }

                _transform = _transform.parent;
            }
        }
#endif
        #endregion

        #region Selectable
        private bool isDeselecting = false;

        // -----------------------

        public override void OnPointerEnter(PointerEventData _eventData) {
            base.OnPointerEnter(_eventData);

            // Select on hover.
            Select();
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);

            // Deselect.
            if (DeselectOnPointerExit && !isDeselecting) {

                #if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return;
                }
                #endif

                EventSystem _eventSystem = EventSystem.current;
                if (_eventSystem.currentSelectedGameObject == gameObject) {

                    isDeselecting = true;
                    _eventSystem.SetSelectedGameObject(null);
                    isDeselecting = false;
                }
            }
        }

        public override void OnDeselect(BaseEventData _eventData) {
            base.OnDeselect(_eventData);

            // Simulate pointer exit.
            isDeselecting = true;
            OnPointerExit(null);
            isDeselecting = false;
        }

        public override void Select() {
            base.Select();

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif

            // Set as last selectable.
            if ((group != null) && (group.FadingObject is FadingGroup _group)) {
                _group.ActiveSelectable = this;
            }
        }

        protected override void DoStateTransition(SelectionState _state, bool _instant) {
            base.DoStateTransition(_state, _instant);

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif

            // Force selection.
            if (_state == SelectionState.Selected) {
                Select();
            }

            // Apply effects.
            SelectableState _selectableState = GetSelectableState(_state);

            ref EnhancedSelectableEffect[] _span = ref Effects;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                EnhancedSelectableEffect _effect = _span[i];

                #if DEVELOPMENT
                // Security.
                if (_effect == null) {
                    this.LogErrorMessage($"Null effect at index {i}");
                    continue;
                }
                #endif

                _effect.OnSelectionState(this, _selectableState, _instant);
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the current state of this button.
        /// </summary>
        /// <returns>The current state of this button.</returns>
        public SelectableState GetState() {
            return GetSelectableState(currentSelectionState);
        }

        private SelectableState GetSelectableState(SelectionState _state) {
            switch (_state) {
                case SelectionState.Normal:
                default:
                    return SelectableState.Normal;

                case SelectionState.Highlighted:
                    return SelectableState.Highlighted;

                case SelectionState.Pressed:
                    return SelectableState.Pressed;

                case SelectionState.Selected:
                    return SelectableState.Selected;

                case SelectionState.Disabled:
                    return SelectableState.Disabled;

            }
        }
        #endregion
    }
}
