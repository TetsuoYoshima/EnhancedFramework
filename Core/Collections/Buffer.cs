// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedFramework.Core {
    // ===== Base ===== \\

    /// <summary>
    /// Buffer used to push and pop values according to a priority system.
    /// <para/>
    /// Automatically sort its content on modification, and use the first one as the active value.
    /// </summary>
    /// <typeparam name="T">Buffer value type.</typeparam>
    /// <typeparam name="U">Buffer key type.</typeparam>
    /// <typeparam name="V">Buffer priority value type.</typeparam>
    [Serializable]
    public abstract class Buffer<T, U, V> : PairCollection<U, V> {
        #region Global Members
        [SerializeField, HideInInspector] protected T defaultValue = default;
        [SerializeField, HideInInspector] protected T value = default;

        /// <summary>
        /// If true, ensures that if two elements are equal when sorting, their order will be preserved.
        /// </summary>
        [SerializeField, HideInInspector] protected bool performStableSorting = true;

        /// <summary>
        /// Called whenever this buffer active value is changed (new value as first, previous one as second).
        /// </summary>
        public Action<T, T> OnValueChanged = null;

        // -----------------------

        /// <summary>
        /// The default value of this buffer.
        /// </summary>
        public T DefaultValue {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        /// <summary>
        /// The current active value of the buffer.
        /// </summary>
        public T Value {
            get { return value; }
        }

        /// <summary>
        /// The full current active value of the buffer.
        /// </summary>
        public Pair<U, V> FullValue {
            get { return GetFullValue(); }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="Buffer{T, U, V}(int, T, bool, Action{T, T})"/>
        public Buffer(T _defaultValue, bool _stableSort, Action<T, T> _onValueChanged) {
            defaultValue = _defaultValue;
            value = _defaultValue;
            performStableSorting = _stableSort;

            OnValueChanged = _onValueChanged;
        }

        /// <summary>
        /// <inheritdoc cref="Buffer{T, U, V}"/>
        /// </summary>
        /// <param name="_capacity">The initial capacity of this buffer.</param>
        /// <param name="_defaultValue"><inheritdoc cref="defaultValue" path="/summary"/></param>
        /// <param name="_stableSort"><inheritdoc cref="performStableSorting" path="/summary"/></param>
        /// <param name="_onValueChanged"><inheritdoc cref="OnValueChanged" path="/summary"/></param>
        public Buffer(int _capacity, T _defaultValue, bool _stableSort, Action<T, T> _onValueChanged) : this(_defaultValue, _stableSort, _onValueChanged) {
            collection = new List<Pair<U, V>>(_capacity);
        }
        #endregion

        #region Buffer
        private T RefreshBuffer() {
            Pair<U, V> _value = GetDefaultValue();

            if (Count != 0) {
                // Sort the buffer by priority.
                Sort();
                _value = First();
            }

            T _newValue = GetValue(_value);

            if (!EqualityUtility.Equals(value, _newValue)) {
                OnValueChanged?.Invoke(_newValue, value);
                value = _newValue;
            }

            return value;
        }

        private Pair<U, V> GetFullValue() {
            return (Count == 0)
                 ? GetDefaultValue()
                 : First();
        }

        // -----------------------

        protected abstract Pair<U, V> GetDefaultValue();

        protected abstract int Compare(Pair<U, V> _first, Pair<U, V> _second);

        protected abstract T GetValue(Pair<U, V> _pair);
        #endregion

        #region Collection
        /// <summary>
        /// Pushes a key and its associated value in this buffer.
        /// <br/> Creates a new pair if not matching key could be found, or set its value.
        /// </summary>
        /// <param name="_key">The key of the pair to push.</param>
        /// <param name="_value">The value associated with the key to push.</param>
        /// <returns>The buffer current active value.</returns>
        public T Push(U _key, V _value) {
            Set(_key, _value);
            return Value;
        }

        /// <summary>
        /// Pops a key and its associated value from this buffer.
        /// </summary>
        /// <param name="_key">The key of the pair to pop from this buffer.</param>
        /// <returns>The buffer current active value.</returns>
        public T Pop(U _key) {
            Remove(_key);
            return Value;
        }

        // -----------------------

        public override int Set(U _key, V _value) {

            Pair<U, V> _pair = new Pair<U, V>(_key, _value);
            int _index = IndexOfKey(_key);

            // Insert element at index 0 instead of adding it at the end.
            if (_index != -1) {
                collection[_index] = _pair;
            } else {
                collection.Insert(0, _pair);
            }

            RefreshBuffer();
            return IndexOfKey(_key);
        }

        public override void CopyTo(Pair<U, V>[] _array, int _arrayIndex) {
            base.CopyTo(_array, _arrayIndex);
            RefreshBuffer();
        }

        public override void Shift(int _index, int _shiftIndex) {
            base.Shift(_index, _shiftIndex);
            RefreshBuffer();
        }

        public override void RemoveAt(int _index) {
            base.RemoveAt(_index);
            RefreshBuffer();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Resets this buffer.
        /// </summary>
        /// <returns>The buffer current active value.</returns>
        public T Reset() {
            Clear();
            return Value;
        }

        // -----------------------

        public override void Clear() {
            base.Clear();
            RefreshBuffer();
        }

        public override void Sort() {

            // Use a helper list to ensure a stable sorting.
            if (performStableSorting) {
                List<Pair<U, V>> _sortHelper = new List<Pair<U, V>>(collection);

                collection.Sort((a, b) => {

                    int _compare = Compare(a, b);

                    if (_compare == 0) {

                        int _aIndex = _sortHelper.IndexOf(a);
                        int _bIndex = _sortHelper.IndexOf(b);

                        if (_aIndex == -1) {

                            _compare = -1;

                        } else if (_bIndex == -1) {

                            _compare = 1;

                        } else {

                            _compare = _aIndex.CompareTo(_bIndex);
                        }
                    }

                    return _compare;
                });
            } else {
                // Unstable sorting.
                collection.Sort(Compare);
            }
        }
        #endregion
    }

    // ===== Derived ===== \\

    /// <summary>
    /// <inheritdoc cref="Buffer{T, U, V}"/>
    /// <para/>
    /// Uses the value as the key.
    /// </summary>
    /// <typeparam name="T">Key / value type.</typeparam>
    [Serializable]
    public sealed class BufferR<T> : Buffer<T, T, int> {
        #region Constructor
        /// <inheritdoc cref="Buffer{T, U, V}.Buffer(T, bool, Action{T, T})"/>
        public BufferR(T _defaultValue = default, bool _stableSort = true, Action<T, T> _onValueChanged = null) :
               base(_defaultValue, _stableSort, _onValueChanged) { }

        /// <inheritdoc cref="Buffer{T, U, V}.Buffer(int, T, bool, Action{T, T})"/>
        public BufferR(int _capacity, T _defaultValue = default, bool _stableSort = true, Action<T, T> _onValueChanged = null) :
               base(_capacity, _defaultValue, _stableSort, _onValueChanged) { }
        #endregion

        #region Buffer
        protected override Pair<T, int> GetDefaultValue() {
            return new Pair<T, int>(defaultValue, int.MinValue);
        }

        protected override int Compare(Pair<T, int> _first, Pair<T, int> _second) {
            return -_first.Second.CompareTo(_second.Second);
        }

        protected override T GetValue(Pair<T, int> _pair) {
            return _pair.First;
        }
        #endregion
    }

    /// <summary>
    /// <inheritdoc cref="Buffer{T, U, V}"/>
    /// <para/>
    /// Uses the priority as the key.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    [Serializable]
    public sealed class BufferI<T> : Buffer<T, int, T> {
        #region Constructor
        /// <inheritdoc cref="Buffer{T, U, V}.Buffer(T, bool, Action{T, T})"/>
        public BufferI(T _defaultValue = default, bool _stableSort = true, Action<T, T> _onValueChanged = null) : base(_defaultValue, _stableSort, _onValueChanged) { }

        /// <inheritdoc cref="Buffer{T, U, V}.Buffer(int, T, bool, Action{T, T})"/>
        public BufferI(int _capacity, T _defaultValue = default, bool _stableSort = true, Action<T, T> _onValueChanged = null) :
               base(_capacity, _defaultValue, _stableSort, _onValueChanged) { }
        #endregion

        #region Buffer
        protected override Pair<int, T> GetDefaultValue() {
            return new Pair<int, T>(int.MinValue, defaultValue);
        }

        protected override int Compare(Pair<int, T> _first, Pair<int, T> _second) {
            return -_first.First.CompareTo(_second.First);
        }

        protected override T GetValue(Pair<int, T> _pair) {
            return _pair.Second;
        }
        #endregion
    }

    /// <summary>
    /// <inheritdoc cref="Buffer{T, U, V}"/>
    /// <para/>
    /// Let the user use a different object for the value, the key and the priority.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    [Serializable]
    public sealed class BufferV<T> : Buffer<T, int, Pair<T, int>> {
        #region Constructor
        /// <inheritdoc cref="Buffer{T, U, V}.Buffer(T, bool, Action{T, T})"/>
        public BufferV(T _defaultValue = default, bool _stableSort = true, Action<T, T> _onValueChanged = null) :
               base(_defaultValue, _stableSort, _onValueChanged) { }

        /// <inheritdoc cref="Buffer{T, U, V}.Buffer(int, T, bool, Action{T, T})"/>
        public BufferV(int _capacity, T _defaultValue = default, bool _stableSort = true, Action<T, T> _onValueChanged = null) : 
               base(_capacity, _defaultValue, _stableSort, _onValueChanged) { }
        #endregion

        #region Buffer
        protected override Pair<int, Pair<T, int>> GetDefaultValue() {
            return new Pair<int, Pair<T, int>>(int.MinValue, new Pair<T, int>(defaultValue, int.MinValue));
        }

        protected override int Compare(Pair<int, Pair<T, int>> _first, Pair<int, Pair<T, int>> _second) {
            return -_first.Second.Second.CompareTo(_second.Second.Second);
        }

        protected override T GetValue(Pair<int, Pair<T, int>> _pair) {
            return _pair.Second.First;
        }
        #endregion
    }
}
