using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
