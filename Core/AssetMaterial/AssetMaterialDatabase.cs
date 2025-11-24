// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Wrapper for a <see cref="AssetMaterialDatabase{T}"/> asset group.
    /// </summary>
    /// <typeparam name="T">This group <see cref="T"/> asset tyoe.</typeparam>
    [Serializable]
    #pragma warning disable
    public sealed class AssetMaterialGroup<T> where T : Object {
        #region Global Members
        [Tooltip("Name of this group (editor only)")]
        [SerializeField, Delayed] private string name = "New Group";

        [Tooltip("Asset of this group")]
        [SerializeField, Enhanced, Required] public T Asset = null;

        [Space(10f)]

        [Tooltip("All materials registered in this group, and associated with this asset")]
        [SerializeField] public BlockList<Material> Materials = new BlockList<Material>();

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="AssetMaterialGroup(T, IList{Material})"/>
        public AssetMaterialGroup() {
            Materials = new BlockList<Material>(true, true, false);
        }

        /// <inheritdoc cref="AssetMaterialGroup(T, IList{Material})"/>
        public AssetMaterialGroup(T _asset, Material _material) : this() {
            Asset = _asset;
            Materials.List.Add(_material);
        }

        /// <param name="_asset">Asset wrapped in this group.</param>
        /// <param name="_materials">All materials associated with this asset.</param>
        /// <inheritdoc cref="AssetMaterialDatabase{T}"/>
        public AssetMaterialGroup(T _asset, IList<Material> _materials) : this() {
            Asset = _asset;
            Materials.List.AddRange(_materials);
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Get the asset from this group if it is associated with a specific <see cref="UnityEngine.Material"/>.
        /// </summary>
        /// <param name="_material"><see cref="UnityEngine.Material"/> to check.</param>
        /// <param name="_asset">Asset associated with this material from this group (null if none).</param>
        /// <returns>True if this material is registered in this group, false otherwise.</returns>
        public bool GetAsset(Material _material, out T _asset) {

            if (Materials.List.Contains(_material)) {
                _asset = Asset;
                return true;
            }

            _asset = null;
            return false;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Registers a specific <see cref="UnityEngine.Material"/> in this group.
        /// </summary>
        /// <param name="_material"><see cref="UnityEngine.Material"/> to register.</param>
        public void RegisterMaterial(Material _material) {

            ref List<Material> _span = ref Materials.List;

            if (!_span.Contains(_material)) {
                _span.Add(_material);
            }
        }

        /// <summary>
        /// Unregisters a specific <see cref="UnityEngine.Material"/> from this group.
        /// </summary>
        /// <param name="_material"><see cref="UnityEngine.Material"/> to unregister.</param>
        /// <returns>True if the material could be successfully unregistered, false otherwise.</returns>
        public bool UnregisterMaterial(Material _material) {

            ref List<Material> _span = ref Materials.List;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                if (_span[i] == _material) {

                    _span.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
        #endregion
    }

    // ===== Asset Database ===== \\

    /// <summary>
    /// Base class for a <see cref="Material"/>-related asset database.
    /// <br/> Used to associate a specific <see cref="T"/> value with a <see cref="Material"/> (like footstep sounds).
    /// </summary>
    public abstract class AssetMaterialDatabase<T> : EnhancedScriptableObject where T : Object {
        #region Global Members
        public const string MenuPrefix  = "MTD_";
        public const string MenuPath    = FrameworkUtility.MenuPath + "Asset Material Database/";
        public const int MenuOrder      = FrameworkUtility.MenuOrder;

        [Section("Asset Material Database"), PropertyOrder(0)]

        [Tooltip("Default asset of this database; returned when no asset is registered for a material")]
        [SerializeField, Enhanced, Required] protected T DefaultAsset = null;

        [Space(10f), PropertyOrder(10)]

        [Tooltip("All material groups in this database")]
        [SerializeField] protected AssetMaterialGroup<T>[] groups = new AssetMaterialGroup<T>[] { new AssetMaterialGroup<T>() };

        [Space(10f)]

        [Tooltip("All materials associated with multiple assets")]
        [ShowIf(nameof(HasDuplicateMaterial), order = -1), HelpBox("Duplicate Material(s) in this database", MessageType.Warning)]
        [SerializeField, Enhanced, ReadOnly] private Set<Material> duplicateMaterials = new Set<Material>();

        [Space(10f)]

        [Tooltip("All materials not assigned to any asset")]
        [ShowIf(nameof(HasUnassignedMaterial), order = -1), HelpBox("Unassigned Material(s) in this database", MessageType.Error)]
        [SerializeField, Enhanced, ReadOnly] private Set<Material> unassignedMaterials = new Set<Material>();

        // -----------------------

        /// <summary>
        /// Indicates if this database has any registered group.
        /// </summary>
        public bool HasAnyGroup {
            get { return groups.Length != 0; }
        }

        /// <summary>
        /// Indicates if this database has any unassigned material.
        /// </summary>
        public bool HasUnassignedMaterial {
            get { return unassignedMaterials.Count != 0; }
        }

        /// <summary>
        /// Indicates if any material is registered multiple times in the database.
        /// </summary>
        public bool HasDuplicateMaterial {
            get { return duplicateMaterials.Count != 0; }
        }
        #endregion

        #region Enhanced Behaviour
        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            ref List<Material> _span = ref unassignedMaterials.collection;
            int _count = _span.Count;

            if (_count == 0) {
                return;
            }

            // Update unassigned materials.
            foreach (AssetMaterialGroup<T> _asset in groups) {

                for (int i = _count; i-- > 0;) {

                    // If material is now assigned, unregister it.
                    if (_asset.GetAsset(_span[i], out _)) {
                        _span.RemoveAt(i);

                        if (--_count == 0) {
                            return;
                        }
                    }
                }
            }

            this.LogWarningMessage($"{_count} unassigned Material(s) in this database");
        }
        #endif
        #endregion

        #region Utility
        // -------------------------------------------
        // Get
        // -------------------------------------------

        /// <summary>
        /// Get the <see cref="T"/> asset associated with a specific material from this database.
        /// </summary>
        /// <param name="_material"><see cref="Material"/> to get the associated asset.</param>
        /// <returns>Asset associated with this material (default if none).</returns>
        public T GetAsset(Material _material) {

            ref AssetMaterialGroup<T>[] _span = ref groups;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                if (_span[i].GetAsset(_material, out T _asset)) {
                    return _asset;
                }
            }

            this.LogWarningMessage($"Material \'{_material.name}\' has no registered {typeof(T).Name} asset", _material);
            unassignedMaterials.Add(_material);

            return DefaultAsset;
        }

        /// <summary>
        /// Get the <see cref="T"/> asset associated with a specific <see cref="RaycastHit"/> <see cref="Material"/> from this database.
        /// </summary>
        /// <param name="_hit"><see cref="RaycastHit"/> to get the associated hit material asset.</param>
        /// <returns>Asset associated with this <see cref="RaycastHit"/> material (default if none).</returns>
        public T GetAsset(RaycastHit _hit) {

            if (_hit.GetSharedMaterial(out Material _material)) {
                return GetAsset(_material);
            }

            this.LogWarningMessage($"Material could not be found from hit");
            return DefaultAsset;
        }

        // -------------------------------------------
        // Other
        // -------------------------------------------

        /// <summary>
        /// Assigns a specific <see cref="T"/> asset to a <see cref="Material"/>.
        /// </summary>
        /// <param name="_material"><see cref="Material"/> to assign an asset to.</param>
        /// <param name="_asset">Asset to associate with the material.</param>
        public void AssignAsset(Material _material, T _asset) {

            ref AssetMaterialGroup<T>[] _span = ref groups;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                AssetMaterialGroup<T> _group = _span[i];

                if (_group.GetAsset(_material, out T _assignedAsset)) {

                    // Already assigned.
                    if (_assignedAsset == _asset) {
                        return;
                    }

                    // Remove from group.
                    _group.Materials.List.Remove(_material);
                }
            }

            ArrayUtility.Add(ref _span, new AssetMaterialGroup<T>(_asset, _material));
        }

        /// <summary>
        /// Creates a new group in this database.
        /// </summary>
        [Button(nameof(HasAnyGroup), ConditionType.False, SuperColor.Green, IsDrawnOnTop = false)]
        public void CreateGroup() {
            ArrayUtility.Add(ref groups, new AssetMaterialGroup<T>());
        }

        /// <summary>
        /// Checks all duplicate materials registered in this asset.
        /// </summary>
        [Button(SuperColor.DarkOrange, IsDrawnOnTop = false)]
        public void CheckDuplicateMaterials() {

            ref AssetMaterialGroup<T>[] _span = ref groups;
            int _count = _span.Length;

            duplicateMaterials.Clear();

            for (int i = _count; i-- > 1;) {

                ref List<Material> _materials = ref _span[i].Materials.List;
                int _materialCount = _materials.Count;

                for (int j = 0; j < _materialCount; j++) {

                    Material _material = _materials[j];
                    for (int k = 0; k < i; k++) {

                        if (_span[k].GetAsset(_material, out _)) {
                            duplicateMaterials.Add(_material);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
