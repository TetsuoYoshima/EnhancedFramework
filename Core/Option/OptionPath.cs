// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.IO;
using UnityEngine;

namespace EnhancedFramework.Core.Option {
    /// <summary>
    /// Determines where the <see cref="OptionSettings"/> data are saved on disk.
    /// </summary>
    public enum OptionPath {
        [Tooltip("Options are not saved on disk, and reset after each play")]
        None = 0,

        [Separator(SeparatorPosition.Top)]

        [Tooltip("Project / Executable data folder path")]
        ApplicationPath = 1,

        [Tooltip("Persistent data directory (AppData...)")]
        PersistentPath  = 2,

        [Tooltip("My Games folder, in My Documents")]
        MyGames         = 3,
    }

    /// <summary>
    /// Contains multiple <see cref="OptionPath"/>-related extension methods.
    /// </summary>
    public static class OptionPathExtensions {
        #region Content
        /// <summary>
        /// Get this <see cref="OptionPath"/> associated path.
        /// </summary>
        /// <param name="_path">Path to get.</param>
        /// <param name="_autoCreate">If true, automatically creates the directory if it does not exist.</param>
        /// <returns>This path full directory value..</returns>
        public static string Get(this OptionPath _path, bool _autoCreate = true) {

            string _directory;
            switch (_path) {

                case OptionPath.ApplicationPath:
                    _directory = Application.dataPath;
                    break;

                case OptionPath.PersistentPath:
                    _directory = Application.persistentDataPath;
                    break;

                case OptionPath.MyGames:
                    return EnhancedUtility.GetMyGamesDirectoryPath(_autoCreate);

                case OptionPath.None:
                default:
                    return string.Empty;
            }

            if (_autoCreate && !Directory.Exists(_directory)) {
                Directory.CreateDirectory(_directory);
            }

            return _directory;
        }
        #endregion
    }
}
