// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

namespace EnhancedFramework.Timeline {
    /// <summary>
    /// Base interface to inherit any fading object <see cref="PlayableAsset"/> from.
    /// </summary>
    public interface IFadingObjectPlayableAsset { }

    /// <summary>
    /// Base non-generic <see cref="FadingObjectBehaviour"/> <see cref="PlayableAsset"/> class.
    /// </summary>
    public abstract class FadingObjectPlayableAsset : EnhancedPlayableAsset, IFadingObjectPlayableAsset { }

    /// <summary>
    /// Base generic class for every <see cref="FadingObjectBehaviour"/> <see cref="PlayableAsset"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="EnhancedPlayableBehaviour"/> playable for this asset.</typeparam>
    public abstract class FadingObjectPlayableAsset<T> : EnhancedPlayableAsset<T, FadingObjectBehaviour>, IFadingObjectPlayableAsset
                                                         where T : EnhancedPlayableBehaviour<FadingObjectBehaviour>, new() { }

    // -------------------------------------------
    // Fade
    // -------------------------------------------

    /// <summary>
    /// Base generic class for every <see cref="FadingObjectBehaviour"/> <see cref="PlayableAsset"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="EnhancedPlayableBehaviour"/> playable for this asset.</typeparam>
    public abstract class FadingObjectFadePlayableAsset<T> : EnhancedPlayableAsset<T, FadingObjectBehaviour>, IFadingObjectPlayableAsset, IPropertyPreview
                                                             where T : FadingObjectFadePlayableBehaviour, new() {
        #region Global Members
        public const string NamePrefix = "Fading Object/";
        #endregion

        #region Behaviour
        public void GatherProperties(PlayableDirector _director, IPropertyCollector _driver) {

            // Register object properties.
            IFadingObject _bindingObject = Template.FadingObject;

            if ((_bindingObject != null) && !EqualityUtility.Equals(_bindingObject, null) && (_bindingObject is Component _component) && (_component != null)) {
                _driver.AddFromComponent(_component.gameObject, _component);

                CanvasGroup _group = _component.GetComponentInChildren<CanvasGroup>();
                if (_group.IsValid()) {
                    _driver.AddFromComponent(_group.gameObject, _group);
                }

                Canvas _canvas = _component.GetComponentInParent<Canvas>();
                if (_canvas.IsValid()) {
                    _driver.AddFromComponent(_canvas.gameObject, _canvas);
                }

                #if RENDER_PIPELINE
                if (_component.TryGetComponent(out Volume _volume)) {
                    _driver.AddFromComponent(_volume.gameObject, _volume);
                }
                #endif
            }
        }
        #endregion
    }

    /// <summary>
    /// Base <see cref="PlayableBehaviour"/> class for a <see cref="FadingObjectBehaviour"/>.
    /// </summary>
    [Serializable]
    public class FadingObjectFadePlayableBehaviour : EnhancedPlayableBehaviour<FadingObjectBehaviour> {
        #region Global Members
        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Visibility of this object when entering this clip")]
        public bool EnterVisibility = true;

        [Tooltip("Visibility of this object when exiting this clip")]
        public bool ExitVisibility  = false;

        [Space(5f)]

        public bool EnterInstant = false;
        public bool ExitInstant  = false;

        // -----------------------

        /// <summary>
        /// <see cref="IFadingObject"/> of this behaviour.
        /// </summary>
        public virtual IFadingObject FadingObject {
            get { return bindingObject; }
        }
        #endregion

        #region Behaviour
        // Cache.
        private bool hasFromVisibility = false;
        private bool fromVisibility    = false;

        // -----------------------

        protected override void OnPlay(Playable _playable, FrameData _info) {
            base.OnPlay(_playable, _info);

            // Cache setup.
            IFadingObject _object = FadingObject;

            if (!hasFromVisibility && (_object != null)) {

                fromVisibility = _object.IsVisible;
                hasFromVisibility = true;
            }
        }

        public override void ProcessFrame(Playable _playable, FrameData _info, object _playerData) {
            base.ProcessFrame(_playable, _info, _playerData);

            IFadingObject _object = FadingObject;
            if (_object == null) {
                return;
            }

            float _time = (float)_playable.GetTime();

            // Fade in.
            float _startDuration = EnterInstant ? 0f : GetDuration(EnterVisibility);

            if ((_time < _startDuration) && (fromVisibility != EnterVisibility)) {

                _object.Evaluate(_time, EnterVisibility);
                return;
            }

            // Fade out.
            float _endDuration = ExitInstant ? 0f : GetDuration(ExitVisibility);
            float _endStart = (float)_playable.GetDuration() - _endDuration;

            if ((_time > _endStart) && (EnterVisibility != ExitVisibility)) {

                _object.Evaluate(_time - _endStart, ExitVisibility);
                return;
            }

            // Enter visibility.
            _object.SetVisibility(EnterVisibility, true);

            // ----- Local Method ----- \\

            float GetDuration(bool _visible) {
                return _visible ? _object.ShowDuration : _object.HideDuration;
            }
        }

        protected override void OnStop(Playable _playable, FrameData _info, bool _completed) {
            base.OnStop(_playable, _info, _completed);

            IFadingObject _object = FadingObject;

            // Exit visibility.
            if (_object != null) {

                bool _visibility = _completed ? ExitVisibility : fromVisibility;
                _object.SetVisibility(_visibility, true);
            }
        }
        #endregion
    }
}
