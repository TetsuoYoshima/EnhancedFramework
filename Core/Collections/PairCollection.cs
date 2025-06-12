// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="Dictionary{TKey, TValue}"/>-similar serializable collection.
    /// </summary>
    /// <typeparam name="T">The key type of this collection.</typeparam>
    /// <typeparam name="U">The value type of this collection.</typeparam>
    [Serializable]
    public class PairCollection<T, U> : Set<Pair<T, U>> {
        #region Constructor
        /// <inheritdoc cref="PairCollection{T, U}()"/>
        public PairCollection() : base() { }

        /// <inheritdoc cref="PairCollection{T, U}(int)"/>
        public PairCollection(int _capacity) : base(_capacity) { }
        #endregion

        #region Collection
        /// <summary>
        /// Sets a new pair entry in this collection.
        /// <br/> Adds a new entry if not yet contained in the collection, or modify its value.
        /// </summary>
        /// <param name="_key">The key of the pair to set.</param>
        /// <param name="_value">The value of the pair to set.</param>
        /// <returns>The index of the pair in this collection.</returns>
        public virtual int Set(T _key, U _value) {
            int _index = IndexOfKey(_key);

            if (_index != -1) {
                collection[_index] = new Pair<T, U>(_key, _value);
                return _index;
            }

            collection.Add(new Pair<T, U>(_key, _value));
            return Count - 1; 
        }

        /// <summary>
        /// Adds a new pair entry in this collection.
        /// <br/> Modify its value if an entry with the same key is found.
        /// </summary>
        /// <inheritdoc cref="Set(T, U)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(T _key, U _value) {
            return Set(_key, _value);
        }

        /// <summary>
        /// Adds a new pair entry in this collection.
        /// <br/> This method is faster but will add a duplicate if the key is already in the collection.
        /// </summary>
        /// <inheritdoc cref="Set(T, U)"/>
        public int AddUnsafe(T _key, U _value) {
            collection.Add(new Pair<T, U>(_key, _value));
            return Count - 1;
        }

        /// <summary>
        /// Removes a specific key and its associated value from this collection.
        /// </summary>
        /// <param name="_key">The key to remove.</param>
        /// <returns>True if an entry with this key could be found and removed, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T _key) {
            if (ContainsKey(_key, out int _index)) {
                RemoveAt(_index);
                return true;
            }

            return false;
        }

        // -----------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Add(Pair<T, U> _pair) {
            Set(_pair.First, _pair.Second);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Remove(Pair<T, U> _pair) {
            return Remove(_pair.First);
        }
        #endregion

        #region Container
        /// <summary>
        /// Get the index of a specific key from this collection.
        /// </summary>
        /// <param name="_key">The key to get the associated index.</param>
        /// <returns>-1 if the key could not be found in this collection, or its index otheriwse.</returns>
        public int IndexOfKey(T _key) {
            ref List<Pair<T, U>> _span = ref collection;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                if (EqualityUtility.Equals(_span[i].First, _key)) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get the index of a specific value from this collection.
        /// </summary>
        /// <param name="_key">The value to get the associated index.</param>
        /// <returns>-1 if the value could not be found in this collection, or its index otheriwse.</returns>
        public int IndexOfValue(U _value) {
            ref List<Pair<T, U>> _span = ref collection;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                if (EqualityUtility.Equals(_span[i].Second, _value)) {
                    return i;
                }
            }

            return -1;
        }

        /// <inheritdoc cref="ContainsKey(T, out int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(T _key) {
            return ContainsKey(_key, out _);
        }

        /// <summary>
        /// Get if a specific key is contained in this collection.
        /// </summary>
        /// <param name="_key">The key to check.</param>
        /// <param name="_index">The index of the key in this collection (-1 if not found).</param>
        /// <returns>True if the key is contained in this collection, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(T _key, out int _index) {
            _index = IndexOfKey(_key);
            return _index != -1;
        }

        /// <inheritdoc cref="ContainsValue(U, out int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsValue(U _value) {
            return ContainsValue(_value, out _);
        }

        /// <summary>
        /// Get if a specific value is contained in this collection.
        /// </summary>
        /// <param name="_value">The value to check.</param>
        /// <param name="_index">The index of the value in this collection (-1 if not found).</param>
        /// <returns>True if the value is contained in this collection, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsValue(U _value, out int _index) {
            _index = IndexOfValue(_value);
            return _index != -1;
        }

        /// <summary>
        /// Tries to get the value associated with a specific key from this collection.
        /// </summary>
        /// <param name="_key">The key to get the associated value.</param>
        /// <param name="_value">The value associated with the key.</param>
        /// <returns>True if the key and its associated value could be found in this collection, false otherwise.</returns>
        public bool TryGetValue(T _key, out U _value) {
            if (ContainsKey(_key, out int _index)) {
                _value = collection[_index].Second;
                return true;
            }

            _value = default;
            return false;
        }

        /// <summary>
        /// Get the key of the collection at a specific index.
        /// </summary>
        /// <param name="_index">Index to get the element at.</param>
        /// <returns>The key of the element at the given index.</returns>
        public T GetKeyAt(int _index) {
            return collection[_index].First;
        }

        /// <summary>
        /// Get the value of the collection at a specific index.
        /// </summary>
        /// <param name="_index">Index to get the element at.</param>
        /// <returns>The value of the element at the given index.</returns>
        public U GetValueAt(int _index) {
            return collection[_index].Second;
        }

        /// <summary>
        /// Set the key of this collection at a specific index.
        /// </summary>
        /// <param name="_index">Index to set the key at.</param>
        /// <param name="_key">Key to set.</param>
        public void SetKey(int _index, T _key) {
            collection[_index] = new Pair<T, U>(_key, collection[_index].Second);
        }

        /// <summary>
        /// Set the value of this collection at a specific index.
        /// </summary>
        /// <param name="_index">Index to set the value at.</param>
        /// <param name="_value">Value to set.</param>
        public void SetValue(int _index, U _value) {
            collection[_index] = new Pair<T, U>(collection[_index].First, _value);
        }

        // -----------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int IndexOf(Pair<T, U> _element) {
            return IndexOfKey(_element.First);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Contains(Pair<T, U> _element) {
            return ContainsKey(_element.First);
        }
        #endregion
    }
}
