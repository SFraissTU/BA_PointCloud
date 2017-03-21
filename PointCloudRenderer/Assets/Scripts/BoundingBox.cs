using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class BoundingBox
{
    public double lx;
    public double ly;
    public double lz;
    public double ux;
    public double uy;
    public double uz;

    public BoundingBox(double lx, double ly, double lz, double ux, double uy, double uz)
    {
        this.lx = lx;
        this.ly = ly;
        this.lz = lz;
        this.ux = ux;
        this.uy = uy;
        this.uz = uz;
    }

    public BoundingBox(Vector3d min, Vector3d max)
    {
        lx = min.x;
        ly = min.y;
        lz = min.z;
        ux = max.x;
        uy = max.y;
        uz = max.z;
    }

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
}
