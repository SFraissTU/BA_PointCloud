using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Script for gameobjects containing a pointcloud-Mesh
 */
public abstract class PointCloud : MonoBehaviour {

    private List<PointCloudPoint> pointList;
    private Material material;

    /* The list of points
     */
    public List<PointCloudPoint> PointList
    {
        get
        {
            return pointList;
        }

        set
        {
            pointList = value;
        }
    }
}
