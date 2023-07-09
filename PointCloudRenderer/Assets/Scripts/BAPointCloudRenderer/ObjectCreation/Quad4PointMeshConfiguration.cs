using System;
using BAPointCloudRenderer.CloudData;
using UnityEngine;

namespace BAPointCloudRenderer.ObjectCreation
{
    /// <summary>
    /// 4-Vertex Quad Rendering, as described in the thesis in chapter 3.3.2.
    /// Creates sceen faced squares or circles for each point with a given size in pixels, by passing each vertex 4 times to the GPU
    /// </summary>
    [Obsolete("This class is for experimental purposes only. For practical usage, please use DefaultMeshConfiguration")]
    class Quad4PointMeshConfiguration : MeshConfiguration {
        /// <summary>
        /// Radius in pixel
        /// </summary>
        public float pointRadius = 10;
        /// <summary>
        /// Whether the quads should be rendered as circles (true) or as squares (false)
        /// </summary>
        public bool renderCircles = false;

        private Material material;

        public void Start() {
            material = new Material(Shader.Find("Custom/Quad4PointScreenSizeShader"));
            material.enableInstancing = true;
            material.SetFloat("_PointSize", pointRadius);
            material.SetInt("_Circles", renderCircles ? 1 : 0);
            Rect screen = Camera.main.pixelRect;
            material.SetInt("_ScreenWidth", (int)screen.width);
            material.SetInt("_ScreenHeight", (int)screen.height);
        }

        public override GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox, Transform parent, string version, Vector3d translationV2) {
            GameObject gameObject = new GameObject(name);

            Mesh mesh = new Mesh();

            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.material = material;

            Vector3[] newVertexBuffer = new Vector3[vertexData.Length * 4];
            Color[] newColorBuffer = new Color[colorData.Length * 4];
            Vector2[] offsetBuffer = new Vector2[vertexData.Length * 4];
            int[] indecies = new int[vertexData.Length * 4];
            for (int i = 0; i < vertexData.Length; ++i) {
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

            //Set Translation
            if (version == "2.0")
            {
                // 20230125: potree v2 vertices have absolute coordinates,
                // hence all gameobjects need to reside at Vector.Zero.
                // And: the position must be set after parenthood has been granted.
                //gameObject.transform.Translate(boundingBox.Min().ToFloatVector());
                gameObject.transform.SetParent(parent, false);
                gameObject.transform.localPosition = translationV2.ToFloatVector();
            }
            else
            {
                gameObject.transform.Translate(boundingBox.Min().ToFloatVector());
                gameObject.transform.SetParent(parent, false);
            }

            return gameObject;
        }

        public override int GetMaximumPointsPerMesh() {
            return 16250;
        }

        public override void RemoveGameObject(GameObject gameObject) {
            if (gameObject != null)
            {
                Destroy(gameObject.GetComponent<MeshFilter>().sharedMesh);
                Destroy(gameObject);
            }
        }
    }
}