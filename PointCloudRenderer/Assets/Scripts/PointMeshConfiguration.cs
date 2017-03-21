using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class PointMeshConfiguration : MeshConfiguration
{
    private Material material;

    public void Start()
    {
         material = new Material(Shader.Find("Custom/PointShader"));
    }

    public override GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox)
    {
        GameObject gameObject = new GameObject(name);

        Mesh mesh = new Mesh();

        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.material = material;

        int[] indecies = new int[vertexData.Length];
        for (int i = 0; i < vertexData.Length; ++i)
        {
            indecies[i] = i;
        }
        mesh.vertices = vertexData;
        mesh.colors = colorData;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);

        //Set Translation
        gameObject.transform.Translate(boundingBox.Min().ToFloatVector());

        return gameObject;
    }

    public override int GetMaximumPointsPerMesh()
    {
        return 65000;
    }
}