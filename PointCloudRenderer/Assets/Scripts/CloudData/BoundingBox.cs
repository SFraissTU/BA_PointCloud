using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CloudData
{
    /* An AABB for a Node.
     */
    [Serializable]
    public class BoundingBox
    {
        //DO NOT SET AFTER INITIALIZING
        //lx, ly, lz: lower coordinates
        public double lx;
        public double ly;
        public double lz;
        //ux, uy, uz: upper coordinates
        public double ux;
        public double uy;
        public double uz;

        //Bounds-Object (Unity-Float-Bounding-Box, used in culling)
        private Bounds bounds;

        public BoundingBox() { }

        public BoundingBox(double lx, double ly, double lz, double ux, double uy, double uz)
        {
            this.lx = lx;
            this.ly = ly;
            this.lz = lz;
            this.ux = ux;
            this.uy = uy;
            this.uz = uz;
            bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
        }

        public BoundingBox(Vector3d min, Vector3d max)
        {
            lx = min.x;
            ly = min.y;
            lz = min.z;
            ux = max.x;
            uy = max.y;
            uz = max.z;
            bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
        }

        public void Init()
        {
            bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
        }

        /* Switches the Y and Z coordinates of the bounding box. This might be neccessary because of different coordinate systems
         */
        public void SwitchYZ()
        {
            double temp = ly;
            ly = lz;
            lz = temp;
            temp = uy;
            uy = uz;
            uz = temp;
            bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
        }

        /* Moves the boxes center to the origin
         */
        public void moveToOrigin()
        {
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

        //Returns the radius of the circumscribed sphere (half the length of the diagonal)
        public double Radius()
        {
            return Size().Length() / 2;
        }

        //Returns the width, length and height of the box
        public Vector3d Size()
        {
            return new Vector3d(ux - lx, uy - ly, uz - lz);
        }

        public Vector3d Min()
        {
            return new Vector3d(lx, ly, lz);
        }

        public Vector3d Max()
        {
            return new Vector3d(ux, uy, uz);
        }

        //Returns the center of the box
        public Vector3d Center()
        {
            return new Vector3d((ux + lx) / 2, (uy + ly) / 2, (uz + lz) / 2);
        }

        //Returns the Bounds-Object (Unity-Class for BoundingBoxes)
        public Bounds GetBoundsObject()
        {
            return bounds;
        }



        public double Lx
        {
            get
            {
                return lx;
            }

            set
            {
                lx = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }

        public double Ly
        {
            get
            {
                return ly;
            }

            set
            {
                ly = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }

        public double Lz
        {
            get
            {
                return lz;
            }

            set
            {
                lz = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }

        public double Ux
        {
            get
            {
                return ux;
            }

            set
            {
                ux = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }

        public double Uy
        {
            get
            {
                return uy;
            }

            set
            {
                uy = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }

        public double Uz
        {
            get
            {
                return uz;
            }

            set
            {
                uz = value;
                bounds = new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
            }
        }
    }

}
