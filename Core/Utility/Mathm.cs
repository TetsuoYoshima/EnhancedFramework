// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

using Random = UnityEngine.Random;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Contains multiple useful math utility methods.
    /// </summary>
    public static class Mathm {
        #region Mathematic
        // -------------------------------------------
        // Range
        // -------------------------------------------

        /// <inheritdoc cref="IsInRange(float, float, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(int _value, int _min, int _max) {
            return (_value >= _min) && (_value <= _max);
        }

        /// <summary>
        /// Get if a specific value is within a given range.
        /// </summary>
        /// <param name="_value">The value to evaluate.</param>
        /// <param name="_min">Minimum allowed value.</param>
        /// <param name="_max">Maximum allowed value.</param>
        /// <returns>True if the specified value is within the given range, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(float _value, float _min, float _max) {
            return (_value >= _min) && (_value <= _max);
        }

        /// <inheritdoc cref="IsInRangeExclusive(float, float, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeExclusive(int _value, int _min, int _max) {
            return (_value > _min) && (_value < _max);
        }

        /// <summary>
        /// Get if a specific value is within a given range (min and max exclusive).
        /// </summary>
        /// <inheritdoc cref="IsInRange(float, float, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeExclusive(float _value, float _min, float _max) {
            return (_value > _min) && (_value < _max);
        }

        // -------------------------------------------
        // Loop
        // -------------------------------------------

        /// <inheritdoc cref="LoopIncrement(float, float, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LoopIncrement(int _value, int _limit, int _increase) {
            return (int)LoopIncrement((float)_value, _limit, _increase);
        }

        /// <summary>
        /// Increments a given value looping between 0 and a given limit (exclusive).
        /// <br/> When the value reaches the limits, it comes back to zero, and vice-versa.
        /// </summary>
        /// <param name="_value">Value to increase.</param>
        /// <param name="_limit"></param>
        /// <param name="_increase">Increase value.</param>
        /// <returns>Clamped loop value.</returns>
        public static float LoopIncrement(float _value, float _limit, float _increase) {
            _value += _increase;

            while (_value < 0f) {
                _value += _limit;
            }

            while (_value >= _limit) {
                _value -= _limit;
            }

            return _value;
        }

        // -------------------------------------------
        // Clamp
        // -------------------------------------------

        /// <summary>
        /// Clamps the given value between a minimum and a maximum.
        /// </summary>
        /// <param name="_value">The value to restrict inside the range of the min and max values.</param>
        /// <param name="_min">The minimum value to compare against.</param>
        /// <param name="_max">The maximum value to compare against.</param>
        /// <returns>The result between the min and max values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double _value, double _min, double _max) {
            return (_value < _min)
                 ? _min : ((_value > _max)
                        ? _max : _value);
        }

        /// <summary>
        /// Clamps a given angle value, so that is remains clamped betweed -360 and 360.
        /// </summary>
        /// <param name="_angle">Angle value to clamp.</param>
        /// <returns>Calmped angle value.</returns>
        public static float ClampAngle(float _angle) {

            const float MaxAngle = 360f;
            float _value = Mathf.Abs(_angle);

            while (_value > MaxAngle) {
                _value -= MaxAngle;
            }

            return _angle * Mathf.Abs(_angle);
        }
        #endregion

        #region Decimal
        /// <summary>
        /// Rounds a given <see cref="float"/> value to a given decimals place.
        /// </summary>
        /// <param name="_value">The value to round.</param>
        /// <param name="_decimal">The final amount of decimal.</param>
        /// <returns>The rounded <see cref="float"/> value.</returns>
        public static float RoundToDecimal(float _value, int _decimal) {
            if (_decimal == 0) {
                return Mathf.Round(_value);
            }

            float _factor = Mathf.Pow(10f, _decimal);
            return Mathf.Round(_value * _factor) / _factor;
        }

        /// <summary>
        /// Floors a given <see cref="float"/> value to a given decimals place.
        /// </summary>
        /// <param name="_value">The value to floor (nearest lower value).</param>
        /// <param name="_decimal">The final amount of decimal.</param>
        /// <returns>The floored <see cref="float"/> value.</returns>
        public static float FloorToDecimal(float _value, int _decimal) {
            if (_decimal == 0) {
                return Mathf.Floor(_value);
            }

            float _factor = Mathf.Pow(10f, _decimal);
            return Mathf.Floor(_value * _factor) / _factor;
        }

        /// <summary>
        /// Ceils a given <see cref="float"/> value to a given decimals place.
        /// </summary>
        /// <param name="_value">The value to round (nearest greater value).</param>
        /// <param name="_decimal">The final amount of decimal.</param>
        /// <returns>The ceiled <see cref="float"/> value.</returns>
        public static float CeilToDecimal(float _value, int _decimal) {
            if (_decimal == 0) {
                return Mathf.Ceil(_value);
            }

            float _factor = Mathf.Pow(10f, _decimal);
            return Mathf.Ceil(_value * _factor) / _factor;
        }
        #endregion

        #region Sign
        /// <inheritdoc cref="BooleanExtensions.Sign(bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(bool _boolean) {
            return _boolean.Sign();
        }

        /// <summary>
        /// Get the sign of a specific integer.
        /// </summary>
        /// <param name="_value">Integer value to get sign from.</param>
        /// <returns>-1 if smaller than 0, 1 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(int _value) {
            return (_value < 0) ? -1 : 1;
        }

        /// <summary>
        /// Get the sign of a specific float.
        /// </summary>
        /// <param name="_value">Float value to get sign from.</param>
        /// <returns>-1 if smaller than 0, 1 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(float _value) {
            return (_value < 0f) ? -1 : 1;
        }

        /// <summary>
        /// Get if two floats have a different sign.
        /// </summary>
        /// <param name="a">First float to compare.</param>
        /// <param name="b">Second float to compare.</param>
        /// <returns>True if the floats have different signs, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HaveDifferentSign(float a, float b) {
            return Sign(a) != Sign(b);
        }

        /// <summary>
        /// Get if two floats have a different sign and are not null.
        /// </summary>
        /// <param name="a">First float to compare.</param>
        /// <param name="b">Second float to compare.</param>
        /// <returns>True if the floats have different signs and are not equal to 0, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HaveDifferentSignAndNotNull(float a, float b) {
            return (a != 0f) && (b != 0f) && HaveDifferentSign(a, b);
        }
        #endregion

        #region Equality
        private const float FloatPrecision = .001f;

        // -----------------------

        /// <summary>
        /// Get if three integers are all equal.
        /// </summary>
        /// <param name="a">First integer to compare.</param>
        /// <param name="b">Second integer to compare.</param>
        /// <param name="c">Third integers to compare.</param>
        /// <returns>True if all integers are equal, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreEquals(int a, int b, int c) {
            return (a == b) && (b == c);
        }

        /// <summary>
        /// Get if three floats are all equal.
        /// </summary>
        /// <param name="a">First float to compare.</param>
        /// <param name="b">Second float to compare.</param>
        /// <param name="c">Third float to compare.</param>
        /// <returns>True if all floats are equal, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreEquals(float a, float b, float c) {
            return (Math.Abs(a - b) < FloatPrecision) && (Math.Abs(b - c) < FloatPrecision);
        }

        /// <inheritdoc cref="VectorExtensions.IsNull(Vector2)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsVectorNull(Vector2 _value) {
            return _value.IsNull();
        }

        /// <inheritdoc cref="VectorExtensions.IsNull(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsVectorNull(Vector3 _value) {
            return _value.IsNull();
        }

        /// <summary>
        /// Get if a specific <see cref="float"/> values approximately equals 0.
        /// </summary>
        /// <param name="_value">Value to check.</param>
        /// <returns>True if the given value is approximately equal to 0, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyZero(float _value) {
            return Mathf.Approximately(_value, 0f);
        }
        #endregion

        #region Geometry
        /// <inheritdoc cref="VectorExtensions.PerpendicularSurface(Vector3, Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ParallelSurface(Vector3 _direction, Vector3 _normal) {
            return _direction.PerpendicularSurface(_normal);
        }

        /// <summary>
        /// Normalizes a specific value on a given range.
        /// <br/> For exemple, a value of 3 on a range of (1, 4) has a 0,666 normalized value.
        /// </summary>
        /// <param name="_value">Value to normalize.</param>
        /// <param name="_from">Range from value.</param>
        /// <param name="_to">Range to value.</param>
        /// <returns>This normalized value.</returns>
        public static float NormalizedValue(float _value, float _from, float _to) {

            float _position = _value - _from;
            float _range    = _to    - _from;

            return (_position == 0f) ? 0f : Mathf.Clamp01(_position / _range);
        }
        #endregion

        #region Rotation
        /// <summary>
        /// Get the angle used to adjust a given forward vector on another vector, snapping to the nearest given angle.
        /// </summary>
        public static float GetAdjustedRotationAngle(Vector3 _from, Vector3 _to, Vector3 _up, float _angle = 90f) {
            float value = Vector3.SignedAngle(_from, _to, _up) % _angle;

            if (Mathf.Abs(value) > (_angle * .5f)) {
                value += -_angle * Mathf.Sign(value);
            }

            return value;
        }
        #endregion

        #region Random
        private const float MinRandomValue    = .001f;
        private const int MaxRandomGeneration = 10;

        private static readonly List<float> randomFloatBuffer = new List<float>();
        private static readonly int[] randomNoRepeatBuffer    = new int[1];

        // -----------------------

        /// <inheritdoc cref="RandomNoRepeat(int, int, IList{int})"/>
        public static int RandomNoRepeat(int _minInclusive, int _maxExclusive, int _ignored) {
            randomNoRepeatBuffer[0] = _ignored;
            return RandomNoRepeat(_minInclusive, _maxExclusive, randomNoRepeatBuffer);
        }

        /// <summary>
        /// Get a random generated number excluding some specific result.
        /// </summary>
        /// <param name="_minInclusive">Minimum random value (inclusive).</param>
        /// <param name="_maxExclusive">Maximum random value (exclusive).</param>
        /// <param name="_ignored">Excluding random result(s).</param>
        /// <returns>Random generated value.</returns>
        public static int RandomNoRepeat(int _minInclusive, int _maxExclusive, IList<int> _ignored) {
            if ((_maxExclusive - _minInclusive) <= 1) {
                return _minInclusive;
            }

            int _count  = MaxRandomGeneration;
            int _random = Random.Range(_minInclusive, _maxExclusive);

            while (_ignored.Contains(_random) && (--_count != 0)) {
                _random = LoopIncrement(_random - _minInclusive, _maxExclusive - _minInclusive, 1) + _minInclusive;
            }

            return _random;
        }

        /// <summary>
        /// Get a random element from a collection using their respective probability.
        /// </summary>
        /// <param name="collection">Collection to get a random element from.</param>
        /// <param name="getProba">Getter of a single element probability (element as parameter).
        /// <br/> Probability must be between 0 (lower) and 1 (higher).</param>
        /// <param name="_ignoreZeroProba">If true, ignore all elements with a probability of 0.</param>
        /// <returns>Index of the random selected element (-1 if none).</returns>
        public static int RandomProbability<T>(IList<T> collection, Func<T, float> getProba, bool _ignoreZeroProba = false) {
            // 1. Store all elements probability.
            // 2. Calculate the sum of all probabilities.
            List<float> _buffer = randomFloatBuffer;
            _buffer.Clear();

            float _sum = 0f;
            int _count = collection.Count;

            for (int i = 0; i < _count; i++) {
                float _probability = getProba(collection[i]);
                _sum += _probability;

                _buffer.Add(_probability);
            }

            return GetRandomProbability(_buffer, _sum, _ignoreZeroProba);
        }

        /// <summary>
        /// Get a random index from a total count, using a given probability system.
        /// </summary>
        /// <param name="count">Total count of elements.</param>
        /// <param name="getProba">Getter of a single element probability (index as parameter).
        /// <br/> Probability must be between 0 (lower) and 1 (higher).</param>
        /// <param name="_ignoreZeroProba">If true, ignore all elements with a probability of 0.</param>
        /// <returns>Selected index (-1 if none).</returns>
        public static int RandomProbability(int count, Func<int, float> getProba, bool _ignoreZeroProba = false) {
            // 1. Store all elements probability.
            // 2. Calculate the sum of all probabilities.
            List<float> _buffer = randomFloatBuffer;
            _buffer.Clear();

            float _sum = 0f;

            for (int i = 0; i < count; i++) {
                float _probability = getProba(i);
                _sum += _probability;

                _buffer.Add(_probability);
            }

            return GetRandomProbability(_buffer, _sum, _ignoreZeroProba);
        }

        /// <summary>
        /// Shuffles a given collection content.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<T> Shuffle<T>(IList<T> _collection) {
            return _collection.Shuffle();
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private static int GetRandomProbability(List<float> _buffer, float _sum, bool _ignoreZeroProba) {
            // 1. Generate a random value using the sum of all probabilities.
            // 2. Iterate over all elements, and select the first one with a probability below the generated number.
            float _minValue = _ignoreZeroProba ? 0f : MinRandomValue;

            if (ApproximatelyZero(_sum))
                return -1;

            float _random = Random.Range(_minValue, _sum);
            int _count    = _buffer.Count;

            _sum = 0f;

            for (int i = 0; i < _count; i++) {
                _sum += _buffer[i];
                if (_random <= _sum) {
                    return i;
                }
            }

            return -1;
        }
        #endregion
    }
}
