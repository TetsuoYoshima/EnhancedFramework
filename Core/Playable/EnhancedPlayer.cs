// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Base interface for any special player.
    /// </summary>
    public interface IEnhancedPlayer {
        #region Content
        /// <summary>
        /// Plays or resumes this player content.
        /// </summary>
        void Play();

        /// <summary>
        /// Pauses this player.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stops playing this player.
        /// </summary>
        void Stop();
        #endregion
    }

    /// <summary>
    /// Base <see cref="EnhancedBehaviour"/> for an <see cref="IEnhancedPlayer"/>.
    /// </summary>
    public abstract class EnhancedPlayer : EnhancedBehaviour, IEnhancedPlayer {
        #region Player
        /// <inheritdoc cref="IEnhancedPlayer.Play"/>
        [Button(SuperColor.Green)]
        public abstract void Play();

        /// <inheritdoc cref="IEnhancedPlayer.Pause"/>
        [Button(SuperColor.Orange)]
        public abstract void Pause();

        /// <inheritdoc cref="IEnhancedPlayer.Stop"/>
        [Button(SuperColor.Crimson)]
        public abstract void Stop();
        #endregion
    }
}
