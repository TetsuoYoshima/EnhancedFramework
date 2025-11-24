// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.UI {
    /// <summary>
    /// Enhanced <see cref="EnhancedOptionUI"/> for an option with multiple available choices.
    /// </summary>
    [ScriptGizmos(false, true)]
    #pragma warning disable
    public sealed class EnhancedOptionSliderUI : EnhancedOptionUI, IDragHandler, ICanvasElement, IInitializePotentialDragHandler {
        #region Parameters
        /// <summary>
        /// <see cref="EnhancedOptionSelectionUI"/>-related UI parameters.
        /// </summary>
        [Flags]
        public enum Parameters {
            None = 0,
            Text = 1 << 0,
        }
        #endregion

        #region Global Members
        [Space(10f)]

        [Tooltip("Determines how selection navigation is interpreted")]
        [SerializeField] private SelectionMovement direction = SelectionMovement.Horizontal;

        [Tooltip("Parameters of this option")]
        [SerializeField] private Parameters parameters = Parameters.None;

        [Space(10f)]

        [Tooltip("RectTransform of the manipulating handle of this slider")]
        [SerializeField] private RectTransform handleRect = null;

        [Tooltip("RectTransform used for the \"fill\" segment of this slider")]
        [SerializeField] private RectTransform fillRect = null;

        [Space(10f)]

        [Tooltip("If true, automatically round the slider value to the nearest integer")]
        [SerializeField] private bool wholeNumbers = false;

        [Tooltip("Slider minimum (left) value")]
        [SerializeField] private float minValue    = 0f;

        [Tooltip("Slider maximum (right) value")]
        [SerializeField] private float maxValue    = 1f;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Audio played when selecting a new option")]
        [SerializeField] private AudioAsset onSelectAudio = null;

        [Tooltip("Minimum cooldown before playing this slider audio again (in seconds)")]
        [SerializeField, Enhanced, Range(0f, 1f), DisplayName("Audio Cooldown")] private float audioCooldownDuration = .25f;

        [Tooltip("This option text")]
        [SerializeField, Enhanced, ShowIf(nameof(UseText))] private TextMeshProUGUI text = null;

        [Space(10f)]

        [Tooltip("Event called whenever this slider value changes")]
        [SerializeField] private UnityEvent<float> onValueChanged = new UnityEvent<float>();

        // -----------------------

        /// <summary>
        /// Slider current value.
        /// </summary>
        public float Value {
            get { return Option.SelectedValue; }
        }

        /// <summary>
        /// Slider current normalized value (between 0 and 1).
        /// </summary>
        public float NormalizedValue {
            get {
                if (Mathf.Approximately(minValue, maxValue))
                    return 0f;

                return Mathf.InverseLerp(minValue, maxValue, Value);
            }
            set {
                value = Mathf.Lerp(minValue, maxValue, value);
                SetValue(value);
            }
        }

        /// <summary>
        /// Total amount of steps for this slider.
        /// </summary>
        public float StepSize {
            get { return wholeNumbers ? 1f : ((maxValue - minValue) * .1f); }
        }

        /// <summary>
        /// Slider minimum and maximum range values.
        /// </summary>
        public Vector2 ValueRange {
            get { return new Vector2(minValue, maxValue); }
        }

        /// <summary>
        /// Indicates if this option uses a text.
        /// </summary>
        public bool UseText {
            get { return HasFlag(Parameters.Text); }
        }
        #endregion

        #region Selectable
        private Vector2 offset = Vector2.zero;

        // -----------------------

        public override void OnMove(AxisEventData _eventData) {

            if (!IsActive() || !IsInteractable()) {
                base.OnMove(_eventData);
                return;
            }

            switch (direction) {

                // Horizontal.
                case SelectionMovement.Horizontal:

                    switch (_eventData.moveDir) {

                        case MoveDirection.Left:
                            SetValue(Value - StepSize);
                            return;

                        case MoveDirection.Right:
                            SetValue(Value + StepSize);
                            return;

                        case MoveDirection.Up:
                        case MoveDirection.Down:
                        case MoveDirection.None:
                        default:
                            break;
                    }

                    break;

                // Vertical.
                case SelectionMovement.Vertical:

                    switch (_eventData.moveDir) {

                        case MoveDirection.Up:
                            SetValue(Value + StepSize);
                            return;

                        case MoveDirection.Down:
                            SetValue(Value - StepSize);
                            return;

                        case MoveDirection.Left:
                        case MoveDirection.Right:
                        case MoveDirection.None:
                        default:
                            break;
                    }

                    break;

                case SelectionMovement.None:
                default:
                    break;
            }

            base.OnMove(_eventData);
        }

        public override void OnPointerDown(PointerEventData _eventData) {

            if (!CanDrag(_eventData)) {
                return;
            }

            base.OnPointerDown(_eventData);

            offset = Vector2.zero;

            if ((handleContainerRect == null) || !RectTransformUtility.RectangleContainsScreenPoint(handleRect, _eventData.position, _eventData.enterEventCamera)) {

                // Outside the slider handle - jump to this point instead.
                UpdateDrag(_eventData, _eventData.pressEventCamera);
                
            } else if (RectTransformUtility.ScreenPointToLocalPointInRectangle(handleRect, _eventData.position, _eventData.pressEventCamera, out Vector2 _localCursor)) {
                offset = _localCursor;
            }
        }

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();

            // This can be called before OnEnabled - we don't want to access other objects then.
            if (!IsActive()) {
                return;
            }

            UpdateVisuals();
        }

        // -------------------------------------------
        // Drag
        // -------------------------------------------

        public void OnDrag(PointerEventData _eventData) {
            if (!CanDrag(_eventData)) {
                return;
            }

            UpdateDrag(_eventData, _eventData.pressEventCamera);
        }

        private bool CanDrag(PointerEventData _eventData) {
            return IsActive() && IsInteractable() && (_eventData.button == PointerEventData.InputButton.Left);
        }

        private void UpdateDrag(PointerEventData _eventData, Camera _camera) {
            RectTransform _rectTransform = handleContainerRect ?? fillContainerRect;
            float _size;

            switch (direction) {
                case SelectionMovement.Horizontal:
                    _size = _rectTransform.rect.size.x;
                    break;

                case SelectionMovement.Vertical:
                    _size = _rectTransform.rect.size.y;
                    break;

                case SelectionMovement.None:
                default:
                    _size = 0f;
                    break;
            }

            if ((_size <= 0f) || !RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, _eventData.position, _camera, out Vector2 _localCursor))
                return;

            _localCursor -= _rectTransform.rect.position + offset;

            float _value = (direction == SelectionMovement.Horizontal) ? _localCursor.x : _localCursor.y;
            _value = Mathf.Clamp01(_value / _size);

            NormalizedValue = _value;
        }

        public void OnInitializePotentialDrag(PointerEventData _eventData) {
            _eventData.useDragThreshold = false;
        }

        // -------------------------------------------
        // Selection
        // -------------------------------------------

        public override Selectable FindSelectableOnLeft() {
            if ((navigation.mode == Navigation.Mode.Automatic) && (direction == SelectionMovement.Horizontal))
                return null;

            return base.FindSelectableOnLeft();
        }

        public override Selectable FindSelectableOnRight() {
            if ((navigation.mode == Navigation.Mode.Automatic) && (direction == SelectionMovement.Horizontal))
                return null;

            return base.FindSelectableOnRight();
        }

        public override Selectable FindSelectableOnUp() {
            if ((navigation.mode == Navigation.Mode.Automatic) && (direction == SelectionMovement.Vertical))
                return null;

            return base.FindSelectableOnUp();
        }

        public override Selectable FindSelectableOnDown() {
            if ((navigation.mode == Navigation.Mode.Automatic) && (direction == SelectionMovement.Vertical))
                return null;

            return base.FindSelectableOnDown();
        }
        #endregion

        #region Option
        private readonly UnscaledCooldown audioCooldown = new UnscaledCooldown();

        // -----------------------

        /// <summary>
        /// Assigns a new value to this option.
        /// </summary>
        private void SetValue(float _value) {

            // Clamp option value.
            _value = Mathf.Clamp(_value, minValue, maxValue);

            if (wholeNumbers) {
                _value = Mathf.Round(_value);
            }

            if (Value == _value)
                return;

            Option.SelectedValue = _value;
            Apply();

            UpdateVisuals();

            // Audio.
            if (onSelectAudio.IsValid() && audioCooldown.IsValid) {

                onSelectAudio.PlayAudio();
                audioCooldown.Reload(audioCooldownDuration);
            }

            // Event.
            onValueChanged.Invoke(_value);
        }
        #endregion

        #region Display
        public override void RefreshOption() {
            RefreshText();
            UpdateVisuals();
        }

        // -------------------------------------------
        // Interface
        // -------------------------------------------

        private void RefreshText() {

            if (!UseText) {
                return;
            }

            text.text = Value.ToString("#.#");
        }
        #endregion

        #region Canvas Element
        public void Rebuild(CanvasUpdate _executing) {
            #if UNITY_EDITOR
            if (_executing == CanvasUpdate.Prelayout) {
                onValueChanged.Invoke(Value);
            }
            #endif
        }

        public void GraphicUpdateComplete() { }

        public void LayoutComplete() { }
        #endregion

        #region Slider
        private DrivenRectTransformTracker rectTransformTracker = default;

        private RectTransform handleContainerRect = null;
        private RectTransform fillContainerRect   = null;
        private Transform handleTransform         = null;
        private Transform fillTransform           = null;
        private Image fillImage                   = null;

        // -------------------------------------------
        // To clean - taken from someone else's code
        // -------------------------------------------

        private void UpdateCachedReferences() {

            if (fillRect && fillRect != (RectTransform)transform) {

                fillTransform = fillRect.transform;
                fillImage     = fillRect.GetComponent<Image>();

                if (fillTransform.parent != null) {
                    fillContainerRect = fillTransform.parent.GetComponent<RectTransform>();
                }

            } else {

                fillContainerRect = null;
                fillImage         = null;
                fillRect          = null;
            }

            if (handleRect && handleRect != (RectTransform)transform) {

                handleTransform = handleRect.transform;

                if (handleTransform.parent != null) {
                    handleContainerRect = handleTransform.parent.GetComponent<RectTransform>();
                }

            } else {
                handleContainerRect = null;
                handleRect          = null;
            }
        }

        private void UpdateVisuals() {

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                UpdateCachedReferences();
            }
            #endif

            rectTransformTracker.Clear();

            if (fillContainerRect != null) {
                rectTransformTracker.Add(this, fillRect, DrivenTransformProperties.Anchors);

                Vector2 _anchorMin = Vector2.zero;
                Vector2 _anchorMax = Vector2.one;

                if ((fillImage != null) && (fillImage.type == Image.Type.Filled)) {
                    fillImage.fillAmount = NormalizedValue;
                } else {
                    _anchorMax[((int)direction) - 1] = NormalizedValue;
                }

                fillRect.anchorMin = _anchorMin;
                fillRect.anchorMax = _anchorMax;
            }

            if (handleContainerRect != null) {
                rectTransformTracker.Add(this, handleRect, DrivenTransformProperties.Anchors);

                Vector2 _anchorMin = Vector2.zero;
                Vector2 _anchorMax = Vector2.one;

                _anchorMin[((int)direction) - 1] = _anchorMax[((int)direction) - 1] = NormalizedValue;
                handleRect.anchorMin = _anchorMin;
                handleRect.anchorMax = _anchorMax;
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get if a specific <see cref="Parameters"/> is enabled.
        /// </summary>
        /// <param name="_flag"><see cref="Parameters"/> to check.</param>
        /// <returns>True if this flag is enable, false otherwise.</returns>
        public bool HasFlag(Parameters _flag) {
            return parameters.HasFlagUnsafe(_flag);
        }
        #endregion
    }
}
