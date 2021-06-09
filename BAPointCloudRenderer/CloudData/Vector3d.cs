using System;
using UnityEngine;

namespace BAPointCloudRenderer.CloudData {
    /// <summary>
    /// A vector using double values. The values are final, calculations always create a new vector. However, the x,y,z-values can be changed directly
    /// </summary>
    public class Vector3d {
        public double x, y, z;

        public Vector3d(double x, double y, double z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3d(Vector3 original) {
            this.x = original.x;
            this.y = original.y;
            this.z = original.z;
        }

        /// <summary>
        /// magnitude of this vector
        /// </summary>
        public double Length() {
            return Math.Sqrt(x * x + y*y + z * z);
        }

        /// <summary>
        /// Returns a Unity-Vector with float-values
        /// </summary>
        public Vector3 ToFloatVector() {
            return new Vector3((float)x, (float)y, (float)z);
        }

        /// <summary>
        /// Returns the distance between the points described by this and the other vector
        /// </summary>
        public double Distance(Vector3d other) {
            return (this - other).Length();
        }

        /// <summary>
        /// Normalizes the vector
        /// </summary>
        public Vector3d Normalize() {
            return this / Length();
        }

        /// <summary>
        /// Divides the vector by the given value
        /// </summary>
        public static Vector3d operator /(Vector3d v, double divisor) {
            return new Vector3d(v.x / divisor, v.y / divisor, v.z / divisor);
        }

        /// <summary>
        /// Adds two vectors
        /// </summary>
        public static Vector3d operator +(Vector3d a, Vector3d b) {
            return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        /// <summary>
        /// Subtracts the second vector from the first
        /// </summary>
        public static Vector3d operator -(Vector3d a, Vector3d b) {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3d operator -(Vector3d a)
        {
            return new Vector3d(-a.x, -a.y, -a.z);
        }

        /// <summary>
        /// Calculates the Dot-Product
        /// </summary>
        public static double operator *(Vector3d a, Vector3d b) {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public override string ToString() {
            return "Vector3d [" + x + ", " + y + ", " + z + "]";
        }
    }
}