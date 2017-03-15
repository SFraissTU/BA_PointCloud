using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
