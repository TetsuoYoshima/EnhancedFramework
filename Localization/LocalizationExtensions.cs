// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EnhancedFramework.Localization {
	/// <summary>
	/// Contains multiple extension methods related to the <see cref="LocalizedString"/> class.
	/// </summary>
	public static class LocalizedStringExtensions {
        #region Content
        /// <returns><inheritdoc cref="GetLocalizedValue(LocalizedString, out string)" path="/param[@name='_value']"/></returns>
        /// <inheritdoc cref="GetLocalizedValue(LocalizedString, out string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetLocalizedValue(this LocalizedString _string) {
			GetLocalizedValue(_string, out string _value);
			return _value;
		}

        /// <summary>
        /// Get the localized text value of this <see cref="LocalizedString"/>.
        /// </summary>
        /// <param name="_string">The <see cref="LocalizedString"/> to get the associated string value.</param>
        /// <param name="_value">The localized text value of the string.</param>
        /// <returns>True if this <see cref="LocalizedString"/> is referencing a valid entry, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetLocalizedValue(this LocalizedString _string, out string _value) {
			if (!GetLocalizedTable(_string, out _)) {
				_value = string.Empty;
				return false;
			}

			_value = _string.GetLocalizedString();

			#if DEVELOPMENT
			if (string.IsNullOrEmpty(_value)) {
				_value = "MISSING LOCALIZATION";
			}
			#endif

			return true;
		}

        /// <summary>
        /// Get the localized text value of this <see cref="LocalizedString"/> asynchronously.
        /// </summary>
        /// <param name="_string">The <see cref="LocalizedString"/> to get the associated string value.</param>
        /// <param name="_handle">The <see cref="AsyncOperationHandle"/> used to load this string localized text value.</param>
        /// <returns>True if this <see cref="LocalizedString"/> is referencing a valid entry, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetLocalizedValueAsync(this LocalizedString _string, out AsyncOperationHandle<string> _handle) {
			if (!GetLocalizedTable(_string, out _)) {
				_handle = default;
				return false;
			}

			_handle = _string.GetLocalizedStringAsync();
			return true;
		}

        /// <summary>
        /// Get the <see cref="TableReference"/> of this <see cref="LocalizedString"/>.
        /// </summary>
        /// <param name="_string">The <see cref="LocalizedString"/> to get the associated table.</param>
        /// <param name="_table">The <see cref="TableReference"/> of this string.</param>
        /// <returns>True if a valid table could be found, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetLocalizedTable(this LocalizedString _string, out TableReference _table) {
			if ((_string != null) && !string.IsNullOrEmpty(_table = _string.TableReference)) {
				return true;
			}

			_table = default;
			return false;
		}

        /// <summary>
        /// Set the localized text value of this <see cref="LocalizedString"/>.
        /// </summary>
        /// <param name="_string">The <see cref="LocalizedString"/> to set the associated string value.</param>
        /// <param name="_text">The localized text of the string for the active locale.</param>
        /// <returns>True if the string value was successfully set, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SetLocalizedValue(this LocalizedString _string, string _text) {
			if ((_string == null) || string.IsNullOrEmpty(_string.TableReference)) {
				return false;
			}

			StringTableEntry _entry = LocalizationSettings.StringDatabase.GetTableEntry(_string.TableReference, _string.TableEntryReference).Entry;
			_entry.Value = _text;

			return true;
		}
		#endregion
	}

	/// <summary>
	/// Contains multiple extension methods related to the <see cref="LocalizedAsset{T}"/> class.
	/// </summary>
	public static class LocalizedAssetExtensions {
        #region Content
        /// <returns><inheritdoc cref="GetLocalizedValue{T}(LocalizedAsset{T}, out T)" path="/param[@name='_value']"/></returns>
        /// <inheritdoc cref="GetLocalizedValue{T}(LocalizedAsset{T}, out T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetLocalizedValue<T>(this LocalizedAsset<T> _asset) where T : Object {
			GetLocalizedValue(_asset, out T _value);
			return _value;
		}

        /// <summary>
        /// Get the localized asset value of this <see cref="LocalizedAsset{T}"/>.
        /// </summary>
        /// <param name="_asset">The <see cref="LocalizedAsset{T}"/> to get the associated asset value.</param>
        /// <param name="_value">The localized asset value.</param>
        /// <returns>True if this <see cref="LocalizedAsset{T}"/> is referencing a valid entry, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetLocalizedValue<T>(this LocalizedAsset<T> _asset, out T _value) where T : Object {
			if (!GetLocalizedTable(_asset, out _)) {
				_value = null;
				return false;
			}

			_value = _asset.LoadAsset();
			return true;
		}

        /// <summary>
        /// Get the localized asset value of this <see cref="LocalizedAsset{T}"/> asynchronously.
        /// </summary>
        /// <param name="_asset">The <see cref="LocalizedAsset{T}"/> to get the associated asset value.</param>
        /// <param name="_handle">The <see cref="AsyncOperationHandle"/> used to load this localized asset value.</param>
        /// <returns>True if this <see cref="LocalizedAsset{T}"/> is referencing a valid entry, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetLocalizedValueAsync<T>(this LocalizedAsset<T> _asset, out AsyncOperationHandle<T> _handle) where T : Object {
			if (!GetLocalizedTable(_asset, out _)) {
				_handle = default;
				return false;
			}

			_handle = _asset.LoadAssetAsync();
			return true;
		}

        /// <summary>
        /// Get the <see cref="TableReference"/> of this <see cref="LocalizedAsset{T}"/>.
        /// </summary>
        /// <param name="_asset">The <see cref="LocalizedAsset{T}"/> to get the associated table.</param>
        /// <param name="_table">The <see cref="TableReference"/> of this asset.</param>
        /// <returns>True if a valid table could be found, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetLocalizedTable<T>(this LocalizedAsset<T> _asset, out TableReference _table) where T : Object {
			if ((_asset != null) && !string.IsNullOrEmpty(_table = _asset.TableReference)) {
				return true;
			}

			_table = default;
			return false;
		}

        /// <summary>
        /// Set the localized asset value of this <see cref="LocalizedAsset{T}"/>.
        /// </summary>
        /// <param name="_asset">The <see cref="LocalizedAsset{T}"/> to set the associated asset value.</param>
        /// <param name="_value">The localized value of the asset for the active locale.</param>
        /// <returns>True if the asset value was successfully set, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SetLocalizedValue<T>(this LocalizedAsset<T> _asset, T _value) where T : Object {
			if ((_asset == null) || string.IsNullOrEmpty(_asset.TableReference)) {
				return false;
			}

			AssetTableEntry _entry = LocalizationSettings.AssetDatabase.GetTableEntry(_asset.TableReference, _asset.TableEntryReference).Entry;
			_entry.SetAssetOverride(_value);

			return true;
		}
		#endregion
	}
}
