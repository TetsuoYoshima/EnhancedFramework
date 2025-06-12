// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.Option;
using System;
using UnityEngine;
using UnityEngine.Localization;

using DisplayName = EnhancedEditor.DisplayNameAttribute;

namespace EnhancedFramework.Localization {

    /// <summary>
    /// <see cref="BaseGameOption"/> used to change the current language of the game.
    /// </summary>
    [Serializable, DisplayName("General/Language")]
    public sealed class LanguageOption : BaseGameOption {
        #region Global Members
        [Space(10f)]

        [Tooltip("Current language of the game")]
        [SerializeField, Enhanced, Required, DisplayName("Default")] private Locale language = null;

        [Tooltip("All available language of the game")]
        [SerializeField] private Locale[] allLanguage = new Locale[0];

        [Tooltip("Identifier of the selected language")]
        [SerializeField, HideInInspector] private LocaleIdentifier languageIdentifier = default;

        // -----------------------

        /// <summary>
        /// Game language.
        /// </summary>
        public Locale Language {
            get { return language; }
            set {
                if (value == null) {
                    return;
                }

                language = value;
                languageIdentifier = value.Identifier;

                Apply();
            }
        }

        /// <summary>
        /// Total count of available language in the game.
        /// </summary>
        public int LanguageCount {
            get { return allLanguage.Length; }
        }

        /// <summary>
        /// The index of the active selected language of the game.
        /// </summary>
        public int SelectedLanguageIndex {
            get { return Array.IndexOf(allLanguage, language); }
            set { Language = GetLanguage(value); }
        }

        // -----------------------

        public override int SelectedValueIndex {
            get { return SelectedLanguageIndex; }
            set { SelectedLanguageIndex = value; }
        }

        public override int AvailableOptionCount {
            get { return LanguageCount; }
        }
        #endregion

        #region Behaviour
        public override void Apply(bool _isInit = false) {
            LocalizationManager.Instance.SelectLocale(language);
        }

        public override void Refresh() {
            language =  LocalizationManager.Instance.SelectedLocale;
        }

        public override void Initialize(BaseGameOption _option) {

            if (_option is not LanguageOption _language) {
                return;
            }

            allLanguage = _language.allLanguage;

            // Retrieve locale.
            Locale _locale = language;

            if ((_locale == null) && FindLocale(out Locale _temp)) {
                _locale = _temp;
            }

            Language = _locale;

            // ----- Local Method ----- \\

            bool FindLocale(out Locale _locale) {

                ref Locale[] _span = ref allLanguage;
                for (int i = _span.Length; i-- > 0;) {

                    _locale = _span[i];
                    if (_locale.Identifier == languageIdentifier)
                        return true;
                }

                _locale = null;
                return false;
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get a game language at a specific index.
        /// <para/>
        /// Use <see cref="LanguageCount"/> to get the total count of available language in the game.
        /// </summary>
        /// <param name="_index">Index of the language to get.</param>
        /// <returns>The language at the given index.</returns>
        public Locale GetLanguage(int _index) {
            return allLanguage[_index];
        }
        #endregion
    }
}
