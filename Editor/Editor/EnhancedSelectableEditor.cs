// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedFramework.UI;
using UnityEditor;
using UnityEditor.UI;

namespace EnhancedFramework.Editor {
    /// <summary>
    /// Custom <see cref="EnhancedSelectable"/> editor.
    /// </summary>
    [CustomEditor(typeof(EnhancedSelectable), true), CanEditMultipleObjects]
    public sealed class EnhancedSelectableEditor : SelectableEditor {
        #region Editor GUI
        public override void OnInspectorGUI() {
            SerializedObject _serializedObject = serializedObject;
            _serializedObject.Update();

            EditorGUILayout.PropertyField(_serializedObject.FindProperty("AutoSelectOnEnabled"));
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("DeselectOnPointerExit"));
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("Effects"));

            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();

            EditorGUILayout.Space(5f);
            base.OnInspectorGUI();

            SerializedProperty _last = _serializedObject.FindProperty("group");

            while (_last.NextVisible(false)) {
                EditorGUILayout.PropertyField(_last);
            }

            _serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}
