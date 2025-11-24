// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if TEXT_MESH_PRO_PACKAGE
using TMPro;
using UnityEngine.Playables;

namespace EnhancedFramework.Timeline {
    /// <summary>
    /// Base interface to inherit any <see cref="TextMeshProTrack"/> <see cref="PlayableAsset"/> from.
    /// </summary>
    public interface ITextMeshProPlayableAsset { }

    /// <summary>
    /// Base non-generic <see cref="TextMeshProUGUI"/> <see cref="PlayableAsset"/> class.
    /// </summary>
    public abstract class TextMeshProPlayableAsset : EnhancedPlayableAsset, ITextMeshProPlayableAsset { }

    /// <summary>
    /// Base generic class for every <see cref="TextMeshProUGUI"/> <see cref="PlayableAsset"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="EnhancedPlayableBehaviour"/> playable for this asset.</typeparam>
    public abstract class TextMeshProPlayableAsset<T> : EnhancedPlayableAsset<T, TextMeshProUGUI>, ITextMeshProPlayableAsset
                                                        where T : EnhancedPlayableBehaviour<TextMeshProUGUI>, new() {
        #region Global Members
        public const string NamePrefix = "Text/";
        #endregion
    }
}
#endif
