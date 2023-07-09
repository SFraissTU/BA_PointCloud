using System;
using System.Collections.Generic;
using UnityEngine;

namespace BAPointCloudRenderer.CloudData {
    /// <summary>
    /// An Axis-Aligned Bounding Box for a Node. Note that the values of the bounding box are not final, but might be changed.
    /// </summary>
    [Serializable]
    public class BoundingBox {
        /// <summary>
        /// Lower X-Coordinate. Do not set manually after constructing! Use the property instead!
        /// </summary>
        public double lx;
        /// <summary>
        /// Lower Y-Coordinate. Do not set manually after constructing! Use the property instead!
        /// </summary>
        public double ly;
        /// <summary>
        /// Lower Z-Coordinate. Do not set manually after constructing! Use the property instead!
        /// </summary>
        public double lz;
        /// <summary>
        /// Upper X-Coordinate. Do not set manually after constructing! Use the property instead!
        /// </summary>
        public double ux;
        /// <summary>
        /// Upper Y-Coordinate. Do not set manually after constructing! Use the property instead!
        /// </summary>
        public double uy;
        /// <summary>
        /// Upper Z-Coordinate. Do not set manually after constructing! Use the property instead!
        /// </summary>
        public double uz;

        //Bounds-Object (Unity-Float-Bounding-Box, used in culling)
        private Bounds bounds;

        public BoundingBox() { }

        /// <summary>
        /// Creates a new Bounding Box with the given parameters
        /// </summary>
        public BoundingBox(double lx, double ly, double lz, double ux, double uy, double uz) {
            this.lx = lx;
            this.ly = ly;
            this.lz = lz;
            this.ux = ux;
            this.uy = uy;
            this.uz = uz;
            bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
        }

        /// <summary>
        /// Creates a new Bounding Box with the given Parameters
        /// </summary>
        /// <param name="min">Vector containing lx, ly and lz</param>
        /// <param name="max">Vector containing ux, uy and uz</param>
        public BoundingBox(Vector3d min, Vector3d max) {
            lx = min.x;
            ly = min.y;
            lz = min.z;
            ux = max.x;
            uy = max.y;
            uz = max.z;
            bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
        }

        public void Init() {
            bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
        }

        /// <summary>
        /// Switches the Y and Z coordinates of the bounding box. This might be neccessary because of different coordinate systems
        /// </summary>
        public void SwitchYZ() {
            double temp = ly;
            ly = lz;
            lz = temp;
            temp = uy;
            uy = uz;
            uz = temp;
            bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
        }
        
        /// <summary>
        /// Moves the boxes, so its center is in the origin
        /// </summary>
        public void MoveToOrigin() {
            Vector3d size = Size();
            Vector3d newMin = (size / -2);
            lx = newMin.x;
            ly = newMin.y;
            lz = newMin.z;
            ux = lx + size.x;
            uy = ly + size.y;
            uz = lz + size.z;
            bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
        }

        /// <summary>
        /// Moves the box along the given vector
        /// </summary>
        public void MoveAlong(Vector3d vector) {
            lx += vector.x;
            ly += vector.y;
            lz += vector.z;
            ux += vector.x;
            uy += vector.y;
            uz += vector.z;
            bounds = new Bounds(Center().ToFloatVector(), bounds.size);
        }
        
        /// <summary>
        /// Returns the radius of the circumscribed sphere (half the length of the diagonal)
        /// </summary>
        public double Radius() {
            return Size().Length() / 2;
        }
        
        /// <summary>
        /// Returns the width (x-length), length(y-length) and height (z-length) of the box
        /// </summary>
        public Vector3d Size() {
            return new Vector3d(ux - lx, uy - ly, uz - lz);
        }

        /// <summary>
        /// Returns the lowest corner of the bounding box
        /// </summary>
        public Vector3d Min() {
            return new Vector3d(lx, ly, lz);
        }

        /// <summary>
        /// Returns the highest corner of the bounding box
        /// </summary>
        public Vector3d Max() {
            return new Vector3d(ux, uy, uz);
        }

        /// <summary>
        /// Returns the center of the box
        /// </summary>
        public Vector3d Center() {
            return new Vector3d((ux + lx) / 2, (uy + ly) / 2, (uz + lz) / 2);
        }
        
        /// <summary>
        /// Returns the Bounds-Object (Unity-Class for BoundingBoxes)
        /// </summary>
        public Bounds GetBoundsObject() {
            return bounds;
        }

        public override string ToString() {
            return "BoundingBox: [" + lx + "/" + ly + "/" + lz + ";" + ux + "/" + uy + "/" + uz + "]";
        }

        /// <summary>
        /// Lower X-Value
        /// </summary>
        public double Lx {
            get {
                return lx;
            }

            set {
                lx = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }

        /// <summary>
        /// Lower Y-Value
        /// </summary>
        public double Ly {
            get {
                return ly;
            }

            set {
                ly = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }

        /// <summary>
        /// Lower Z-Value
        /// </summary>
        public double Lz {
            get {
                return lz;
            }

            set {
                lz = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }

        /// <summary>
        /// Upper X-Value
        /// </summary>
        public double Ux {
            get {
                return ux;
            }

            set {
                ux = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }

        /// <summary>
        /// Upper Y-Value
        /// </summary>
        public double Uy {
            get {
                return uy;
            }

            set {
                uy = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }

        /// <summary>
        /// Upper Z-Value
        /// </summary>
        public double Uz {
            get {
                return uz;
            }

            set {
                uz = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }

        public BoundingBox Clone()
        {
            return new BoundingBox(lx, ly, lz, ux, uy, uz);
        }
    }

    [Serializable]
    public class BoundingBoxV2
    {
        /// <summary>
        /// Alternative representation of lx, ly, lz (use Init to convert)
        /// </summary>
        public List<double> min;
        /// <summary>
        /// Alternative representation of ux, uy, uz (use Init to convert)
        /// </summary>
        public List<double> max;
    }

}
