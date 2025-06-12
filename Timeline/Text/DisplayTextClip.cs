// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if TEXT_MESH_PRO_PACKAGE
using EnhancedEditor;
using System;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

using DisplayName = System.ComponentModel.DisplayNameAttribute;
using Range       = EnhancedEditor.RangeAttribute;
using ReadOnly    = EnhancedEditor.ReadOnlyAttribute;

namespace EnhancedFramework.Timeline {
    /// <summary>
    /// Displays a given <see cref="TextMeshProUGUI"/> content for the duration of the clip.
    /// </summary>
    [DisplayName(NamePrefix + "Display Text")]
    public sealed class DisplayTextClip : TextMeshProPlayableAsset<DisplayTextBehaviour> {
        #region Global Members
        #if UNITY_EDITOR
        [Space(10f)]

        [SerializeField, Enhanced, ShowIf(nameof(UseCharacterDuration)), ReadOnly] internal float perfectCharacterDuration = 0f;
        #endif

        // -----------------------

        /// <inheritdoc cref="DisplayTextBehaviour.UseCharacterDuration"/>
        public bool UseCharacterDuration {
            get { return Template.UseCharacterDuration; }
        }
        #endregion

        #region Behaviour
        public override Playable CreatePlayable(PlayableGraph _graph, GameObject _owner) {
            #if UNITY_EDITOR
            Template.source = this;
            #endif

            return base.CreatePlayable(_graph, _owner);
        }
        #endregion

        #region Utility
        public override string ClipDefaultName {
            get { return "Display Text"; }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="DisplayTextClip"/>-related <see cref="PlayableBehaviour"/>.
    /// </summary>
    [Serializable]
    public sealed class DisplayTextBehaviour : EnhancedPlayableBehaviour<TextMeshProUGUI> {
        #region Global Members
        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("If true, uses a specific per character duration for display instead of using the duration of this clip")]
        public bool UseCharacterDuration = true;

        [Enhanced, ShowIf(nameof(UseCharacterDuration)), Range(0f, 1f)] public float CharacterDuration = .05f;

        // -----------------------

        #if UNITY_EDITOR
        [NonSerialized] internal DisplayTextClip source = null;
        #endif
        #endregion

        #region Behaviour
        public override void ProcessFrame(Playable _playable, FrameData _info, object _playerData) {
            base.ProcessFrame(_playable, _info, _playerData);

            TextMeshProUGUI _text = bindingObject;
            if (_text.IsNull()) {
                return;
            }

            #if UNITY_EDITOR
            // Get best per character duration using clip duration.
            if (!Application.isPlaying) {

                int _characterCount = _text.textInfo.characterCount;
                source.perfectCharacterDuration = (_characterCount != 0)
                                                ? ((float)_playable.GetDuration() / _characterCount)
                                                : 0f;
            }
            #endif

            // Display.
            float _characterDuration;

            if (UseCharacterDuration) {
                _characterDuration = CharacterDuration;
            } else {
                int _characterCount = _text.textInfo.characterCount;
                _characterDuration = (float)_playable.GetDuration() / _characterCount;
            }

            int _count = (int)(_playable.GetTime() / _characterDuration);
            _text.maxVisibleCharacters = _count;
        }

        protected override void OnStop(Playable _playable, FrameData _info, bool _completed) {
            base.OnStop(_playable, _info, _completed);

            TextMeshProUGUI _text = bindingObject;
            if (_text.IsNull()) {
                return;
            }

            // Set visible.
            _text.maxVisibleCharacters = int.MaxValue;
        }
        #endregion
    }
}
#endif
