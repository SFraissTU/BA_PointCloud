using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class BoundingBox
{
    public float lx;
    public float ly;
    public float lz;
    public float ux;
    public float uy;
    public float uz;

    public BoundingBox(float lx, float ly, float lz, float ux, float uy, float uz)
    {
        this.lx = lx;
        this.ly = ly;
        this.lz = lz;
        this.ux = ux;
        this.uy = uy;
        this.uz = uz;
    }

    public BoundingBox(Vector3 min, Vector3 max)
    {
        lx = min.x;
        ly = min.y;
        lz = min.z;
        ux = max.x;
        uy = max.y;
        uz = max.z;
    }

    public Vector3 Size()
    {
        return new Vector3(ux - lx, uy - ly, uz - lz);
    }

    public Vector3 Min()
    {
        return new Vector3(lx, ly, lz);
    }

    public Vector3 Max()
    {
        return new Vector3(ux, uy, uz);
    }
}
