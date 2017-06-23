using CloudData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ObjectCreation {

    enum ParaboloidMode {
        OFF,
        FRAGMENT,
        GEOMETRY1,
        GEOMETRY2,
        GEOMETRY3
    }

    class GeoQuadMeshConfiguration : MeshConfiguration {

        //Size of the quad/circle
        public float pointRadius = 10;
        //wether the quads should be rendered as circles or not
        public bool renderCircles = true;
        //size in screen or world coordinates
        public bool screenSize = true;
        //Wether and how to use paraboloids
        public ParaboloidMode paraboloid = ParaboloidMode.OFF;
        //If changing the parameters should be possible during execution, this variable has to be set to true in the beginning! Later changes will not change anything
        public bool reloadingPossible = true;
        //Set this to true to reload the shaders according to the changed parameters
        public bool reload = false;

        private Material material;
        private Camera mainCamera;
        private HashSet<GameObject> gameObjectCollection = null;

        private void LoadShaders() {
            if (paraboloid == ParaboloidMode.OFF) {
                if (screenSize) {
                    material = new Material(Shader.Find("Custom/QuadGeoScreenSizeShader"));
                } else {
                    material = new Material(Shader.Find("Custom/QuadGeoWorldSizeShader"));
                }
            }
            if (paraboloid == ParaboloidMode.GEOMETRY1 || paraboloid == ParaboloidMode.GEOMETRY2 || paraboloid == ParaboloidMode.GEOMETRY3) {
                if (screenSize) {
                    material = new Material(Shader.Find("Custom/ParaboloidGeoScreenSizeShader"));
                } else {
                    material = new Material(Shader.Find("Custom/ParaboloidGeoWorldSizeShader"));
                }
                switch (paraboloid) {
                    case ParaboloidMode.GEOMETRY1:
                        material.SetInt("_Details", 1);
                        break;
                    case ParaboloidMode.GEOMETRY2:
                        material.SetInt("_Details", 2);
                        break;
                    case ParaboloidMode.GEOMETRY3:
                        material.SetInt("_Details", 3);
                        break;
                }
            } else if (paraboloid == ParaboloidMode.FRAGMENT) {
                if (screenSize) {
                    material = new Material(Shader.Find("Custom/ParaboloidFragScreenSizeShader"));
                } else {
                    material = new Material(Shader.Find("Custom/ParaboloidFragWorldSizeShader"));
                }
            }
            material.SetFloat("_PointSize", pointRadius);
            material.SetInt("_Circles", renderCircles ? 1 : 0);
        }

        public void Start() {
            if (reloadingPossible) {
                gameObjectCollection = new HashSet<GameObject>();
            }
            LoadShaders();
            mainCamera = Camera.main;
        }

        public void Update() {
            if (reload && gameObjectCollection != null) {
                LoadShaders();
                foreach (GameObject go in gameObjectCollection) {
                    go.GetComponent<MeshRenderer>().material = material;
                }
                reload = false;
            }
            if (screenSize) {
                if (paraboloid != ParaboloidMode.OFF) {
                    Matrix4x4 invP = (GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, true)).inverse;
                    material.SetMatrix("_InverseProjMatrix", invP);
                    material.SetFloat("_FOV", Mathf.Deg2Rad * mainCamera.fieldOfView);
                }
                Rect screen = Camera.main.pixelRect;
                material.SetInt("_ScreenWidth", (int)screen.width);
                material.SetInt("_ScreenHeight", (int)screen.height);
            }
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

            if (gameObjectCollection != null) {
                gameObjectCollection.Add(gameObject);
            }

            return gameObject;
        }

        public override int GetMaximumPointsPerMesh() {
            return 65000;
        }

        public override void RemoveGameObject(GameObject gameObject) {
            if (gameObjectCollection != null) {
                gameObjectCollection.Remove(gameObject);
            }
            Destroy(gameObject.GetComponent<MeshFilter>().sharedMesh);
            Destroy(gameObject);
        }
    }
}
