using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A pointcloud with the maixmum of 65000 points rendered with the Point-Primitive. Material and PointList are given via the properties of the super class.
 */
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PointPointCloud : PointCloud {

    private static Material material = null;

    void Start()
    {
        if (material == null)
        {
            material = Resources.Load("Materials/PointPointMaterial", typeof(Material)) as Material;
        }

        Mesh mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.material = material;

        Vector3[] points = new Vector3[PointList.Count];
        int[] indecies = new int[PointList.Count];
        Color[] colors = new Color[PointList.Count];
        for (int i = 0; i < PointList.Count; ++i)
        {
            points[i] = new Vector3(PointList[i].X, PointList[i].Z, PointList[i].Y);
            colors[i] = new Color(PointList[i].R, PointList[i].G, PointList[i].B);
            indecies[i] = i;
        }
        mesh.vertices = points;
        mesh.colors = colors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
    }
}
