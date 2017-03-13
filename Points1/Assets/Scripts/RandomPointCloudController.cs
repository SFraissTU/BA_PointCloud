using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script creates a random point cloud in the shape of a rectangle.
 * This was used as a first test to see if everything is working.
 * Pressing space leads to the cloud regenerating itself to see how fast replacement of points is possible.
 * 
 * Tutorial: http://www.kamend.com/2014/05/rendering-a-point-cloud-inside-unity/
 */
 

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RandomPointCloudController : MonoBehaviour {

    private Mesh mesh;
    public int numPoints = 10000;

	// Use this for initialization
	void Start () {
        mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;
        CreateMesh();
	}

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            CreateMesh();
        }
    }

    void CreateMesh()
    {
        Vector3[] points = new Vector3[numPoints * 4];
        int[] indecies = new int[numPoints * 4];
        Color[] colors = new Color[numPoints * 4];
        Vector2[] offset = new Vector2[numPoints * 4];
        float offsetlen = 1f; //set in the shader via uniform pointsize
        for (int i = 0; i < numPoints; ++i)
        {
            int startindex = i * 4;
            points[startindex] = points[startindex+1] = points[startindex+2] = points[startindex+3] = new Vector3(Random.Range(-10.0f, 10.0f), 0, Random.Range(-10.0f, 10.0f));
            offset[startindex + 0] = new Vector2(-offsetlen, +offsetlen);
            offset[startindex + 1] = new Vector2(+offsetlen, +offsetlen);
            offset[startindex + 2] = new Vector2(+offsetlen, -offsetlen);
            offset[startindex + 3] = new Vector2(-offsetlen, -offsetlen);
            colors[startindex] = colors[startindex+1] = colors[startindex+2] = colors[startindex + 3] = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            indecies[startindex] = startindex;
            indecies[startindex+1] = startindex+1;
            indecies[startindex+2] = startindex+2;
            indecies[startindex+3] = startindex+3;
        }
        mesh.vertices = points;
        mesh.colors = colors;
        mesh.uv = offset;
        mesh.SetIndices(indecies, MeshTopology.Quads, 0);
    }
	
}
