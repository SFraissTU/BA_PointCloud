using System;
using CloudData;
using UnityEngine;

namespace ObjectCreation
{
    /// <summary>
    /// Renders every point as a single pixel using the points primitive. As described in the Bachelor Thesis in chapter 3.3.1 "Single-Pixel Point Rendering".
    /// </summary>
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

            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;
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

        public override void RemoveGameObject(GameObject gameObject) {
            Destroy(gameObject.GetComponent<MeshFilter>().sharedMesh);
            Destroy(gameObject);
        }
    }
}