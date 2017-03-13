using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* One single point inside a pointcloud
 */
public class PointCloudPoint
{
    //Attribute markieren. Strg+. -> Generate Constructor
    //Strg+R, Strg+E -> Properties erzeugen
    private float x;
    private float y;
    private float z;
    private float r;
    private float g;
    private float b;

    public PointCloudPoint(float x, float y, float z, float r, float g, float b)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.R = r;
        this.G = g;
        this.B = b;
    }




    public float X
    {
        get
        {
            return x;
        }

        set
        {
            x = value;
        }
    }

    public float Y
    {
        get
        {
            return y;
        }

        set
        {
            y = value;
        }
    }

    public float Z
    {
        get
        {
            return z;
        }

        set
        {
            z = value;
        }
    }

    public float R
    {
        get
        {
            return r;
        }

        set
        {
            r = value;
        }
    }

    public float G
    {
        get
        {
            return g;
        }

        set
        {
            g = value;
        }
    }

    public float B
    {
        get
        {
            return b;
        }

        set
        {
            b = value;
        }
    }


}