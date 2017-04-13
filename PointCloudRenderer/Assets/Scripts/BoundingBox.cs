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

    public void SwitchYZ()
    {
        double temp = ly;
        ly = lz;
        lz = temp;
        temp = uy;
        uy = uz;
        uz = temp;
    }

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
    }

    public double Radius()
    {
        return Size().Length() / 2;
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

    public Vector3d Center()
    {
        return new Vector3d((ux + lx) / 2, (uy + ly) / 2, (uz + lz) / 2);
    }

    public Bounds ToBounds()
    {
        return new Bounds(Center().ToFloatVector(), Size().ToFloatVector());
    }
}
