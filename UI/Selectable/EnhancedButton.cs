// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EnhancedFramework.UI {
    /// <summary>
    /// Enhanced <see cref="Button"/> behaviour, being automatically selected on mouse hover.
    /// </summary>
    [AddComponentMenu(FrameworkUtility.MenuPath + "UI/Button/Enhanced Button"), DisallowMultipleComponent]
    public class EnhancedButton : EnhancedSelectable, IPointerClickHandler, IEventSystemHandler, ISubmitHandler {
        #region Global Members
        [Space(5f)]

        [Tooltip("Called when the user clicks on the button")]
        [SerializeField] private UnityEvent m_OnClick = new UnityEvent();

        // -----------------------

        /// <summary>
        /// Called when the user clicks on the button.
        /// </summary>
        public UnityEvent OnClick {
            get { return m_OnClick; }
            set { m_OnClick = value; }
        }
        #endregion

        #region Button
        private const string PressMarkerName = "Button.onClick";

        // -----------------------

        public void Press() {

            if (IsActive() && IsInteractable()) {

                UISystemProfilerApi.AddMarker(PressMarkerName, this);
                m_OnClick.Invoke();
            }
        }

        public virtual void OnPointerClick(PointerEventData _eventData) {
            if (_eventData.button == PointerEventData.InputButton.Left) {
                Press();
            }
        }

        public virtual void OnSubmit(BaseEventData _eventData) {
            Press();

            if (IsActive() && IsInteractable()) {
                DoStateTransition(SelectionState.Pressed, false);
                StartCoroutine(OnFinishSubmit());
            }
        }

        private IEnumerator OnFinishSubmit() {
            float _fadeTime    = colors.fadeDuration;
            float _elapsedTime = 0f;

            while (_elapsedTime < _fadeTime) {
                _elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }
        #endregion
    }
}
