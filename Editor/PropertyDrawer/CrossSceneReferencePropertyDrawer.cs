// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedEditor.Editor;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Object     = UnityEngine.Object;
using SceneAsset = EnhancedEditor.SceneAsset;

namespace EnhancedFramework.Editor {
    /// <summary>
    /// Custom <see cref="CrossSceneReference{T}"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(CrossSceneReference<>), true)]
    public sealed class CrossSceneReferencePropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        private const int CacheLimit = 100;

        private static readonly string crossSceneReferenceName = typeof(CrossSceneReference<>).Name;
        private static readonly GUIContent copyIDGUI    = new GUIContent("Copy ID", "Copy this object reference id in buffer");
        private static readonly GUIContent pasteIDGUI   = new GUIContent("Paste ID", "Paste the last id copied in buffer");

        private static readonly GUIContent lockGUI      = new GUIContent(EditorGUIUtility.IconContent("IN LockButton on").image, "Toggles the reference GUID edit mode");
        private static readonly GUIContent unlockGUI    = new GUIContent(EditorGUIUtility.IconContent("IN LockButton").image, "Toggles the reference GUID edit mode");

        private static readonly GUIContent notLoadedGUI = new GUIContent(EditorGUIUtility.IconContent("console.infoicon.sml").image,
                                                                         "The reference object scene is currently not loaded");
        private static readonly GUIContent missingGUI   = new GUIContent(EditorGUIUtility.IconContent("console.erroricon.sml").image,
                                                                         "The reference object has been destroyed or could not be found");

        private static readonly Dictionary<string, Pair<EnhancedObjectID, CrossSceneObject>> references = new  Dictionary<string, Pair<EnhancedObjectID, CrossSceneObject>>();
        private static readonly SceneAsset sceneAssetBuffer = new SceneAsset();

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            // ID property.
            SerializedProperty _idProperty = _property.Copy();
            _idProperty.Next(true);
            
            // Variable init.
            _position.height = EditorGUIUtility.singleLineHeight;

            Rect _propertyPosition = new Rect(_position) {
                width = _position.width - (EnhancedEditorGUIUtility.IconWidth + 2f)
            };

            Rect _buttonPosition = new Rect(_position) {
                xMin = _position.xMax - EnhancedEditorGUIUtility.IconWidth
            };

            string _propertyID = EnhancedEditorUtility.GetSerializedPropertyID(_property);
            bool _isLock = _property.isExpanded;

            // Cache limit.
            if (references.Count > CacheLimit) {
                references.Clear();
            }

            EnhancedObjectID _id;

            // Cache the reference object for performance.
            SceneAsset _sceneBuffer = sceneAssetBuffer;

            if (references.TryGetValue(_propertyID, out var _pair)) {
                _id = _pair.First;

                if ((_pair.Second == null) && _id.GetScene(_sceneBuffer) && _sceneBuffer.IsLoaded && GetReference(_id, out CrossSceneObject _object)) {
                    _pair.Second = _object;
                    references[_propertyID] = _pair;
                }
            } else {
                if (!GetMemberValue(_idProperty).GetValue(_idProperty, out _id)) {
                    Debug.LogError($"{typeof(CrossSceneReference<>).Name} field could not be found ({_idProperty.propertyPath})");
                    return 0f;
                }

                CrossSceneObject _object = null;

                bool _shouldLock = _id.GetScene(_sceneBuffer) && (!_sceneBuffer.IsLoaded || !GetReference(_id, out _object));

                if (!_isLock) {
                    _property.isExpanded = _isLock
                                         = _shouldLock;
                }

                _pair = new Pair<EnhancedObjectID, CrossSceneObject>(_id, _object);
                references.Add(_propertyID, _pair);
            }

            // Get the editing field generic type as the component reference type.
            Type _type = GetFieldInfo(_property).FieldType;

            while (!_type.IsGenericType) {
                _type = _type.BaseType;
            }

            _type = _type.GetGenericArguments()[0];

            using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property)) {

                // To avoid the prefix label being affected by the GUIEnabled scope below,
                // draw an empty field outside of the scope.
                _propertyPosition = EditorGUI.PrefixLabel(_propertyPosition, _label);
                EditorGUI.LabelField(_propertyPosition, GUIContent.none);

                using (var _indent = EnhancedEditorGUI.ZeroIndentScope()) {

                    if (_isLock && _id.IsValid && (_pair.Second == null)) {
                        // When the reference could not be found, display an informative help box.
                        GUIContent _gui;

                        if (!_id.GetScene(_sceneBuffer) || _sceneBuffer.IsLoaded) {
                            _gui = missingGUI;
                            _gui.text = $"Missing Reference [{_type.Name.Bold()}]";
                        } else {
                            _gui = notLoadedGUI;
                            _gui.text = $"Unloaded [{_sceneBuffer.Name.Bold()}]";
                        }

                        EditorGUI.LabelField(_propertyPosition, GUIContent.none, EditorStyles.helpBox);

                        _propertyPosition.xMin += 1f;
                        _propertyPosition.y += 1f;
                        _propertyPosition.height -= 2f;

                        EditorGUI.LabelField(_propertyPosition, _gui, EnhancedEditorStyles.RichText);
                    } else {
                        // Object reference field.
                        using (var _enabled = EnhancedGUI.GUIEnabled.Scope(!_isLock))
                        using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                            Component _component = EditorGUI.ObjectField(_propertyPosition, GUIContent.none, _pair.Second, _type, true) as Component;

                            if (_changeCheck.changed) {
                                if (_component == null) {

                                    // Null reference.
                                    _pair = new Pair<EnhancedObjectID, CrossSceneObject>(EnhancedObjectID.Default, null);
                                } else {

                                    // Valid reference.
                                    GameObject _object = _component.gameObject;

                                    _pair.Second = _object.AddComponentIfNone<CrossSceneObject>();
                                    _pair.First = _pair.Second.ID;

                                    EditorUtility.SetDirty(_object);
                                }

                                references[_propertyID] = _pair;
                                _id = _pair.First;

                                _isLock = _property.isExpanded
                                        = true;

                                // Set value.
                                if (GetMemberValue(_idProperty).SetValue(_idProperty, _id)) {
                                    EditorUtility.SetDirty(_property.serializedObject.targetObject);
                                } else {
                                    Debug.LogError($"{typeof(CrossSceneReference<>).Name} field could not be set ({_idProperty.propertyPath})");
                                }
                            }
                        }
                    }
                }
            }

            // Lock toggle.
            if (EnhancedEditorGUI.IconButton(_buttonPosition, _isLock ? lockGUI : unlockGUI)) {
                _property.isExpanded = !_isLock;
            }

            return _position.height;
        }

        private static bool GetReference(EnhancedObjectID _id, out CrossSceneObject _object) {
            CrossSceneObject[] _objects = Object.FindObjectsByType<CrossSceneObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (CrossSceneObject _temp in _objects) {
                if (_temp.ID == _id) {

                    _object = _temp;
                    return true;
                }
            }

            _object = null;
            return false;
        }

        private static MemberValue<EnhancedObjectID> GetMemberValue(SerializedProperty _property) {
            return new MemberValue<EnhancedObjectID>(_property.name);
        }

        // -----------------------

        [SerializedPropertyMenu]
        #pragma warning disable IDE0051
        private static void OnContextMenu(GenericMenu _menu, SerializedProperty _property) {
            if (!_property.type.StartsWith(crossSceneReferenceName)) {
                return;
            }

            SerializedProperty _idProperty = _property.Copy();
            _idProperty.Next(true);

            // Copy ID to clipboard.
            if (GetMemberValue(_idProperty).GetValue(_idProperty, out EnhancedObjectID _id) && _id.IsValid) {

                _menu.AddItem(copyIDGUI, false, () => {

                    GUIUtility.systemCopyBuffer = _id.ToString();
                });

            } else {
                _menu.AddDisabledItem(copyIDGUI);
            }

            // Paste ID from clipboard.
            if (string.IsNullOrEmpty(GUIUtility.systemCopyBuffer)) {
                _menu.AddDisabledItem(pasteIDGUI);
            } else {

                _menu.AddItem(pasteIDGUI, false, () => {

                    if (EnhancedObjectID.TryParse(GUIUtility.systemCopyBuffer, out EnhancedObjectID _id) && GetMemberValue(_idProperty).SetValue(_idProperty, _id)) {

                        // Clear cache.
                        references.Clear();
                        EditorUtility.SetDirty(_idProperty.serializedObject.targetObject);
                    }
                });
            }
        }
        #endregion
    }
}
