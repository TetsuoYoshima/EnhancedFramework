// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Geometry related static utility class.
    /// </summary>
    public static class EnhancedGeometryUtility {
        #region Content
        /// <summary>
        /// Determines if a point is inside of a polygon on the XZ plane (the y value being ignored).
        /// </summary>
        /// <param name="_point">The <see cref="Vector3"/> point to test.</param>
        /// <param name="_vertices">The vertices that make up the bounds of the polygon.</param>
        /// <returns>True if the point is inside the polygon, false otherwise</returns>
        public static bool PointInPolygon(Vector3 _point, List<Vector3> _vertices) {

            // Sanity check - not enough bounds vertices = nothing to be inside of.
            int _count = _vertices.Count;
            if (_count < 3) {
                return false;
            }

            // Check how many lines this test point collides with going in one direction.
            // Odd = Inside, Even = Outside.
            int _collisions = 0;
            int _vertexCounter = 0;
            Vector3 _startPoint = _vertices[_count - 1];

            // We recenter the test point around the origin to simplify the math a bit.
            _startPoint.x -= _point.x;
            _startPoint.z -= _point.z;

            bool _currentSide = false;

            if (!Mathm.ApproximatelyZero(_startPoint.z)) {
                _currentSide = _startPoint.z < 0f;
            } else {

                // We need a definitive side of the horizontal axis to start with (since we need to know when we cross it),
                // so we go backwards through the vertices until we find one that does not lie on the horizontal.
                for (int i = _count - 2; i >= 0; --i) {

                    float _vertZ = _vertices[i].z;
                    _vertZ -= _point.z;

                    if (!Mathm.ApproximatelyZero(_vertZ)) {
                        _currentSide = _vertZ < 0f;
                        break;
                    }
                }
            }

            while (_vertexCounter < _count) {

                Vector3 _endPoint = _vertices[_vertexCounter];
                _endPoint.x -= _point.x;
                _endPoint.z -= _point.z;

                Vector3 _startToEnd = _endPoint - _startPoint;
                float _edgeSqrMagnitude = _startToEnd.sqrMagnitude;

                // This line goes through the start point, which means the point is on an edge of the polygon.
                if (Mathm.ApproximatelyZero((_startToEnd.x * _endPoint.z) - (_startToEnd.z * _endPoint.x)) &&
                    (_startPoint.sqrMagnitude <= _edgeSqrMagnitude) && (_endPoint.sqrMagnitude <= _edgeSqrMagnitude)) {

                    return true;
                }

                // Ignore lines that end at the horizontal axis.
                if (!Mathm.ApproximatelyZero(_endPoint.z)) {

                    bool _nextSide = _endPoint.z < 0f;
                    if (_nextSide != _currentSide) {
                        _currentSide = _nextSide;

                        // If we've crossed the horizontal, check if the origin is to the left of the line.
                        if ((_startPoint.x * _endPoint.z - _startPoint.z * _endPoint.x) / -(_startPoint.z - _endPoint.z) > 0) {
                            _collisions++;
                        }
                    }
                }

                _startPoint = _endPoint;
                _vertexCounter++;
            }

            return (_collisions % 2) > 0;
        }

        /// <summary>
        /// Determines if a point is inside of a polygon.
        /// </summary>
        /// <param name="_point">The <see cref="Vector2"/> point to test.</param>
        /// <param name="_vertices">The vertices that make up the bounds of the polygon.</param>
        /// <returns>True if the point is inside the polygon, false otherwise</returns>
        public static bool PointInPolygon(Vector2 _point, List<Vector2> _vertices) {

            // Sanity check - not enough bounds vertices = nothing to be inside of.
            int _count = _vertices.Count;
            if (_count < 3) {
                return false;
            }

            // Check how many lines this test point collides with going in one direction.
            // Odd = Inside, Even = Outside.
            int _collisions = 0;
            int _vertexCounter = 0;
            Vector2 _startPoint = _vertices[_count - 1];

            // We recenter the test point around the origin to simplify the math a bit.
            _startPoint.x -= _point.x;
            _startPoint.y -= _point.y;

            bool _currentSide = false;

            if (!Mathm.ApproximatelyZero(_startPoint.y)) {
                _currentSide = _startPoint.y < 0f;
            } else {

                // We need a definitive side of the horizontal axis to start with (since we need to know when we cross it),
                // so we go backwards through the vertices until we find one that does not lie on the horizontal.
                for (int i = _count - 2; i >= 0; --i) {

                    float _vertZ = _vertices[i].y;
                    _vertZ -= _point.y;

                    if (!Mathm.ApproximatelyZero(_vertZ)) {
                        _currentSide = _vertZ < 0f;
                        break;
                    }
                }
            }

            while (_vertexCounter < _count) {

                Vector2 _endPoint = _vertices[_vertexCounter];
                _endPoint.x -= _point.x;
                _endPoint.y -= _point.y;

                Vector2 _startToEnd = _endPoint - _startPoint;
                float _edgeSqrMagnitude = _startToEnd.sqrMagnitude;

                // This line goes through the start point, which means the point is on an edge of the polygon.
                if (Mathm.ApproximatelyZero((_startToEnd.x * _endPoint.y) - (_startToEnd.y * _endPoint.x)) &&
                    (_startPoint.sqrMagnitude <= _edgeSqrMagnitude) && (_endPoint.sqrMagnitude <= _edgeSqrMagnitude)) {

                    return true;
                }

                // Ignore lines that end at the horizontal axis.
                if (!Mathm.ApproximatelyZero(_endPoint.y)) {

                    bool _nextSide = _endPoint.y < 0f;
                    if (_nextSide != _currentSide) {
                        _currentSide = _nextSide;

                        // If we've crossed the horizontal, check if the origin is to the left of the line.
                        if (((_startPoint.x * _endPoint.y) - (_startPoint.y * _endPoint.x)) / -(_startPoint.y - _endPoint.y) > 0f) {
                            _collisions++;
                        }
                    }
                }

                _startPoint = _endPoint;
                _vertexCounter++;
            }

            return (_collisions % 2) > 0;
        }

        /// <summary>
        /// Determines if a point is inside of a convex polygon and lies on the surface.
        /// </summary>
        /// <param name="_point">The <see cref="Vector3"/> point to test.</param>
        /// <param name="_vertices">The vertices that make up the bounds of the polygon (these should be convex and coplanar but can have any normal).</param>
        /// <returns>True if the point is inside the polygon and coplanar, false otherwise.</returns>
        public static bool PointInPolygon3D(Vector3 _point, List<Vector3> _vertices) {

            // Not enough bounds vertices = nothing to be inside of.
            int _count = _vertices.Count;
            if (_count < 3) {
                return false;
            }

            // Compute the sum of the angles between the test point and each pair of edge points.
            double _angleSum = 0d;
            for (int i = 0; i < _count; i++) {

                Vector3 _toA = _vertices[i] - _point;
                Vector3 _toB = _vertices[(i + 1) % _count] - _point;
                float _distance = _toA.sqrMagnitude * _toB.sqrMagnitude; // Use sqrMagnitude, take sqrt of result later.

                // On a vertex
                if (_distance <= Mathf.Epsilon) {
                    return true;
                }

                double _cosTheta = Vector3.Dot(_toA, _toB) / Mathf.Sqrt(_distance);
                double _angle    = Math.Acos(_cosTheta);

                _angleSum += _angle;
            }

            // The sum will only be 2*PI if the point is on the plane of the polygon and on the interior.
            const float radiansCompareThreshold = 0.01f;
            return Mathf.Abs((float)_angleSum - (Mathf.PI * 2f)) < radiansCompareThreshold;
        }
        #endregion
    }
}
