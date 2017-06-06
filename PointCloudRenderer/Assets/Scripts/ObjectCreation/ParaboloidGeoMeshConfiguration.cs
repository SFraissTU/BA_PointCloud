using CloudData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ObjectCreation {
    class ParaboloidGeoMeshConfiguration : MeshConfiguration {
        //Size of the quad/circle
        public float pointRadius = 10;
        //wether the quads should be rendered as circles or not
        public bool renderCircles = false;
        public bool worldSize = true;//TODO

        private Material material;
        private GameObjectCache goCache;

        private Camera userCamera;

        public void Start() {
            material = new Material(Shader.Find("Custom/ParaboloidGeoWorldSizeShader"));
            material.SetFloat("_PointSize", pointRadius);
            material.SetInt("_Circles", renderCircles ? 1 : 0);
            userCamera = Camera.main;
            goCache = new GameObjectCache();
        }

        public void Update() {
            material.SetVector("_CameraPos", userCamera.transform.position);
        }

        public override GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox) {
            GameObject gameObject;
            bool reused = goCache.RequestGameObject(name, out gameObject);
            //GameObject gameObject = new GameObject(name);

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

        public override void RemoveGameObject(GameObject gameObject) {
            gameObject.GetComponent<MeshFilter>().mesh = null;
            gameObject.transform.position = new Vector3(0, 0, 0);
            goCache.RecycleGameObject(gameObject);
            //Destroy(gameObject);
        }
    }
}
