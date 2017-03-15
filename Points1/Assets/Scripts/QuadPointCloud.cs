using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* A pointcloud with the maixmum of 16250 points rendered with the Quad-Primitive. Material and PointList are given via the properties of the super class.
 */
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class QuadPointCloud : PointCloud
{
    private static Material material = null;

    void Start()
    {
        if (material == null)
        {
            material = Resources.Load("Materials/QuadPointMaterial", typeof(Material)) as Material;
            Rect screen = GameObject.Find("Main Camera").GetComponent<Camera>().pixelRect;
            material.SetInt("_ScreenWidth", (int)screen.width);
            material.SetInt("_ScreenHeight", (int)screen.height);
        }

        Mesh mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.material = material;

        Vector3[] points = new Vector3[PointList.Count * 4];
        int[] indecies = new int[PointList.Count * 4];
        Color[] colors = new Color[PointList.Count * 4];
        Vector2[] offset = new Vector2[PointList.Count * 4];
        float offsetlen = 1f; //set in the shader via uniform pointsize
        for (int i = 0; i < PointList.Count; ++i)
        {
            int startindex = i * 4;
            points[startindex] = points[startindex + 1] = points[startindex + 2] = points[startindex + 3] = new Vector3(PointList[i].X, PointList[i].Z, PointList[i].Y);
            offset[startindex + 0] = new Vector2(-offsetlen, +offsetlen);
            offset[startindex + 1] = new Vector2(+offsetlen, +offsetlen);
            offset[startindex + 2] = new Vector2(+offsetlen, -offsetlen);
            offset[startindex + 3] = new Vector2(-offsetlen, -offsetlen);
            colors[startindex] = colors[startindex + 1] = colors[startindex + 2] = colors[startindex + 3] = new Color(PointList[i].R, PointList[i].G, PointList[i].B);
            indecies[startindex] = startindex;
            indecies[startindex + 1] = startindex + 1;
            indecies[startindex + 2] = startindex + 2;
            indecies[startindex + 3] = startindex + 3;
        }
        mesh.vertices = points;
        mesh.colors = colors;
        mesh.uv = offset;
        mesh.SetIndices(indecies, MeshTopology.Quads, 0);
        //Debug.Log("Mesh Created");
    }
    
}