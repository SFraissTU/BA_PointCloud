using System;
using CloudData;
using UnityEngine;

namespace ObjectCreation
{
    /* Renders every point as a quad or a circle
     */
    class QuadMeshConfiguration : MeshConfiguration
    {
        //Size of the quad/circle
        public float pointRadius = 10;
        //wether the quads should be rendered as circles or not
        public bool renderCircles = false;

        private Material material;

        private GameObjectCache goCache;

        public void Start()
        {
            material = new Material(Shader.Find("Custom/QuadShader"));
            material.SetFloat("_PointSize", pointRadius);
            material.SetInt("_Circles", renderCircles ? 1 : 0);
            Rect screen = Camera.main.pixelRect;
            material.SetInt("_ScreenWidth", (int)screen.width);
            material.SetInt("_ScreenHeight", (int)screen.height);
            goCache = new GameObjectCache();
        }

        public override GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox)
        {
            //GameObject gameObject = new GameObject(name);
            GameObject gameObject;
            bool reused = goCache.RequestGameObject(name, out gameObject);

            Mesh mesh = new Mesh();

            MeshFilter filter;
            if (reused) {
                filter = gameObject.GetComponent<MeshFilter>();
            } else {
                filter = gameObject.AddComponent<MeshFilter>();
            }
            filter.mesh = mesh;
            if (!reused) {
                MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.material = material;
            }

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

            //Set Translation
            gameObject.transform.Translate(boundingBox.Min().ToFloatVector());

            return gameObject;
        }

        public override int GetMaximumPointsPerMesh()
        {
            return 16250;
        }

        public override void RemoveGameObject(GameObject gameObject) {
            //Destroy(gameObject);
            gameObject.GetComponent<MeshFilter>().mesh = null;
            gameObject.transform.position = new Vector3(0, 0, 0);
            goCache.RecycleGameObject(gameObject);
        }
    }
}