// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;

namespace EnhancedFramework.UI {
    // ===== Base ===== \\

    /// <summary>
    /// Base class for <see cref="EnhancedBehaviour"/>-encapsulated <see cref="FadingGroup"/>.
    /// <para/>
    /// Inherited by <see cref="FadingGroupBehaviour"/> and <see cref="TweeningFadingGroupBehaviour"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="FadingGroup"/> class type used by this object.</typeparam>
    public abstract class BaseFadingGroupBehaviour<T> : FadingObjectBehaviour where T : FadingGroup, new() {
        #region Global Members
        [Section("Fading Group")]

        [SerializeField, Enhanced, Block] protected T group = default;

        [Space(10f)]

        [Tooltip("Fading mode applied on this behaviour init")]
        [SerializeField] protected FadingMode initMode = FadingMode.Hide;

        // -----------------------

        public T Group {
            get { return group; }
        }

        public override IFadingObject FadingObject {
            get { return group; }
        }

        public override FadingMode InitMode {
            get { return initMode; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Group callback.
            group.OnDisabled();
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            // References.
            if (group == null) {
                group = Activator.CreateInstance<T>();
            }

            if (!group.Group) {
                group.Group = GetComponent<CanvasGroup>();
            }

            if (group.UseCanvas && !group.Canvas) {
                group.Canvas = GetComponent<Canvas>();
            }

            if (group.UseController && !group.Controller) {
                group.Controller = GetComponent<FadingGroupController>();
            }

            if (group.UseSelectable && !Application.isPlaying) {
                group.ActiveSelectable = group.Selectable;
            }
        }
        #endif
        #endregion

        #region Play Mode Data
        public override bool CanSavePlayModeData {
            get { return true; }
        }

        // -----------------------

        public override void SavePlayModeData(PlayModeEnhancedObjectData _data) {

            // Save as json.
            _data.Strings.Add(JsonUtility.ToJson(group));
        }

        public override void LoadPlayModeData(PlayModeEnhancedObjectData _data) {

            // Load from json.
            T _group = JsonUtility.FromJson<T>(_data.Strings[0]);

            _group.Controller = group.Controller;
            _group.Canvas = group.Canvas;
            _group.Group = group.Group;

            _group.Selectable = group.Selectable;
            _group.ActiveSelectable = group.ActiveSelectable;

            group = _group;
        }
        #endregion
    }

    // ===== Derived ===== \\

    /// <summary>
    /// Base class for <see cref="EnhancedBehaviour"/>-encapsulated <see cref="FadingObjectTransitionFadingGroup"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="FadingObjectTransitionFadingGroup"/> class type used by this object.</typeparam>
    public abstract class FadingObjectTransitionFadingGroupBehaviour<T> : BaseFadingGroupBehaviour<T> where T : FadingObjectTransitionFadingGroup, new() {
        #region Behaviour
        private Action<bool> onTransitionCompleteCallback = null;
        private Action onTransitionFadeOutCallback        = null;

        private Action<bool> onTransitionFaded = null;

        // -----------------------

        /// <inheritdoc cref="FadingObjectTransitionFadingGroup.StartFadeIn"/>
        public virtual void StartFadeIn(Action _onFaded = null, Action<bool> _onComplete = null) {
            group.StartFadeIn(_onFaded, _onComplete);
        }

        /// <inheritdoc cref="FadingObjectTransitionFadingGroup.StartFadeOut"/>
        public virtual void StartFadeOut(Action _onFaded = null, Action<bool> _onComplete = null) {
            group.StartFadeOut(_onFaded, _onComplete);
        }

        /// <inheritdoc cref="FadingObjectTransitionFadingGroup.CompleteFade"/>
        public virtual void CompleteFade(Action<bool> _onComplete = null) {
            group.CompleteFade(_onComplete);
        }

        // -------------------------------------------
        // Transition
        // -------------------------------------------

        /// <summary>
        /// Shows this transition group.
        /// </summary>
        /// <param name="_onFadeIn">Called once the transition group has faded in.</param>
        /// <param name="_onFadeOut">Called before the transition group fade out.</param>
        /// <param name="_onComplete">Called once fading has been completed.</param>
        public virtual void ShowTransition(Action _onFadeIn = null, Action _onFadeOut = null, Action<bool> _onComplete = null) {
            CancelCurrentFade();

            onTransitionCompleteCallback = _onComplete;
            onTransitionFadeOutCallback = _onFadeOut;

            onTransitionFaded ??= OnTransitionFaded;
            StartFadeIn(_onFadeIn, onTransitionFaded);
        }

        /// <summary>
        /// Hides this transition group.
        /// </summary>
        /// <inheritdoc cref="ShowTransition"/>
        public virtual void HideTransition(Action _onFadeIn = null, Action _onFadeOut = null, Action<bool> _onComplete = null) {
            CancelCurrentFade();

            onTransitionCompleteCallback = _onComplete;
            onTransitionFadeOutCallback = _onFadeOut;

            onTransitionFaded ??= OnTransitionFaded;
            StartFadeOut(_onFadeIn, onTransitionFaded);
        }

        // -----------------------

        private void OnTransitionFaded(bool _completed) {
            onTransitionFadeOutCallback?.Invoke();
            CompleteFade(onTransitionCompleteCallback);
        }
        #endregion
    }
}
