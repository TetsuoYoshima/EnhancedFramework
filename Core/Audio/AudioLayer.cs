// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.Runtime.CompilerServices;
using UnityEngine;

using AudioSettings = EnhancedFramework.Core.Settings.AudioSettings;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Layers used to play audio sound effects, acting as a priority.
    /// <para/>
    /// For instance, you might want to play a battle music, but one might already be playing (new foes have entered the party).
    /// <br/> Once the battle over, you want to resume the exploration music just where it was.
    /// <para/>
    /// Using audio layers, you could play all battle musics on the same layer, so that when a new one is played, it simply oveerrides the other.
    /// <br/> And once over, resume the exploration music which was setup on a lower layer level.
    /// </summary>
    public enum AudioLayer {
        /// <summary>
        /// Default minimum layer value.
        /// </summary>
        [Ethereal]
        Default         = 0,

        [Separator(SeparatorPosition.Top)]

        [DisplayName("Layer 1")]
        Layer_One       = 1,

        [DisplayName("Layer 2")]
        Layer_Two       = 2,

        [DisplayName("Layer 3")]
        Layer_Three     = 3,

        [DisplayName("Layer 4")]
        Layer_Four      = 4,

        [DisplayName("Layer 5")]
        Layer_Five      = 5,

        [DisplayName("Layer 6")]
        Layer_Six       = 6,

        [DisplayName("Layer 7")]
        Layer_Seven     = 7,

        [DisplayName("Layer 8")]
        Layer_Eight     = 8,

        [DisplayName("Layer 9")]
        Layer_Nine      = 9,

        [DisplayName("Layer 10")]
        Layer_Ten       = 10,

        [DisplayName("Layer 11")]
        Layer_Eleven    = 11,

        [DisplayName("Layer 12")]
        Layer_Twelve    = 12,

        [DisplayName("Layer 13")]
        Layer_Thirteen  = 13,

        [DisplayName("Layer 14")]
        Layer_Fourteen  = 14,

        [DisplayName("Layer 15")]
        Layer_Fifteen   = 15,

        [DisplayName("Layer 16")]
        Layer_Sixteen   = 16,
    }

    /// <summary>
    /// Contains multiple <see cref="AudioLayer"/> utility methods.
    /// </summary>
    public static class AudioLayerUtility {
        #region Content
        /// <summary>
        /// <inheritdoc cref="AudioSettings.NameToAudioLayer(string, out AudioLayer)" path="/summary"/>
        /// <br/> Only works during play mode.
        /// </summary>
        /// <inheritdoc cref="AudioSettings.NameToAudioLayer(string, out AudioLayer)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NameToLayer(string _name, out AudioLayer _layer) {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                _layer = AudioLayer.Default;
                return false;
            }
            #endif

            return AudioSettings.I.NameToAudioLayer(_name, out _layer);
        }

        /// <summary>
        /// <inheritdoc cref="AudioSettings.AudioLayerToName(AudioLayer)" path="/summary"/>
        /// <br/> Only works during play mode.
        /// </summary>
        /// <inheritdoc cref="AudioSettings.AudioLayerToName(AudioLayer)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string LayerToName(AudioLayer _layer) {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return _layer.ToString();
            }
            #endif

            return AudioSettings.I.AudioLayerToName(_layer);
        }
        #endregion
    }
}
