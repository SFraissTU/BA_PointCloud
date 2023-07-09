using BAPointCloudRenderer.CloudData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BAPointCloudRenderer.ObjectCreation {

    /// <summary>
    /// What kind of interpolation to use
    /// </summary>
    enum InterpolationMode {
        /// <summary>
        /// No interpolation
        /// </summary>
        OFF,
        /// <summary>
        /// Exact paraboloids
        /// </summary>
        FRAGMENT_PARA,
        /// <summary>
        /// Exact cones
        /// </summary>
        FRAGMENT_CONE,
        /// <summary>
        /// Paraboloids approximated with 8 triangles
        /// </summary>
        GEOMETRY0,
        /// <summary>
        /// Paraboloids approximated with 16 triangles
        /// </summary>
        GEOMETRY1,
        /// <summary>
        /// Paraboloids approximated with 32 triangles
        /// </summary>
        GEOMETRY2,
        /// <summary>
        /// Paraboloids approximated with 48 triangles
        /// </summary>
        GEOMETRY3
    }

    /// <summary>
    /// Geometry Shader Quad Rendering, as described in the Bachelor Thesis in chapter 3.3.3.
    /// Creates a screen facing square or circle for each point using the Geometry Shader.
    /// Also supports various interpolation modes (see Thesis chapter 3.3.4 "Interpolation").
    /// This configuration also supports changes of the parameters while the application is running. Just change the parameters and check the checkbox "reload".
    /// </summary>
    [Obsolete("This class is for experimental purposes only. For practical usage, please use DefaultMeshConfiguration")]
    class GeoQuadMeshConfiguration : MeshConfiguration {

        /// <summary>
        /// Radius of the point (in pixel or world units, depending on variable screenSize)
        /// </summary>
        public float pointRadius = 10;
        /// <summary>
        /// Whether the quads should be rendered as circles (true) or as squares (false)
        /// </summary>
        public bool renderCircles = true;
        /// <summary>
        /// True, if pointRadius should be interpreted as pixels, false if it should be interpreted as world units
        /// </summary>
        public bool screenSize = true;
        /// <summary>
        /// Wether and how to use interpolation
        /// </summary>
        public InterpolationMode interpolation = InterpolationMode.OFF;
        /// <summary>
        /// If changing the parameters should be possible during execution, this variable has to be set to true in the beginning! Later changes to this variable will not change anything
        /// </summary>
        public bool reloadingPossible = true;
        /// <summary>
        /// Set this to true to reload the shaders according to the changed parameters
        /// </summary>
        public bool reload = false;

        private Material material;
        private Camera mainCamera;
        private HashSet<GameObject> gameObjectCollection = null;

        private void LoadShaders() {
            if (interpolation == InterpolationMode.OFF) {
                if (screenSize) {
                    material = new Material(Shader.Find("Custom/QuadGeoScreenSizeShader"));
                } else {
                    material = new Material(Shader.Find("Custom/QuadGeoWorldSizeShader"));
                }
            }
            if (interpolation == InterpolationMode.GEOMETRY0 || interpolation == InterpolationMode.GEOMETRY1 || interpolation == InterpolationMode.GEOMETRY2 || interpolation == InterpolationMode.GEOMETRY3) {
                if (screenSize) {
                    material = new Material(Shader.Find("Custom/ParaboloidGeoScreenSizeShader"));
                } else {
                    material = new Material(Shader.Find("Custom/ParaboloidGeoWorldSizeShader"));
                }
                switch (interpolation) {
                    case InterpolationMode.GEOMETRY0:
                        material.SetInt("_Details", 0);
                        break;
                    case InterpolationMode.GEOMETRY1:
                        material.SetInt("_Details", 1);
                        break;
                    case InterpolationMode.GEOMETRY2:
                        material.SetInt("_Details", 2);
                        break;
                    case InterpolationMode.GEOMETRY3:
                        material.SetInt("_Details", 3);
                        break;
                }
            } else if (interpolation == InterpolationMode.FRAGMENT_PARA || interpolation == InterpolationMode.FRAGMENT_CONE) {
                if (screenSize) {
                    material = new Material(Shader.Find("Custom/ParaboloidFragScreenSizeShader"));
                } else {
                    material = new Material(Shader.Find("Custom/ParaboloidFragWorldSizeShader"));
                }
                material.SetInt("_Cones", (interpolation == InterpolationMode.FRAGMENT_CONE) ? 1 : 0);
            }
            material.enableInstancing = true;
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
                if (interpolation != InterpolationMode.OFF) {
                    Matrix4x4 invP = (GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, true)).inverse;
                    material.SetMatrix("_InverseProjMatrix", invP);
                    material.SetFloat("_FOV", Mathf.Deg2Rad * mainCamera.fieldOfView);
                }
                Rect screen = mainCamera.pixelRect;
                material.SetInt("_ScreenWidth", (int)screen.width);
                material.SetInt("_ScreenHeight", (int)screen.height);
            }
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

            int[] indecies = new int[vertexData.Length];
            for (int i = 0; i < vertexData.Length; ++i) {
                indecies[i] = i;
            }
            mesh.vertices = vertexData;
            mesh.colors = colorData;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);

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
            if (gameObject != null)
            {
                Destroy(gameObject.GetComponent<MeshFilter>().sharedMesh);
                Destroy(gameObject);
            }
        }
    }
}
