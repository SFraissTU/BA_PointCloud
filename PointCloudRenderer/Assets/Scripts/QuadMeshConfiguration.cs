using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class QuadMeshConfiguration : MeshConfiguration
{
    public float pointRadius;
    public bool renderCircles;

    private Material material;

    public void Start()
    {
        /*material = Resources.Load("Materials/QuadMaterial", typeof(Material)) as Material;
        material.SetFloat("Point Size", pointRadius);
        material.SetInt("Circles", renderCircles ? 1 : 0);
        Rect screen = GameObject.Find("Main Camera").GetComponent<Camera>().pixelRect;
        material.SetInt("Screen Width", (int)screen.width);
        material.SetInt("Screen Height", (int)screen.height);*/
        material = new Material(Shader.Find("Custom/QuadShader"));
        material.SetFloat("_PointSize", pointRadius);
        material.SetInt("_Circles", renderCircles ? 1 : 0);
        Rect screen = GameObject.Find("Main Camera").GetComponent<Camera>().pixelRect;
        material.SetInt("_ScreenWidth", (int)screen.width);
        material.SetInt("_ScreenHeight", (int)screen.height);
    }

    public override GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData)
    {
        GameObject gameObject = new GameObject(name);

        Mesh mesh = new Mesh();

        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.material = material;

        Vector3[] newVertexBuffer = new Vector3[vertexData.Length * 4];
        Color[] newColorBuffer = new Color[colorData.Length * 4];
        Vector2[] offsetBuffer = new Vector2[vertexData.Length * 4];
        int[] indecies = new int[vertexData.Length * 4];
        for (int i = 0; i < vertexData.Length; ++i)
        {
            int startindex = i * 4;
            newVertexBuffer[startindex] = newVertexBuffer[startindex + 1] = newVertexBuffer[startindex + 2] = newVertexBuffer[startindex + 3] = vertexData[i];
            offsetBuffer[startindex + 0] = new Vector2(-1.0f, +1.0f);
            offsetBuffer[startindex + 1] = new Vector2(+1.0f, +1.0f);
            offsetBuffer[startindex + 2] = new Vector2(+1.0f, -1.0f);
            offsetBuffer[startindex + 3] = new Vector2(-1.0f, -1.0f);
            newColorBuffer[startindex] = newColorBuffer[startindex + 1] = newColorBuffer[startindex + 2] = newColorBuffer[startindex + 3] = colorData[i];
            indecies[startindex] = startindex;
            indecies[startindex + 1] = startindex + 1;
            indecies[startindex + 2] = startindex + 2;
            indecies[startindex + 3] = startindex + 3;
        }
        mesh.vertices = newVertexBuffer;
        mesh.colors = newColorBuffer;
        mesh.uv = offsetBuffer;
        mesh.SetIndices(indecies, MeshTopology.Quads, 0);
        return gameObject;
    }

    public override int GetMaximumPointsPerMesh()
    {
        return 16250;
    }
}