// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using UnityEngine;
using UnityEngine.Playables;

using Object = UnityEngine.Object;

namespace EnhancedFramework.Timeline {
	/// <summary>
	/// References all possible <see cref="EnhancedPlayableBehaviour"/> states.
	/// </summary>
	public enum PlayableState {
		Inactive = 0,

		Playing  = 1,
		Paused   = 2,
	}

    /// <summary>
    /// Base Enhanced <see cref="PlayableBehaviour"/> class.
    /// </summary>
    [Serializable]
    public abstract class EnhancedPlayableBehaviour : PlayableBehaviour {
        #region Global Members
        [Section("Clip Settings")]

        [Tooltip("Indicates whether this behaviour should be executed in edit mode or not")]
        [Enhanced, ShowIf(nameof(CanExecuteInEditMode))] public bool ExecuteInEditMode = true;

        // -----------------------

        /// <summary>
        /// Indicates if this behaviour can be executed in edit mode.
        /// </summary>
        protected virtual bool CanExecuteInEditMode {
			get { return true; }
		}
		#endregion

		#region Behaviour
		private PlayableState state = PlayableState.Inactive;

		// -----------------------

		public override void OnBehaviourPlay(Playable _playable, FrameData _info) {
            base.OnBehaviourPlay(_playable, _info);

			#if UNITY_EDITOR
			if ((!CanExecuteInEditMode || !ExecuteInEditMode) && !Application.isPlaying) {
				return;
			}
            #endif

            switch (state) {

				// Already playing.
				case PlayableState.Playing:
					return;

				// Initialize and play.
                case PlayableState.Inactive:
                    OnPlay(_playable, _info);
                    OnResume(_playable, _info);
                    break;

				// Resume from pause.
                case PlayableState.Paused:
                    OnResume(_playable, _info);
                    break;

				default:
					throw new IndexOutOfRangeException($"Invalid Playable state ({state})");
			}

            SetState(PlayableState.Playing);
        }

        public override void OnBehaviourPause(Playable _playable, FrameData _info) {
            base.OnBehaviourPause(_playable, _info);

			#if UNITY_EDITOR
			if ((!ExecuteInEditMode || !ExecuteInEditMode) && !Application.isPlaying) {
				return;
			}
            #endif

            bool _isStopped = (_info.effectivePlayState == PlayState.Paused) || _playable.GetGraph().GetRootPlayable(0).IsDone();
            bool _completed = Mathf.Approximately((float)_playable.GetTime(), (float)_playable.GetDuration());

            switch (state) {

                // Already inactive.
                case PlayableState.Inactive:
                    return;

                // Pause & Stop.
                case PlayableState.Playing:

                    OnPause(_playable, _info);

                    if (_isStopped) {
                        OnStop(_playable, _info, _completed);
                    }

                    break;

                // Stop.
                case PlayableState.Paused:

                    if (_isStopped) {
                        OnStop(_playable, _info, _completed);
                    }

                    break;

                default:
                    throw new IndexOutOfRangeException($"Invalid Playable state ({state})");
            }

            SetState(_isStopped ? PlayableState.Inactive : PlayableState.Paused);
        }

        // -------------------------------------------
        // Callbacks
        // -------------------------------------------

        /// <summary>
        /// Called when this playable starts being played.
        /// <para/>
        /// Guaranteed to be called before the OnResume callback.
        /// </summary>
        /// <param name="_playable">The <see cref="Playable"/> that owns the current <see cref="PlayableBehaviour"/>.</param>
        /// <param name="_info">A <see cref="FrameData"/> structure that contains information about the current frame context.</param>
        protected virtual void OnPlay(Playable _playable, FrameData _info) { }

        /// <summary>
        /// Called when this playable is resumed after being paused.
        /// <para/>
        /// Called right after the OnPlay callback.
        /// </summary>
        /// <inheritdoc cref="OnPlay(Playable, FrameData)"/>
        protected virtual void OnResume(Playable _playable, FrameData _info) { }

        /// <summary>
        /// Called when this playable is paused.
        /// <para/>
        /// Called right before the OnStop callback.
        /// </summary>
        /// <inheritdoc cref="OnPlay(Playable, FrameData)"/>
        protected virtual void OnPause(Playable _playable, FrameData _info) { }

        /// <summary>
        /// Called when this playable is stopped.
        /// <para/>
        /// Guaranteed to be called after the OnPause callback.
        /// </summary>
        /// <param name="_completed">Indicates if the playable is being completed or not.</param>
        /// <inheritdoc cref="OnPlay(Playable, FrameData)"/>
        protected virtual void OnStop(Playable _playable, FrameData _info, bool _completed) { }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get the normalized time (between 0 and 1) of this behaviour.
        /// </summary>
        /// <returns>This behaviour normalized time (between 0 and 1).</returns>
        /// <inheritdoc cref="OnPlay(Playable, FrameData)"/>
        public float GetNormalizedTime(Playable _playable) {
            double _time = _playable.GetTime();
            if (_time != 0d) {
                _time /= _playable.GetDuration();
            }

            return (float)_time;
        }

        private void SetState(PlayableState _state) {
			state = _state;
		}
        #endregion
    }

    /// <summary>
    /// Base Enhanced <see cref="PlayableBehaviour"/> class with a bound object.
    /// </summary>
    [Serializable]
    public abstract class EnhancedPlayableBehaviour<T> : EnhancedPlayableBehaviour where T : Object {
        #region Behaviour
        /// <summary>
        /// The <see cref="T"/> binding object of this playable.
        /// </summary>
        [SerializeField, HideInInspector] internal protected T bindingObject = null;
        #endregion
    }
}
