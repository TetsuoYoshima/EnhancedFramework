// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Playables;

[assembly: InternalsVisibleTo("EnhancedFramework.Editor")]
namespace EnhancedFramework.Timeline {
    /// <summary>
    /// Base interface to inherit any audio <see cref="PlayableAsset"/> from.
    /// </summary>
    public interface IAudioEnhancedPlayableAsset { }

    /// <summary>
    /// Base non-generic audio <see cref="PlayableAsset"/> class.
    /// </summary>
    public abstract class AudioEnhancedPlayableAsset : EnhancedPlayableAsset, IAudioEnhancedPlayableAsset {
        #region Global Members
        public const string NamePrefix = "Audio/";

        /// <summary>
        /// <see cref="AudioSource"/> bound object of this playable.
        /// </summary>
        [SerializeField, HideInInspector] internal AudioSource audioSource = null;
        #endregion
    }

    /// <summary>
    /// Base generic class for every audio <see cref="PlayableAsset"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="EnhancedPlayableBehaviour"/> playable for this asset.</typeparam>
    public abstract class AudioEnhancedPlayableAsset<T> : EnhancedPlayableAsset<T, AudioSource>, IAudioEnhancedPlayableAsset
                                                          where T : EnhancedPlayableBehaviour<AudioSource>, new() { }
}
