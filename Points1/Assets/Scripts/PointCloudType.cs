using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
 * Contains information about the type of PointCloud to use.
 */
public class PointCloudType
{
    /*
     * Available types of PointClouds.
     * This exists as an enum, so one can choose the desired type in the editor
     */
    public enum Types
    {
        /* PointCloud using the POINTS-Primitive. Every vertex is given once to the shaders during each pass */
        PointPointCloud = 1,
        /* PointCloud using the QUADS-Primitive. Every vertex is given four times to the shaders during each pass */
        QuadPointCloud = 2
    }

    public static readonly PointCloudType PointPointCloud = new PointCloudType(65000, typeof(PointPointCloud));
    public static readonly PointCloudType QuadPointCloud = new PointCloudType(16250, typeof(QuadPointCloud));

    /*
     * Returns the PointCloudType-object containing the information about the type from a chosen enum.
     */
    public static PointCloudType getTypeObject(Types en)
    {
        switch (en)
        {
            case Types.PointPointCloud:
                return PointPointCloud;
            case Types.QuadPointCloud:
                return QuadPointCloud;
        }
        return null;
    }

    private int maxPointsPerMesh;
    private Type cloudClass;

    private PointCloudType(int maxPointsPerMesh, Type cloudClass)
    {
        this.maxPointsPerMesh = maxPointsPerMesh;
        this.cloudClass = cloudClass;
    }

    /* The maximum number of points given to mesh (QuadPointCloud can only use a forth of the number of points a PointPointCloud can use)
     */
    public int MaxPointsPerMesh
    {
        get
        {
            return maxPointsPerMesh;
        }
    }

    /* The class which implements this type
     */
    public Type CloudClass
    {
        get
        {
            return cloudClass;
        }
    }
}