// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Contrains multiple collection-related buffer and utility members.
    /// </summary>
    public static class BufferUtility {
        #region Content
        public static readonly List<EnhancedBehaviour>  BaseBehaviourList   = new List<EnhancedBehaviour>();

        public static readonly List<SpriteRenderer>     SpriteRendererList  = new List<SpriteRenderer>();
        public static readonly List<TrailRenderer>      TrailRendererList   = new List<TrailRenderer>();
        public static readonly List<MeshRenderer>       MeshRendererList    = new List<MeshRenderer>();
        public static readonly List<VisualEffect>       VisualEffectList    = new List<VisualEffect>();
        public static readonly List<MeshFilter>         MeshFilterList      = new List<MeshFilter>();
        public static readonly List<Transform>          TransformList       = new List<Transform>();
        public static readonly List<Animator>           AnimatorList        = new List<Animator>();
        public static readonly List<Collider>           ColliderList        = new List<Collider>();
        public static readonly List<Material>           MaterialList        = new List<Material>();
        public static readonly List<Renderer>           RendererList        = new List<Renderer>();

        public static readonly List<RaycastHit>         RaycastList         = new List<RaycastHit>();
        public static readonly List<Vector3>            Vector3List         = new List<Vector3>();
        public static readonly List<Vector2>            Vector2List         = new List<Vector2>();
        public static readonly List<string>             StringBuffer        = new List<string>();
        public static readonly List<float>              FloatList           = new List<float>();

        public static Collider[]                        ColliderArray       = new Collider[32];
        public static RaycastHit[]                      RaycastArray        = new RaycastHit[16];
        public static int[]                             IntArray            = Array.Empty<int>();

        public static CapsuleCollider                   CaspuleCollider     = null;
        public static SphereCollider                    SphereCollider      = null;
        public static BoxCollider                       BoxCollider         = null;

        // -----------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize() {
            // Create collider(s).
            // 
            // -- Capsule

            GameObject _capsule = new GameObject("BUF_CapsuleCollider");
            Transform _capsuleTransform = _capsule.transform;

            _capsuleTransform.SetParent(GameManager.Instance.transform);
            _capsuleTransform.ResetLocal();

            CaspuleCollider = _capsule.AddComponent<CapsuleCollider>();
            _capsule.gameObject.SetActive(false);

            // -- Sphere

            GameObject _sphere = new GameObject("BUF_SphereCollider");
            Transform _sphereTransform = _sphere.transform;

            _sphereTransform.SetParent(GameManager.Instance.transform);
            _sphereTransform.ResetLocal();

            SphereCollider = _sphere.AddComponent<SphereCollider>();
            _sphere.gameObject.SetActive(false);

            // -- Box

            GameObject _box = new GameObject("BUF_BoxCollider");
            Transform _boxTransform = _box.transform;

            _boxTransform.SetParent(GameManager.Instance.transform);
            _boxTransform.ResetLocal();

            BoxCollider = _box.AddComponent<BoxCollider>();
            _box.gameObject.SetActive(false);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Removes all pending elements from a given collection.
        /// </summary>
        /// <typeparam name="T">List element type.</typeparam>
        /// <param name="_collection">Collection to remove pending elements from.</param>
        /// <param name="_length">Length of this collection to remove pending elements from.</param>
        /// <param name="_pending">All pending elements to remove from the collection.</param>
        /// <returns>New length of the modified collection.</returns>
        public static int RemovePending<T>(List<T> _collection, int _length, List<T> _pending) {
            int _pendingCount = _pending.Count;

            // Nothing to remove.
            if (_pendingCount == 0)
                return _length;

            int _removedCount = 0;

            // Remove all pending items.
            for (int i = _length; i-- > 0;) {
                // Ignore if not in pending.
                if (!_pending.Remove(_collection[i]))
                    continue;

                // Last element, so simply clear.
                if (_length == 1) {
                    _collection.Clear();
                    _removedCount = 0;
                    _length = 0;

                    break;
                }

                // Swap.
                _collection[i] = _collection[--_length];
                _removedCount++;

                // All items removed.
                if (--_pendingCount == 0)
                    break;
            }

            // Trim end.
            if (_removedCount != 0) {
                _collection.RemoveRange(_length, _removedCount);
            }

            // Should not be necessary, but clear pending.
            if (_pendingCount != 0) {
                _pending.Clear();
            }

            return _length;
        }
        #endregion
    }
}
