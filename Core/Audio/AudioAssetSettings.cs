// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

using MessageType = EnhancedEditor.MessageType;
#endif

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="ScriptableObject"/> encapsulated audio settings applied to <see cref="AudioAsset"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "AST_AudioSettings", menuName = FrameworkUtility.MenuPath + "Audio/Audio Asset Settings", order = FrameworkUtility.MenuOrder)]
    public sealed class AudioAssetSettings : EnhancedScriptableObject {
        #region Global Members
        [Section("Audio Settings")]

        [Tooltip("Multiplier applied to the audio volume")]
        [SerializeField, Enhanced, Range(0f, 10f)] private float volumeMultiplier = 1f;

        [Space(10f, order = 0), Title("3D Sound", order = 1), Space(3f, order = 3)]

        [Tooltip("Doppler scale used on the audio")]
        [SerializeField, Enhanced, Range(0f, 5f)] private float dopplerLevel = 1f;

        [Tooltip("Spread angle (in degrees) of a 3D channel stereo or multichannel sound in speaker space")]
        [SerializeField, Enhanced, Range(0f, 360f)] private float spreadAngle = 0f;

        [Space(10f)]

        [Tooltip("Determines how the audio attenuates over distance")]
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        [Tooltip("Minimum and maximum distance used to attenuate the audio volume (loudest at min distance, completly attenuated at max)")]
        [SerializeField, Enhanced, MinMax(0f, 500f)] private Vector2 attenuationDistance = new Vector2(1f, 30f);

        [Space(10f, order = 0), Title("3D Curves", order = 1), Space(5f, order = 3)]

        [Enhanced, HelpBox("Value as Y value, min and max distance as X value", MessageType.Info)]
        [SerializeField] private MultiCurve curves = new MultiCurve(new MultiCurve.Curve[] {
            new MultiCurve.Curve(AnimationCurve.EaseInOut(0f, 1f, 1f, 0f),  "Custom Rolloff",   SuperColor.DarkOrange.Get()),
            new MultiCurve.Curve(AnimationCurve.Constant(0f, 1f, 1f),       "Reverb Zone Mix",  SuperColor.Yellow.Get()),
            new MultiCurve.Curve(AnimationCurve.Constant(0f, 1f, 0f),       "Spatial Blend",    SuperColor.Green.Get()),
            new MultiCurve.Curve(AnimationCurve.Constant(0f, 1f, 0f),       "Spread",           SuperColor.Blue.Get()),
        }, new Vector2(0f, 1f), false, true, 1f, 1f);

        // -----------------------

        /// <summary>
        /// Custom rolloff <see cref="AnimationCurve"/>.
        /// </summary>
        public AnimationCurve CustomRollofCurve {
            get { return curves[0]; }
            set { curves[0] = value; }
        }

        /// <summary>
        /// Custom reverb zone mix <see cref="AnimationCurve"/>.
        /// </summary>
        public AnimationCurve ReverbZoneMixCurve {
            get { return curves[1]; }
            set { curves[1] = value; }
        }

        /// <summary>
        /// Custom spatial blend <see cref="AnimationCurve"/>.
        /// </summary>
        public AnimationCurve SpatialBlendCurve {
            get { return curves[2]; }
            set { curves[2] = value; }
        }

        /// <summary>
        /// Custom spread <see cref="AnimationCurve"/>.
        /// </summary>
        public AnimationCurve SpreadCurve {
            get { return curves[3]; }
            set { curves[3] = value; }
        }

        /// <summary>
        /// Multiplier applied to the audio volume.
        /// </summary>
        public float VolumeMultiplier {
            get { return volumeMultiplier; }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Applies these settings value to a given <see cref="AudioSource"/>.
        /// </summary>
        /// <param name="_audio"><see cref="AudioSource"/> to apply these settings values on.</param>
        [Button(SuperColor.Green, IsDrawnOnTop = false)]
        public void ApplyValuesOn(AudioSource _audio) {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                Undo.RecordObject(_audio, "Apply audio settings values");
            }
            #endif

            _audio.SetCustomCurve(AudioSourceCurveType.CustomRolloff, CustomRollofCurve);
            _audio.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, ReverbZoneMixCurve);
            _audio.SetCustomCurve(AudioSourceCurveType.SpatialBlend,  SpatialBlendCurve);
            _audio.SetCustomCurve(AudioSourceCurveType.Spread,        SpreadCurve);

            _audio.dopplerLevel = dopplerLevel;
            _audio.rolloffMode  = rolloffMode;
            _audio.minDistance  = attenuationDistance.x;
            _audio.maxDistance  = attenuationDistance.y;
            _audio.spread       = spreadAngle;
        }

        /// <summary>
        /// Copies the values from a specific <see cref="AudioSource"/> in these settings.
        /// </summary>
        /// <param name="_audio"><see cref="AudioSource"/> to copy the values from in these settings.</param>
        [Button(SuperColor.Crimson, IsDrawnOnTop = false)]
        public void CopyValuesFrom(AudioSource _audio) {
            #if UNITY_EDITOR
            Undo.RecordObject(this, "Copy audio settings values");
            #endif

            CustomRollofCurve   = _audio.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
            ReverbZoneMixCurve  = _audio.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix);
            SpatialBlendCurve   = _audio.GetCustomCurve(AudioSourceCurveType.SpatialBlend);
            SpreadCurve         = _audio.GetCustomCurve(AudioSourceCurveType.Spread);

            dopplerLevel = _audio.dopplerLevel;
            rolloffMode  = _audio.rolloffMode;
            spreadAngle  = _audio.spread;
            attenuationDistance.x = _audio.minDistance;
            attenuationDistance.y = _audio.maxDistance;
        }
        #endregion
    }
}
