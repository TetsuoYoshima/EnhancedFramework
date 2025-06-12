// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EnhancedFramework.Localization {
    /// <summary>
    /// Implement this interface to easily load and unload an object associated
    /// <br/> localization tables, using a <see cref="LocalizationResourceLoader"/>.
    /// </summary>
    public interface ILocalizable {
        #region Content
        /// <summary>
        /// Get all localization tables associated with this object instance.
        /// </summary>
        /// <param name="_stringTables">String table collection to fill.</param>
        /// <param name="_assetTables">Asset table collection to fill.</param>
        void GetLocalizationTables(Set<TableReference> _stringTables, Set<TableReference> _assetTables);
        #endregion
    }

    /// <summary>
    /// Interface used to receives callbacks on multiple localization-related events.
    /// <para/>
    /// Must be registered on initialization and unregistered on deactivation using
    /// <see cref="LocalizationManager.Register(ILocalizer)"/> and <see cref="LocalizationManager.Unregister(ILocalizer)"/>.
    /// </summary>
    public interface ILocalizer {
        #region Content
        /// <summary>
        /// Called when the game active locale is changed.
        /// </summary>
        /// <param name="_locale">New active locale (use this to update localized strings and assets).</param>
        void OnLocaleChanged(Locale _locale);
        #endregion
    }

    /// <summary>
    /// <see cref="ResourceLoader{T}"/> used to load various localization tables from a <see cref="ILocalizable"/>.
    /// </summary>
    [Serializable]
    public sealed class LocalizationResourceLoader : ResourceLoader<LocalizationResourceLoader> {
        #region Global Members
        public override bool IsProcessing {
            get { return currentOperations.Count != 0; }
        }

        // -----------------------

        private readonly Set<TableReference> stringTables = new Set<TableReference>();
        private readonly Set<TableReference> assetTables  = new Set<TableReference>();

        private readonly List<AsyncOperationHandle> currentOperations = new List<AsyncOperationHandle>();
        #endregion

        #region Behaviour
        public override void Load() {
            // Load all localization tables, previously setup in the Fill method.
            if (stringTables.Count != 0) {
                ManageOperation(LocalizationManager.Instance.LoadStringTables(stringTables));
            }

            if (assetTables.Count != 0) {
                ManageOperation(LocalizationManager.Instance.LoadAssetTables(assetTables));
            }

            // ----- Local Methods ----- \\

            void ManageOperation(AsyncOperationHandle _handle) {
                if (_handle.IsDone) {
                    return;
                }

                currentOperations.Add(_handle);
                _handle.Completed += OnOperationComplete;
            }

            void OnOperationComplete(AsyncOperationHandle _handle) {
                currentOperations.Remove(_handle);
            }
        }

        public override void Unload() {
            // Release all loaded tables.
            if (stringTables.Count != 0) {
                LocalizationManager.Instance.ReleaseStringTables(stringTables);
            }

            if (assetTables.Count != 0) {
                LocalizationManager.Instance.ReleaseAssetTables(assetTables);
            }
        }

        // -----------------------

        /// <summary>
        /// Fills this loader localization tables with a specific <see cref="ILocalizable"/> instance.
        /// </summary>
        /// <param name="_localizable">The <see cref="ILocalizable"/> to preload tables.</param>
        public void FillTables(ILocalizable _localizable) {
            _localizable.GetLocalizationTables(stringTables, assetTables);
        }
        #endregion
    }

    // ===== Manager ===== \\

    /// <summary>
    /// Localization singleton class, managing all localization-related operations (like tables loading and selected locale).
    /// </summary>
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(-200)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Localization/Localization Manager"), DisallowMultipleComponent]
    public sealed class LocalizationManager : EnhancedSingleton<LocalizationManager>, ILoadingProcessor {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Loading Processor
        public override bool IsLoadingProcessor => true;

        // True while any localization table async operation is running.
        public bool IsProcessing {
            get { return operations.Count != 0; }
        }
        #endregion

        #region Global Members
        [Section("Localization Manager")]

        [Tooltip("If true, unload unused tables when possible")]
        [SerializeField] private bool unloadTables = true;

        [Space(10f)]

        [Tooltip("All string tables to load on game init")]
        [SerializeField] private LocalizedStringTable[] generalStrings = new LocalizedStringTable[0];

        [Tooltip("All asset tables to load on game init")]
        [SerializeField] private LocalizedAssetTable[] generalAssets = new LocalizedAssetTable[0];

        // -----------------------

        private readonly EnhancedCollection<ILocalizer> localizables = new EnhancedCollection<ILocalizer>();
        private readonly List<AsyncOperationHandle> operations = new List<AsyncOperationHandle>();
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Locale callback.
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        protected override void OnInit() {
            base.OnInit();

            // Initialize the localization system.
            // Tables can't be preloaded while not initialized.
            AsyncOperationHandle<LocalizationSettings> _handle = LocalizationSettings.InitializationOperation;

            if (!_handle.IsDone) {
                operations.Add(_handle);
                _handle.Completed += OnComplete;
            }

            // Load.
            ref LocalizedAssetTable[] _assetSpan = ref generalAssets;
            for (int i = _assetSpan.Length; i-- > 0;) {
                LoadAssetTable(_assetSpan[i].TableReference);
            }

            ref LocalizedStringTable[] _stringSpan = ref generalStrings;
            for (int i = _stringSpan.Length; i-- > 0;) {
                LoadStringTable(_stringSpan[i].TableReference);
            }

            // ----- Local Method ----- \\

            void OnComplete(AsyncOperationHandle<LocalizationSettings> _operation) {
                _operation.Completed -= OnComplete;
                operations.Remove(_operation);

                switch (_operation.Status) {
                    case AsyncOperationStatus.Succeeded:
                        this.LogMessage("Localization initialization successfully completed");
                        break;

                    case AsyncOperationStatus.Failed:
                        this.LogErrorMessage("Localization initialization failed");
                        break;

                    case AsyncOperationStatus.None:
                    default:
                        this.LogWarningMessage("Localization initialization status unknown");
                        break;
                }
            }
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Locale callback.
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }
        #endregion

        #region Localizable
        /// <summary>
        /// Registers a new localizer instance.
        /// </summary>
        /// <param name="_localizable">The localizer to register.</param>
        public void Register(ILocalizer _localizable) {
            localizables.Add(_localizable);
        }

        /// <summary>
        /// Unregisters a specific localizer.
        /// </summary>
        /// <param name="_localizable">The localizer to unregister.</param>
        public void Unregister(ILocalizer _localizable) {
            localizables.Remove(_localizable);
        }
        #endregion

        #region Locale
        /// <summary>
        /// Called whenever selecting a locale (even if it has not changed).
        /// </summary>
        public static Action<Locale> OnSelectLocale = null;

        /// <summary>
        /// The active selected locale of the game.
        /// </summary>
        public Locale SelectedLocale {
            get { return LocalizationSettings.SelectedLocale; }
        }

        // -----------------------

        /// <summary>
        /// Selects and activates a specific locale.
        /// </summary>
        /// <param name="_locale">The new locale to select.</param>
        public void SelectLocale(Locale _locale) {
            Instance.LogMessage($"Select Locale \'{_locale?.name}\'");

            LocalizationSettings.SelectedLocale = _locale;
            OnSelectLocale?.Invoke(_locale);
        }

        private void OnLocaleChanged(Locale _locale) {
            this.LogWarningMessage($"Changed Locale to \'{_locale?.name}\'");

            // Update localizables.
            ref List<ILocalizer> _span = ref localizables.collection;

            for (int i = _span.Count; i-- > 0;) {
                _span[i].OnLocaleChanged(_locale);
            }
        }
        #endregion

        #region Loading
        /// <summary>
        /// Loads a specific string table.
        /// </summary>
        /// <param name="_table">The string table to load.</param>
        public AsyncOperationHandle LoadStringTable(TableReference _table) {
            return ManageAsyncOperationHandle(LocalizationSettings.StringDatabase.PreloadTables(_table));
        }

        /// <summary>
        /// Loads a collection of string tables.
        /// </summary>
        /// <param name="_tables">All string tables to load.</param>
        public AsyncOperationHandle LoadStringTables(IList<TableReference> _tables) {
            return ManageAsyncOperationHandle(LocalizationSettings.StringDatabase.PreloadTables(_tables));
        }

        /// <summary>
        /// Loads a specific asset table.
        /// </summary>
        /// <param name="_table">The asset table to load.</param>
        public AsyncOperationHandle LoadAssetTable(TableReference _table) {
            return ManageAsyncOperationHandle(LocalizationSettings.AssetDatabase.PreloadTables(_table));
        }

        /// <summary>
        /// Loads a collection of asset tables.
        /// </summary>
        /// <param name="_tables">All asset tables to load.</param>
        public AsyncOperationHandle LoadAssetTables(IList<TableReference> _tables) {
            return ManageAsyncOperationHandle(LocalizationSettings.AssetDatabase.PreloadTables(_tables));
        }

        // -----------------------

        private AsyncOperationHandle ManageAsyncOperationHandle(AsyncOperationHandle _handle) {
            // Use a callback if the operation is not complete.
            if (_handle.IsDone) {
                OnAsyncOperationComplete(_handle);
                return _handle;
            }

            operations.Add(_handle);
            _handle.Completed += OnComplete;

            return _handle;

            // ----- Local Method ----- \\

            void OnComplete(AsyncOperationHandle _operation) {
                _operation.Completed -= OnComplete;

                operations.Remove(_handle);
                OnAsyncOperationComplete(_operation);
            }
        }

        private void OnAsyncOperationComplete(AsyncOperationHandle _handle) {
            switch (_handle.Status) {
                // Everything is fine.
                case AsyncOperationStatus.Succeeded:
                    this.LogMessage("Localization Table loading operation successfully completed");
                    break;

                // Error.
                case AsyncOperationStatus.Failed:
                    this.LogErrorMessage("Localization Table loading operation failed");
                    break;

                // Unknown.
                case AsyncOperationStatus.None:
                default:
                    this.LogWarningMessage("Localization Table loading operation status unknown");
                    break;
            }
        }
        #endregion

        #region Release
        /// <summary>
        /// Releases a collection of string tables.
        /// </summary>
        /// <param name="_tables">All string tables to release.</param>
        public void ReleaseStringTables(IList<TableReference> _tables) {
            for (int i = _tables.Count; i-- > 0;) {
                ReleaseStringTable(_tables[i]);
            }
        }

        /// <summary>
        /// Releases a collection of asset tables.
        /// </summary>
        /// <param name="_tables">All asset tables to release.</param>
        public void ReleaseAssetTables(IList<TableReference> _tables) {
            for (int i = _tables.Count; i-- > 0;) {
                ReleaseAssetTable(_tables[i]);
            }
        }

        /// <summary>
        /// Releases a specific string table.
        /// </summary>
        /// <param name="_table">The string table to release.</param>
        public void ReleaseStringTable(TableReference _table) {
            
            if (!unloadTables) {
                return;
            }

            LocalizationSettings.StringDatabase.ReleaseTable(_table);
        }

        /// <summary>
        /// Releases a specific asset table.
        /// </summary>
        /// <param name="_table">The asset table to release.</param>
        public void ReleaseAssetTable(TableReference _table) {

            if (!unloadTables) {
                return;
            }

            LocalizationSettings.AssetDatabase.ReleaseTable(_table);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Utility method only used from the component inspector window.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Green)]
        #pragma warning disable IDE0051
        private void GetOperationCount() {
            this.LogWarningMessage($"Localization async operation count - {operations.Count}");
        }
        #endregion

        #region Logger
        public override Color GetLogMessageColor(LogType _type) {
            return SuperColor.DarkOrange.Get();
        }
        #endregion
    }
}
