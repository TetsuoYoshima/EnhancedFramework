// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if UNITY_2021_3_OR_NEWER
#define PARENT_TRACK_METHOD
#endif

#if TIMELINE_ENABLED
using EnhancedEditor;
using EnhancedFramework.Timeline;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace EnhancedFramework.Editor {
    /// <summary>
    /// Custom <see cref="EnhancedPlayableAsset"/> editor.
    /// </summary>
    [CustomTimelineEditor(typeof(EnhancedPlayableAsset))]
    #pragma warning disable
    public class EnhancedPlayableAssetEditor : ClipEditor {
        #region Utility
        public override void OnCreate(TimelineClip _clip, TrackAsset _track, TimelineClip _clonedFrom) {
            base.OnCreate(_clip, _track, _clonedFrom);

            // TrackAsset callback OnCreateClip in called before assigning the clip default values and references.
            // So use the editor callback instead.

            if (_clip.asset is EnhancedPlayableAsset _asset) {
                _asset.OnCreated(_clip);
            }
        }

        public override void GetSubTimelines(TimelineClip _clip, PlayableDirector _director, List<PlayableDirector> _subTimelines) {
            base.GetSubTimelines(_clip, _director, _subTimelines);

            // Use this callback to assign the Audio Source bound object reference to timeline clips.
            TrackAsset _track;

            #if PARENT_TRACK_METHOD
            _track = _clip.GetParentTrack();
            #else
            _track = _clip.parentTrack;
            #endif

            Object _binding = _director.GetGenericBinding(_track);

            if ((_clip.asset is EnhancedPlayableAsset _asset) && _asset.SerializeBindingInComponent) {
                _director.gameObject.AddComponentIfNone<EnhancedPlayableBindingData>().RegisterData(_asset, _binding);
            }
        }
        #endregion
    }
}
#endif
