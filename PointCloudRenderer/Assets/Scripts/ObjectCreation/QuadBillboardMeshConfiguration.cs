using System;
using CloudData;
using UnityEngine;

namespace ObjectCreation {
    /* Renders every point as a 1px-Point
     */
    class QuadBillboardMeshConfiguration : MeshConfiguration {
        //Size of the quad/circle
        public float pointRadius = 10;
        //wether the quads should be rendered as circles or not
        public bool renderCircles = false;

        private Material material;

        private Camera userCamera;

        public void Start() {
            material = new Material(Shader.Find("Custom/QuadBillboardShader"));
            material.SetFloat("_PointSize", pointRadius);
            material.SetInt("_Circles", renderCircles ? 1 : 0);
            userCamera = Camera.main;
        }

        public void Update() {
            material.SetVector("_CameraPos", userCamera.transform.position);
            System.Random r = new System.Random();
            material.SetFloat("_Color", (float)r.NextDouble());
        }

        public override GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox) {
            GameObject gameObject = new GameObject(name);

            Mesh mesh = new Mesh();

            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.material = material;

            int[] indecies = new int[vertexData.Length];
            for (int i = 0; i < vertexData.Length; ++i) {
                indecies[i] = i;
            }
            mesh.vertices = vertexData;
            mesh.colors = colorData;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);

            //Set Translation
            gameObject.transform.Translate(boundingBox.Min().ToFloatVector());

            return gameObject;
        }

        public override int GetMaximumPointsPerMesh() {
            return 65000;
        }

        public override void RemoveGameObject(GameObject gameObject, out Vector3[] vertexData, out Color[] colorData) {
            Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
            vertexData = mesh.vertices;
            colorData = mesh.colors;
            Destroy(gameObject);
        }
    }
}