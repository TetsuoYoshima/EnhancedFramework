// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if DOTWEEN_ENABLED
#define TWEENING
#endif

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;
using UnityEngine.Rendering;

#if TWEENING
using DG.Tweening;
#endif

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Rendering {
    /// <summary>
    /// Class wrapper for a fading <see cref="UnityEngine.Rendering.Volume"/> instance.
    /// <para/> Fading is performed using tweening, if enabled.
    /// </summary>
    [Serializable]
    public sealed class FadingVolume : FadingObject {
        #region Global Members
        /// <summary>
        /// This object <see cref="UnityEngine.Rendering.Volume"/>.
        /// </summary>
        [Enhanced, Required] public Volume Volume = null;

        /// <summary>
        /// The fading target weight of this volume: first value when fading out, second when fading in.
        /// </summary>
        [Tooltip("The fading target weight of this volume: first value when fading out, second when fading in")]
        [Enhanced, MinMax(0f, 1f)] public Vector2 FadeWeight = new Vector2(0f, 1f);

        [Tooltip("If true, enables / disables the associated volume component when fading this object")]
        public bool EnableVolume = true;

        [Space(10f)]

        [SerializeField, Enhanced, Range(0f, 10f)] private float fadeInDuration = .5f;
        #if TWEENING
        [SerializeField] private Ease fadeInEase = Ease.OutSine;

        [Space(5f)]
        #endif

        [SerializeField, Enhanced, Range(0f, 10f)] private float fadeOutDuration = .5f;
        #if TWEENING
        [SerializeField] private Ease fadeOutEase = Ease.InSine;
        #endif

        // -----------------------

        public override float ShowDuration {
            get { return fadeInDuration; }
        }

        public override float HideDuration {
            get { return fadeOutDuration; }
        }

        public override bool IsVisible {
            get { return Volume.weight == FadeWeight.y; }
        }

        public override bool RealTime {
            get { return true; }
        }
        #endregion

        #region Behaviour
        private Action<float> onSetFadeValue  = null;
        private Action<bool>  onFadeStopped   = null;
        private Action<bool>  onFadeCompleteCallback = null;

        private TweenHandler tween = default;

        // -------------------------------------------
        // Core
        // -------------------------------------------

        protected override void OnShow(bool _isInstant, Action<bool> _onComplete = null) {
            OnFade(_isInstant, FadeWeight.y, fadeInDuration,
                   #if TWEENING
                   fadeInEase,
                   #endif
                   _onComplete);
        }

        protected override void OnHide(bool _isInstant, Action<bool> _onComplete = null) {
            OnFade(_isInstant, FadeWeight.x, fadeOutDuration,
                   #if TWEENING
                   fadeOutEase,
                   #endif
                   _onComplete);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        public override void Evaluate(float _time, bool _show) {

            float _duration = _show ? fadeInDuration : fadeOutDuration;
            float _value    = (_duration != 0f) ? Mathf.Clamp01(_time / _duration) : 0f;

            SetFadeValue(_value, _show);
        }

        public override void SetFadeValue(float _value, bool _show) {
            float _weight;

            #if TWEENING
            if (_show) {
                _weight = DOVirtual.EasedValue(FadeWeight.x, FadeWeight.y, _value, fadeInEase);
            } else {
                _weight = DOVirtual.EasedValue(FadeWeight.y, FadeWeight.x, _value, fadeOutEase);
            }
            #else
            if (_show) {
                _weight = Mathf.Lerp(FadeWeight.x, FadeWeight.y, _value);
            } else {
                _weight = Mathf.Lerp(FadeWeight.y, FadeWeight.x, _value);
            }
            #endif

            CancelCurrentFade();
            SetWeight(_weight);
        }

        public override void CancelCurrentFade() {
            base.CancelCurrentFade();

            #if EDITOR_COROUTINE
            if (!Application.isPlaying && (coroutine != null)) {
                EditorCoroutineUtility.StopCoroutine(coroutine);
            }
            #endif

            // Stop, without complete.
            tween.Stop();
        }

        // -------------------------------------------
        // Fade
        // -------------------------------------------

        private void OnFade(bool _isInstant, float _value, float _duration,
                            #if TWEENING
                            Ease _ease,
                            #endif
                            Action<bool> _onComplete) {

            CancelCurrentFade();
            onFadeCompleteCallback = _onComplete;

            // Instant.
            if (_isInstant || (_duration == 0f)) {
                SetWeight(_value);
                OnStopped(true);
                return;
            }

            // Already faded.
            if (Volume.weight == _value) {
                OnStopped(true);
                return;
            }

            // Delegates.
            if (onSetFadeValue == null) {
                onSetFadeValue = Set;
                onFadeStopped  = OnStopped;
            }

            // Tween.
            tween = Core.Tweener.Tween(Volume.weight, _value, onSetFadeValue, _duration,
                                       #if TWEENING
                                       _ease,
                                       #endif
                                       true, onFadeStopped);

            // ----- Local Methods ----- \\

            void Set(float _value) {
                SetWeight(_value);
            }

            void OnStopped(bool _completed) {
                onFadeCompleteCallback?.Invoke(_completed);
            }
        }

        private void SetWeight(float _weight) {

            if (Volume.weight == _weight) {
                return;
            }

            Volume.weight = _weight;

            if (EnableVolume) {
                Volume.enabled = !Mathf.Approximately(_weight, FadeWeight.x);
            }
        }
        #endregion
    }

    /// <summary>
    /// Ready-to-use <see cref="EnhancedBehaviour"/>-encapsulated <see cref="FadingVolume"/>.
    /// <br/> Use this to quickly implement instantly fading <see cref="UnityEngine.Rendering.Volume"/> objects.
    /// </summary>
    [ScriptGizmos(false, true)]
    [RequireComponent(typeof(Volume))]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Rendering/Fading Volume"), DisallowMultipleComponent]
    public sealed class FadingVolumeBehaviour : FadingObjectBehaviour {
        #region Global Members
        [Section("Fading Volume")]

        [SerializeField, Enhanced, Block] private FadingVolume volume = default;

        [Space(10f)]

        [SerializeField] private FadingMode initMode = FadingMode.Hide;

        // -----------------------

        public FadingVolume Volume {
            get { return volume; }
        }

        public override IFadingObject FadingObject {
            get { return volume; }
        }

        public override FadingMode InitMode {
            get { return initMode; }
        }
        #endregion

        #region Enhanced Behaviour
        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            // References.
            if (!volume.Volume) {
                volume.Volume = GetComponent<Volume>();
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
            _data.Strings.Add(JsonUtility.ToJson(volume));
        }

        public override void LoadPlayModeData(PlayModeEnhancedObjectData _data) {

            // Load from json.
            FadingVolume _volume = JsonUtility.FromJson<FadingVolume>(_data.Strings[0]);
            _volume.Volume = volume.Volume;

            volume = _volume;
        }
        #endregion
    }
}
