// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Video;

using Min   = EnhancedEditor.MinAttribute;
using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    // ===== Base ===== \\

    /// <summary>
    /// Base class for a <see cref="SplashManager"/> animation.
    /// </summary>
    [Serializable]
    public abstract class SplashAnimation {
        #region Behaviour
        /// <summary>
        /// Plays this animation.
        /// </summary>
        public abstract IEnumerator Play();
        #endregion
    }

    // ===== Derived ===== \\

    /// <summary>
    /// Waits for a given amount of time, in second(s).
    /// </summary>
    [Serializable, DisplayName("Wait")]
    public sealed class WaitSplashAnimation : SplashAnimation {
        #region Global Members
        [Enhanced, Min(0f)] public float Duration = 1f;
        #endregion

        #region Behaviour
        public override IEnumerator Play() {
            yield return new WaitForSecondsRealtime(Duration);
        }
        #endregion
    }

    /// <summary>
    /// Instantly triggers a <see cref="UnityEvent"/>.
    /// </summary>
    [Serializable, DisplayName("Unity Event")]
    public sealed class UnityEventSplashAnimation : SplashAnimation {
        #region Global Members
        public UnityEvent Event = new UnityEvent();
        #endregion

        #region Behaviour
        public override IEnumerator Play() {
            Event.Invoke();
            yield break;
        }
        #endregion
    }

    /// <summary>
    /// Plays a specific <see cref="EnhancedVideoPlayer"/>.
    /// </summary>
    [Serializable, DisplayName("Video")]
    public sealed class VideoSplashAnimation : SplashAnimation {
        #region Global Members
        [Enhanced, Required] public EnhancedVideoPlayer VideoPlayer = null;
        #endregion

        #region Behaviour
        public override IEnumerator Play() {
            bool _isWaiting = true;

            VideoPlayer.Stopped += OnComplete;
            VideoPlayer.Play();

            while (_isWaiting) {
                yield return null;
            }

            // ----- Local Method ----- \\

            void OnComplete(VideoPlayer _player) {
                VideoPlayer.Stopped -= OnComplete;
                _isWaiting = false;
            }
        }
        #endregion
    }

    /// <summary>
    /// Plays a specific <see cref="PlayableDirector"/>.
    /// </summary>
    [Serializable, DisplayName("Playable")]
    public sealed class PlayableSplashAnimation : SplashAnimation {
        #region Global Members
        [Enhanced, Required] public EnhancedPlayablePlayer Playable = null;
        #endregion

        #region Behaviour
        public override IEnumerator Play() {
            bool _isWaiting = true;

            Playable.GetPlayableDirector().stopped += OnComplete;
            Playable.Play();

            while (_isWaiting) {
                yield return null;
            }

            // ----- Local Method ----- \\

            void OnComplete(PlayableDirector _playable) {
                Playable.GetPlayableDirector().stopped -= OnComplete;
                _isWaiting = false;
            }
        }
        #endregion
    }

    /// <summary>
    /// Set the visibility of a specific <see cref="IFadingObject"/>.
    /// </summary>
    [Serializable, DisplayName("Fading Object")]
    public sealed class FadingObjectSplashAnimation : SplashAnimation {
        #region Global Members
        public SerializedInterface<IFadingObject> FadingObject = new SerializedInterface<IFadingObject>();
        public FadingMode Mode = FadingMode.Show;
        public bool WaitForCompletion = true;
        [Enhanced, ShowIf(nameof(ShowWait)), Range(0f, 5f)] public float WaitDuration = .5f;

        public bool ShowWait {
            get { return Mode == FadingMode.FadeInOut; }
        }
        #endregion

        #region Behaviour
        public override IEnumerator Play() {
            bool _isWaiting = WaitForCompletion;

            FadingObject.Interface.Fade(Mode, OnComplete, WaitDuration);

            while (_isWaiting) {
                yield return null;
            }

            // ----- Local Method ----- \\

            void OnComplete(bool _completed) {
                _isWaiting = false;
            }
        }
        #endregion
    }

    /// <summary>
    /// Fades the 
    /// </summary>
    [Serializable, DisplayName("Screen Fade")]
    public sealed class FadingSingletonSplashAnimation : SplashAnimation {
        #region Global Members
        public FadingMode Mode = FadingMode.Show;
        public bool WaitForCompletion = true;
        [Enhanced, ShowIf(nameof(ShowWait)), Range(0f, 5f)] public float WaitDuration = .5f;

        public bool ShowWait {
            get { return Mode == FadingMode.FadeInOut; }
        }
        #endregion

        #region Behaviour
        public override IEnumerator Play() {
            bool _isWaiting = WaitForCompletion;

            ScreenFadingGroup.Instance.Fade(Mode, OnComplete, WaitDuration);

            while (_isWaiting) {
                yield return null;
            }

            // ----- Local Method ----- \\

            void OnComplete(bool _completed) {
                _isWaiting = false;
            }
        }
        #endregion
    }
}
