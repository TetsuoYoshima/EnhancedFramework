// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Collections;
using UnityEngine.Localization;

using DisplayName = EnhancedEditor.DisplayNameAttribute;

namespace EnhancedFramework.Localization {
    /// <summary>
    /// Waits until the localization selected locale is changed.
    /// </summary>
    [Serializable, DisplayName("Language Selection")]
    public sealed class LanguageSelectionSplashAnimation : SplashAnimation {
        #region Behaviour
        public override IEnumerator Play() {
            LocalizationManager.OnSelectLocale += OnLocaleChanged;

            bool _isWaiting = true;
            while (_isWaiting) {
                yield return null;
            }

            // ----- Local Method ----- \\

            void OnLocaleChanged(Locale _locale) {
                LocalizationManager.OnSelectLocale -= OnLocaleChanged;
                _isWaiting = false;
            }
        }
        #endregion
    }
}
