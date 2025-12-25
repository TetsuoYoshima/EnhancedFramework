// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.IO;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Used to determines where a file data is saved on disk.
    /// </summary>
    public enum FilePath {
        [Tooltip("File is not saved on disk, and reset after each play")]
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
    /// Contains multiple <see cref="FilePath"/>-related extension methods.
    /// </summary>
    public static class FilePathExtensions {
        #region Content
        /// <inheritdoc cref="Get(FilePath, string, bool)"/>
        public static string Get(this FilePath _path, bool _autoCreate = true) {
            return Get(_path, string.Empty, _autoCreate);
        }

        /// <summary>
        /// Get this <see cref="FilePath"/> associated path.
        /// </summary>
        /// <param name="_path">Path to get.</param>
        /// <param name="_path">Path additional subfolder.</param>
        /// <param name="_autoCreate">If true, automatically creates the directory if it does not exist.</param>
        /// <returns>This path full directory value..</returns>
        public static string Get(this FilePath _path, string _subFolder, bool _autoCreate = true) {

            string _directory;
            switch (_path) {

                case FilePath.ApplicationPath:
                    _directory = Application.dataPath;
                    break;

                case FilePath.PersistentPath:
                    _directory = Application.persistentDataPath;
                    break;

                case FilePath.MyGames:
                    return EnhancedUtility.GetMyGamesDirectoryPath(_autoCreate);

                case FilePath.None:
                default:
                    return string.Empty;
            }

            if (!string.IsNullOrEmpty(_subFolder)) {
                _directory = Path.Combine(_directory, _subFolder);
            }

            if (_autoCreate && !Directory.Exists(_directory)) {
                Directory.CreateDirectory(_directory);
            }

            return _directory;
        }
        #endregion
    }
}
