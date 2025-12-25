// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.Settings;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Progression saving related settings.
    /// </summary>
    public sealed class SaveSettings : BaseSettings<SaveSettings> {
        #region Wrapper
        [Serializable]
        internal class SavableDataGroup {
            public string Name = "New Group";
            public EnhancedScriptableObject[] Data = new EnhancedScriptableObject[0];
        }
        #endregion

        #region Global Members
        [Section("Save Settings")]

        [Tooltip("Main Path where to write save files on disk")]
        [SerializeField] private FilePath path = FilePath.PersistentPath;

        [Tooltip("Path subfolder where to write save files on disk")]
        [SerializeField] private string subFolder = string.Empty;

        [Space(10f)]

        [Tooltip("Extension used for writing save files on disk")]
        [SerializeField] private string fileExtension = "sav";

        [Tooltip("Default name for save file to write on disk")]
        [SerializeField] private string fileName = "SaveFile{0}";

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        #if UNITY_EDITOR
        [SerializeField, Enhanced, ShowIf(nameof(foo))] private bool foo = false; // Editor field.
        #endif

        [Tooltip("All Scriptable Objects to be automatically saved")]
        [SerializeField] private SavableDataGroup[] savableScriptableObjects = new SavableDataGroup[0];

        // -----------------------

        /// <summary>
        /// Save files full base directory.
        /// </summary>
        public string FileDirectory {
            get { return path.Get(subFolder, true); }
        }
        #endregion

        #region Initialization
        protected internal override void Init() {
            base.Init();

            // Register global static data.
            SaveManager.Instance.SetStaticGlobalObjects(savableScriptableObjects);
        }
        #endregion

        #region File
        private readonly BinaryFormatter formatter = new BinaryFormatter();

        // -----------------------

        /// <summary>
        /// Editor method used to debug file content.
        /// </summary>
        [Button(SuperColor.Green)]
        public void LogFile(string _fileName) {
            if (ReadFile(_fileName, out string _content)) {
                this.LogMessage(_content);
            } else {
                this.LogErrorMessage("File could not be opened or read");
            }
        }

        /// <summary>
        /// Loads and reads a given save file content from disk.
        /// </summary>
        public bool ReadFile(string _fileName, out string _content) {
            string _filePath = GetFilePath(_fileName);

            if (File.Exists(_filePath)) {
                //_content = File.ReadAllText(_filePath);

                try {
                    FileStream _stream = File.Open(_filePath, FileMode.Open);
                    _content = formatter.Deserialize(_stream) as string;
                    _stream.Close();

                    return true;
                } catch (Exception e) {
                    this.LogErrorMessage("An error occured - file could not be read from disk. See exception log for additional information");
                    this.LogException(e);
                }
            }

            _content = string.Empty;
            return false;
        }

        /// <summary>
        /// Saves and writes a given save content in a file on disk.
        /// </summary>
        [Button(SuperColor.HarvestGold)]
        public bool WriteFile(string _content, string _fileName) {
            string _filePath = GetFilePath(_fileName);
            //File.WriteAllText(_filePath, _content);

            try {
                FileStream _stream = File.Open(_filePath, FileMode.OpenOrCreate);
                formatter.Serialize(_stream, _content);
                _stream.Close();
            } catch (Exception e) {
                this.LogErrorMessage("An error occured - file could not be written on disk. See exception log for additional information");
                this.LogException(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deletes a given save file from disk.
        /// </summary>
        [Button(SuperColor.Crimson, IsDrawnOnTop = false)]
        public void DeleteFile(string _fileName) {
            string _filePath = GetFilePath(_fileName);
            if (File.Exists(_filePath)) {
                File.Delete(_filePath);
            }
        }

        /// <summary>
        /// Clears and deletes all save files from disk.
        /// </summary>
        [Button(SuperColor.Crimson, IsDrawnOnTop = false)]
        public void DeleteAll() {
            string[] _files = GetAllFiles();

            for (int i = _files.Length; i-- > 0;) {
                File.Delete(_files[i]);
            }
        }

        /// <summary>
        /// Opens the game save file directory.
        /// </summary>
        [Button(SuperColor.Green, IsDrawnOnTop = false)]
        public void OpenDirectory() {
            string _directory = FileDirectory;
            if (Directory.Exists(_directory)) {
                Application.OpenURL(_directory);
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get the full path for a given file name.
        /// </summary>
        /// <param name="_fileName">Name to get the associated full path.</param>
        /// <returns>Full file path for the given name.</returns>
        public string GetFilePath(string _fileName) {
            return Path.Combine(FileDirectory, $"{_fileName}.{fileExtension}");
        }

        /// <summary>
        /// Get the full name of all game files in the save directory.
        /// </summary>
        public string[] GetAllFiles() {
            string _directory = FileDirectory;
            return Directory.GetFiles(_directory, $"*.{fileExtension}", SearchOption.AllDirectories);
        }
        #endregion
    }
}
