// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if TEXT_MESH_PRO_PACKAGE
using System.Text;
using TMPro;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Contains multiple <see cref="TextMeshProUGUI"/>-related extension methods.
    /// </summary>
    public static class TextMeshProUGUIExtensions {
        #region Content
        private static readonly StringBuilder stringBuilder = new StringBuilder();

        // -----------------------

        /// <summary>
        /// Get the visible <see cref="string"/> content of a specific <see cref="TextMeshProUGUI"/>.
        /// </summary>
        /// <param name="_text"><see cref="TextMeshProUGUI"/> to get the visible text.</param>
        /// <returns>Visible <see cref="string"/> content of this <see cref="TextMeshProUGUI"/></returns>
        public static string GetVisibleText(this TextMeshProUGUI _text) {

            stringBuilder.Clear();

            TMP_TextInfo _info  = _text.textInfo;
            int _characterCount = _info.characterCount;

            for (int i = 0; i < _characterCount; i++) {

                TMP_CharacterInfo _character = _info.characterInfo[i];
                stringBuilder.Append(_character.character);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Get the visible <see cref="string"/> content of a specific <see cref="TextMeshProUGUI"/> current page.
        /// </summary>
        /// <param name="_text"><see cref="TextMeshProUGUI"/> to get the current page visible text.</param>
        /// <returns>Visible <see cref="string"/> content of this <see cref="TextMeshProUGUI"/> current page</returns>
        public static string GetPageVisibleText(this TextMeshProUGUI _text) {

            TMP_PageInfo _pageInfo = _text.textInfo.pageInfo[_text.pageToDisplay - 1];
            int _firstCharIndex    = _pageInfo.firstCharacterIndex;
            int _lastCharIndex     = _pageInfo.lastCharacterIndex;

            return _text.GetVisibleText().Substring(_firstCharIndex, (_lastCharIndex - _firstCharIndex) + 1);
        }

        /// <summary>
        /// Get the total count of characters from a <see cref="TextMeshProUGUI"/>, compared to its visible character count.
        /// </summary>
        /// <param name="_text"><see cref="TextMeshProUGUI"/> to get the total count of visible character.</param>
        /// <returns>Total amount of characters displayed for the specified amount of visible characters.</returns>
        public static int GetVisibleCharacterCount(this TextMeshProUGUI _text) {

            TMP_TextInfo _info = _text.textInfo;
            int _count = 0;

            for (int i = _info.characterCount; i-- > 0;) {

                TMP_CharacterInfo _character = _info.characterInfo[i];
                if (_character.isVisible) {
                    _count++;
                }
            }

            return _count;
        }
        #endregion
    }
}
#endif
